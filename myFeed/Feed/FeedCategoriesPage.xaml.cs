using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using myFeed.Article;
using myFeed.Navigation;
using myFeed.Sources;

namespace myFeed.Feed
{
    public sealed partial class FeedCategoriesPage
    {
        public FeedCategoriesViewModel ViewModel => DataContext as FeedCategoriesViewModel;

        public static Frame NavigationFrame { get; private set; }

        public FeedCategoriesPage()
        {
            // Init components.
            InitializeComponent();
            ArticleFrame.Navigate(typeof(EmptyPage));
            NavigationFrame = ArticleFrame;

            // Handle back navigation for article.
            var manager = NavigationManager.GetInstance();
            manager.AddBackHandler(args =>
            {
                if (!ArticleFrame.CanGoBack) return;
                ArticleFrame.GoBack();
                args.Handled = true;
            }, 
            EventPriority.High);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var manager = SourcesManager.GetInstance();
            if (!manager.SourcesChanged) return;
            ViewModel.LoadAsync();
            manager.SourcesChanged = false;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            // Go back and back and back and back...
            while (true)
                if (ArticleFrame.CanGoBack)
                    ArticleFrame.GoBack();
                else break;
        }
    }
}
