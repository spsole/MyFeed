using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using myFeed.Services.Abstractions;
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
            {typeof(SourcesViewModel), typeof(SourcesView)},
            {typeof(SearchViewModel), typeof(SearchView)},
            {typeof(MenuViewModel), typeof(MenuView)},
            {typeof(FaveViewModel), typeof(FaveView)},
            {typeof(FeedViewModel), typeof(FeedView)}
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
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            systemNavigationManager.BackRequested += NavigateBack;
            EnsureStatusBarIsEnabled();
        }

        private static void EnsureStatusBarIsEnabled()
        {
            var page = (Page)((Frame)Window.Current.Content).Content;
            var color = (Color)Application.Current.Resources["SystemChromeLowColor"];
            StatusBar.SetBackgroundColor(page, color);
            StatusBar.SetBackgroundOpacity(page, 1);
        }

        public event EventHandler<Type> Navigated;

        public Task Navigate<T>() where T : class => Navigate(Uwp.Current.Resolve<T>());

        public Task Navigate<T>(T instance) where T : class
        {
            switch (typeof(T).Name)
            {
                case nameof(FeedViewModel):
                case nameof(FaveViewModel):
                case nameof(SearchViewModel):
                case nameof(SourcesViewModel):
                case nameof(SettingsViewModel):
                    NavigateFrame(GetChild<Frame>(Window.Current.Content, 0), instance);
                    break;
                case nameof(ArticleViewModel):
                    NavigateFrame(GetChild<Frame>(Window.Current.Content, 1), instance);
                    break;
                case nameof(MenuViewModel):
                    NavigateFrame((Frame)Window.Current.Content, instance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Task.CompletedTask; 
        }

        private void NavigateFrame<T>(Frame frame, T viewModel) where T : class 
        {
            frame.Navigate(Pages[typeof(T)], viewModel);
            ((Page)frame.Content).DataContext = viewModel;
            RaiseNavigated(viewModel);
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
