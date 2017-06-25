using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using myFeed.Article;
using myFeed.FeedModels.Models;

namespace myFeed.Feed
{
    public sealed partial class FeedPage
    {
        public FeedViewModel ViewModel => DataContext as FeedViewModel;

        public FeedPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var category = (FeedCategoryModel)e.Parameter;
            DataContext = new FeedViewModel(category);
            ViewModel.FetchAsync();
            base.OnNavigatedTo(e);
        }

        private void ShowFlyout(object sender, RoutedEventArgs e) => 
            FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);

        private void OpenArticleUsingXamlWeeds(object sender, RoutedEventArgs e)
        {
            // Navigate categories page's frame.
            var model = (FeedItemViewModel)((FrameworkElement)sender).DataContext;
            model.MarkAsRead();
            FeedCategoriesPage.NavigationFrame.Navigate(typeof(ArticlePage), model);
        }
    }
}
