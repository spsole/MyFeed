using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using myFeed.Article;
using myFeed.FeedModels.Models;

namespace myFeed.Feed
{
    public sealed partial class FeedPage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(FeedViewModel),
                typeof(FeedPage),
                new PropertyMetadata(null)
            );

        public FeedViewModel ViewModel
        {
            get => (FeedViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public FeedPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var category = (FeedCategoryModel)e.Parameter;
            ViewModel = new FeedViewModel(category);
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
