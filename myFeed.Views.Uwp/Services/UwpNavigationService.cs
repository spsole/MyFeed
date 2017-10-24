using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Implementations;
using myFeed.Views.Uwp.Views;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using myFeed.Views.Uwp.Controls;

namespace myFeed.Views.Uwp.Services
{
    public class UwpNavigationService : INavigationService
    {
        private static readonly IReadOnlyDictionary<Type, Type> Pages = new Dictionary<Type, Type>
        {
            {typeof(SettingsViewModel), typeof(SettingsView)},
            {typeof(ArticleViewModel), typeof(ArticleView)},
            {typeof(ChannelsViewModel), typeof(SourcesView)},
            {typeof(SearchViewModel), typeof(SearchView)},
            {typeof(MenuViewModel), typeof(MenuView)},
            {typeof(FaveViewModel), typeof(FaveView)},
            {typeof(FeedViewModel), typeof(FeedView)}
        };
        private static readonly IReadOnlyDictionary<Type, object> Symbols = new Dictionary<Type, object>
        {
            {typeof(FaveViewModel), Symbol.OutlineStar},
            {typeof(SettingsViewModel), Symbol.Setting},
            {typeof(FeedViewModel), Symbol.PostUpdate},
            {typeof(ChannelsViewModel), Symbol.List},
            {typeof(SearchViewModel), Symbol.Zoom}
        };
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IFactoryService _factoryService;

        public UwpNavigationService(
            ICategoriesRepository categoriesRepository,
            IFactoryService factoryService)
        {
            _factoryService = factoryService;
            _categoriesRepository = categoriesRepository;

            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            systemNavigationManager.BackRequested += NavigateBack;

            var page = (Page)((Frame)Window.Current.Content).Content;
            var color = (Color)Application.Current.Resources["SystemChromeLowColor"];
            StatusBar.SetBackgroundColor(page, color);
            StatusBar.SetBackgroundOpacity(page, 1);
        }

        public event EventHandler<Type> Navigated;

        public IReadOnlyDictionary<Type, object> Icons => Symbols;

        public Task Navigate<TViewModel>() where TViewModel : class => Navigate<TViewModel>(Uwp.Current.Resolve<TViewModel>());

        public async Task Navigate<TViewModel>(object arg) where TViewModel : class
        {
            switch (typeof(TViewModel).Name)
            {
                case nameof(FeedViewModel):
                case nameof(FaveViewModel):
                case nameof(SearchViewModel):
                case nameof(ChannelsViewModel):
                case nameof(SettingsViewModel):
                    NavigateFrame(GetChild<Frame>(Window.Current.Content, 0));
                    break;
                case nameof(MenuViewModel):
                    NavigateFrame((Frame)Window.Current.Content);
                    break;
                case nameof(ArticleViewModel) when arg is Guid guid:
                    var article = await _categoriesRepository.GetArticleByIdAsync(guid);
                    if (article == null) return;
                    var viewModel = _factoryService.CreateInstance<ArticleViewModel>(article);
                    if (GetChild<Frame>(Window.Current.Content, 1) == null) await Navigate<FeedViewModel>();
                    await Task.Delay(150);
                    await Navigate<ArticleViewModel>(viewModel);
                    break;
                case nameof(ArticleViewModel):
                    NavigateFrame(GetChild<Frame>(Window.Current.Content, 1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void NavigateFrame(Frame frame)
            {
                frame.Navigate(Pages[typeof(TViewModel)], arg);
                ((Page) frame.Content).DataContext = arg;
                RaiseNavigated(arg);
            }
        }

        private void RaiseNavigated(object instance) => Navigated?.Invoke(this, instance?.GetType());

        private void NavigateBack(object sender, BackRequestedEventArgs e)
        {
            var splitView = GetChild<SwipeableSplitView>(Window.Current.Content, 0);
            if (splitView.IsSwipeablePaneOpen && splitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                splitView.IsSwipeablePaneOpen = false;
                e.Handled = true;
                return;
            }

            var articleFrame = GetChild<Frame>(Window.Current.Content, 1);
            var splitViewFrame = GetChild<Frame>(Window.Current.Content, 0);
            var frame = articleFrame?.CanGoBack == true ? articleFrame
                      : splitViewFrame?.CanGoBack == true ? splitViewFrame : null;
            if (frame == null) return;

            var instance = frame.BackStack.Last().Parameter;
            frame.GoBack();
            e.Handled = true;
            frame.ForwardStack.Clear();
            if (instance == null) return;

            ((Page)frame.Content).DataContext = instance is ArticleViewModel 
                ? instance : Uwp.Current.Resolve(instance.GetType());
            RaiseNavigated(instance);
        }

        private static T GetChild<T>(DependencyObject root, int depth) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var x = 0; x < childrenCount; x++)
            {
                var child = VisualTreeHelper.GetChild(root, x);
                if (child is T ths && depth-- == 0) return ths;
                var frame = GetChild<T>(child, depth);
                if (frame != null) return frame;
            }
            return null;
        }
    }
}
