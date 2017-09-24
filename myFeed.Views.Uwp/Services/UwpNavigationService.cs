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
            {typeof(SourcesViewModel), typeof(SourcesView)},
            {typeof(ArticleViewModel), typeof(ArticleView)},
            {typeof(SearchViewModel), typeof(SearchView)},
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
            _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            _systemNavigationManager.BackRequested += NavigateBack;
        }

        public event EventHandler<Type> Navigated;

        public Task Navigate(Type viewModelType) => Navigate(viewModelType, null);

        public Task Navigate(Type viewModelType, object parameter)
        {
            switch (viewModelType.Name)
            {
                case "FeedViewModel":
                case "FaveViewModel":
                case "SearchViewModel":
                case "SourcesViewModel":
                case "SettingsViewModel":
                    var splitViewFrame = GetFrame(Window.Current.Content, 0);
                    splitViewFrame?.Navigate(Pages[viewModelType], parameter);
                    OnNavigated(viewModelType);
                    break;
                case "ArticleViewModel":
                    var articleFrame = GetFrame(Window.Current.Content, 1);
                    articleFrame?.Navigate(Pages[viewModelType], parameter);
                    OnNavigated(viewModelType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewModelType), viewModelType, null);
            }
            return Task.CompletedTask;
        }

        private void NavigateBack(object sender, BackRequestedEventArgs e)
        {
            var articleFrame = GetFrame(Window.Current.Content, 1);
            if (articleFrame != null && articleFrame.CanGoBack)
            {
                articleFrame.GoBack();
                OnNavigatedByView(articleFrame.CurrentSourcePageType);
                return;
            }
            var splitViewFrame = GetFrame(Window.Current.Content, 0);
            if (splitViewFrame.CanGoBack) splitViewFrame.GoBack();
            OnNavigatedByView(splitViewFrame.CurrentSourcePageType);

            void OnNavigatedByView(Type viewType)
            {
                if (Pages.Values.Contains(viewType))
                    OnNavigated(Pages.First(x => x.Value == viewType).Key);
            }
        }

        private void OnNavigated(Type viewModelType)
        {
            Navigated?.Invoke(this, viewModelType);
            UpdateBackButtonVisibility();
        }

        private void UpdateBackButtonVisibility() =>
            _systemNavigationManager.AppViewBackButtonVisibility =
                GetFrame(Window.Current.Content, 0)?.CanGoBack == true ||
                GetFrame(Window.Current.Content, 1)?.CanGoBack == true
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;

        private static Frame GetFrame(DependencyObject root, int depth)
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var x = 0; x < childrenCount; x++)
            {
                var child = VisualTreeHelper.GetChild(root, x);
                if (child is Frame thisFrame && depth-- == 0) return thisFrame;
                var frame = GetFrame(child, depth);
                if (frame != null) return frame;
            }
            return null;
        }
    }
}
