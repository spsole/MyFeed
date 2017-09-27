using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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
                    NavigateWithViewModel(GetChild<Frame>(Window.Current.Content, 0));
                    break;
                case nameof(ArticleViewModel):
                    NavigateWithViewModel(GetChild<Frame>(Window.Current.Content, 1));
                    break;
                case nameof(MenuViewModel):
                    NavigateWithViewModel((Frame)Window.Current.Content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Task.CompletedTask; void NavigateWithViewModel(Frame frame)
            {
                frame.Navigate(Pages[typeof(T)], instance);
                ((Page) frame.Content).DataContext = instance;
                RaiseNavigated(instance);
            }
        }

        private void NavigateBack(object sender, BackRequestedEventArgs e)
        {
            var articleFrame = GetChild<Frame>(Window.Current.Content, 1);
            var splitViewFrame = GetChild<Frame>(Window.Current.Content, 0);
            var frame = 
                articleFrame?.CanGoBack == true
                    ? articleFrame
                    : splitViewFrame?.CanGoBack == true
                        ? splitViewFrame
                        : null;
            if (frame == null) return;

            var instance = frame.BackStack.Last().Parameter;
            frame.GoBack();
            frame.ForwardStack.Clear();
            if (instance == null) return;

            // DataContext should be updated with _new_ instance of 
            // ViewModel. Reusing old ones may cause misbehavior.
            ((Page)frame.Content).DataContext = instance is ArticleViewModel 
                ? instance : Uwp.Current.Resolve(instance.GetType());
            RaiseNavigated(instance);
        }

        private void RaiseNavigated(object instance)
        {
            Navigated?.Invoke(this, instance?.GetType());
            _systemNavigationManager.AppViewBackButtonVisibility =
                GetChild<Frame>(Window.Current.Content, 0)?.CanGoBack == true ||
                GetChild<Frame>(Window.Current.Content, 1)?.CanGoBack == true
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
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
