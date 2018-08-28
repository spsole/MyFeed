using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using DryIoc;
using DryIocAttributes;
using MyFeed.Platform;
using MyFeed.Uwp.Controls;
using MyFeed.Uwp.Views;
using MyFeed.ViewModels;

namespace MyFeed.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(INavigationService))]
    public sealed class UwpNavigationService : INavigationService
    {
        private readonly IResolver _resolver;
        private readonly Subject<Type> _navigated = new Subject<Type>();
        private readonly IReadOnlyDictionary<Type, Type> _views = new Dictionary<Type, Type>
        {
            {typeof(FeedItemFullViewModel), typeof(ArticleView)},
            {typeof(SettingViewModel), typeof(SettingView)},
            {typeof(ChannelViewModel), typeof(ChannelView)},
            {typeof(SearchViewModel), typeof(SearchView)},
            {typeof(MenuViewModel), typeof(MenuView)},
            {typeof(FaveViewModel), typeof(FaveView)},
            {typeof(FeedViewModel), typeof(FeedView)}
        };

        public UwpNavigationService(IResolver resolver)
        {
            _resolver = resolver;
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            systemNavigationManager.BackRequested += NavigateBack;
        }

        public IObservable<Type> Navigated => _navigated;

        public Type CurrentViewModelType { get; private set; }
        
        public Task Navigate<TViewModel>() => NavigateTo(_resolver.Resolve<TViewModel>());
        
        public Task NavigateTo<TViewModel>(TViewModel viewModel)
        {
            var viewModelType = typeof(TViewModel);
            var depth = viewModelType.Name == nameof(FeedItemFullViewModel) ? 1 : 0;
            var navigationFrame = GetChild<Frame>(Window.Current.Content, depth);
            navigationFrame.Navigate(_views[viewModelType], viewModel);

            var page = (Page)navigationFrame.Content ?? throw new NullReferenceException();
            page.DataContext = viewModel;
            RaiseNavigated(viewModelType);
            return Task.CompletedTask;
        }

        private void NavigateBack(object sender, BackRequestedEventArgs args)
        {
            if (NavigateBackSplitView(args)) return;
            if (NavigateBackArticleFrame(args)) return;
            if (NavigateBackFrame(args)) return;
        }

        private bool NavigateBackFrame(BackRequestedEventArgs args)
        {
            var navigationFrame = GetChild<Frame>(Window.Current.Content, 0);
            if (navigationFrame?.CanGoBack != true) return false;
            var instance = navigationFrame.BackStack.Last().Parameter;
            navigationFrame.GoBack();
            navigationFrame.ForwardStack.Clear();
            args.Handled = true;
            
            if (instance == null) return true;
            var page = (Page)navigationFrame.Content ?? throw new NullReferenceException();
            var type = instance.GetType();
            page.DataContext = _resolver.Resolve(type);
            RaiseNavigated(type);
            return true;
        }

        private bool NavigateBackArticleFrame(BackRequestedEventArgs args)
        {
            var navigationFrame = GetChild<Frame>(Window.Current.Content, 1);
            if (navigationFrame?.CanGoBack != true) return false;
            var instance = navigationFrame.BackStack.Last().Parameter;
            navigationFrame.GoBack();
            navigationFrame.ForwardStack.Clear();
            args.Handled = true;

            if (instance == null) return true;
            var page = (Page)navigationFrame.Content ?? throw new NullReferenceException();
            page.DataContext = instance;
            RaiseNavigated(instance.GetType());
            return true;
        }
        
        private bool NavigateBackSplitView(BackRequestedEventArgs args)
        {
            var splitView = GetChild<SwipeableSplitView>(Window.Current.Content, 0);
            var shouldClose = splitView.IsSwipeablePaneOpen && splitView.DisplayMode == SplitViewDisplayMode.Overlay;
            if (!shouldClose) return false;
            splitView.IsSwipeablePaneOpen = false;
            return args.Handled = true;
        }

        private void RaiseNavigated(Type type)
        {
            if (type == typeof(FeedItemFullViewModel))
            {
                var navigationFrame = GetChild<Frame>(Window.Current.Content, 0);
                var page = (Page) navigationFrame.Content ?? throw new NullReferenceException();
                type = page.DataContext.GetType();
            }
            CurrentViewModelType = type;
            _navigated.OnNext(type);
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
