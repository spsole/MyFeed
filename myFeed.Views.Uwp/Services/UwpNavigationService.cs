using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Implementations;
using myFeed.Views.Uwp.Views;

namespace myFeed.Views.Uwp.Services
{
    public class UwpNavigationService : INavigationService
    {
        private readonly SystemNavigationManager _systemNavigationManager;
        private static readonly IReadOnlyDictionary<Type, Type> Pages = new Dictionary<Type, Type>
        {
            {typeof(SettingsViewModel), typeof(SettingsView)},
            {typeof(ArticleViewModel), typeof(ArticleView)},
            {typeof(SourcesViewModel), typeof(SourcesView)},
            {typeof(SearchViewModel), typeof(SearchView)},
            {typeof(MenuViewModel), typeof(MenuView)},
            {typeof(FaveViewModel), typeof(FaveView)},
            {typeof(FeedViewModel), typeof(FeedView)},
        };
        public IReadOnlyDictionary<Type, object> Icons => new Dictionary<Type, object>
        {
            {typeof(FaveViewModel), Symbol.OutlineStar},
            {typeof(SettingsViewModel), Symbol.Setting},
            {typeof(FeedViewModel), Symbol.PostUpdate},
            {typeof(SourcesViewModel), Symbol.List},
            {typeof(SearchViewModel), Symbol.Zoom}
        };

        public UwpNavigationService()
        {
            _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            _systemNavigationManager.BackRequested += NavigateBack;
        }

        public event EventHandler<Type> Navigated;

        public Task Navigate<T>() => Navigate<T>(UwpViewModelLocator.Current.Resolve<T>());

        public Task Navigate<T>(object instance)
        {
            var viewModelType = typeof(T);
            var rootFrame = (Frame)Window.Current.Content;
            switch (viewModelType.Name)
            {
                case nameof(FeedViewModel):
                case nameof(FaveViewModel):
                case nameof(SearchViewModel):
                case nameof(SourcesViewModel):
                case nameof(SettingsViewModel):
                    NavigateWithViewModel(GetChild<Frame>(rootFrame, 0));
                    break;
                case nameof(ArticleViewModel):
                    NavigateWithViewModel(GetChild<Frame>(rootFrame, 1));
                    break;
                case nameof(MenuViewModel):
                    NavigateWithViewModel(rootFrame);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Task.CompletedTask;

            void NavigateWithViewModel(Frame frame)
            {
                frame.Navigate(Pages[viewModelType], instance);
                frame.DataContext = instance;
                Navigated?.Invoke(this, viewModelType);
                UpdateBackButtonVisibility();
            }
        }

        private void NavigateBack(object sender, BackRequestedEventArgs e)
        {
            var rootObject = (Frame)Window.Current.Content;
            var articleFrame = GetChild<Frame>(rootObject, 1);
            var navigationFrame = articleFrame?.CanGoBack == true ? articleFrame : GetChild<Frame>(rootObject, 0);
            NavigateBackWithViewModel(navigationFrame);

            void NavigateBackWithViewModel(Frame frame)
            {
                frame.GoBack();
                var viewType = frame.CurrentSourcePageType;
                if (!Pages.Values.Contains(viewType)) return;
                var viewModelType = Pages.First(x => x.Value == viewType).Key;
                frame.DataContext = UwpViewModelLocator.Current.Resolve(viewModelType);
                Navigated?.Invoke(this, viewModelType);
                UpdateBackButtonVisibility();
            }
        }

        private void UpdateBackButtonVisibility() =>
            _systemNavigationManager.AppViewBackButtonVisibility =
                GetChild<Frame>(Window.Current.Content, 0)?.CanGoBack == true ||
                GetChild<Frame>(Window.Current.Content, 1)?.CanGoBack == true
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;

        public static T GetChild<T>(DependencyObject root, int depth) where T : DependencyObject
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
