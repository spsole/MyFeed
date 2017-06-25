using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Article;
using myFeed.Feed;
using myFeed.Navigation;

namespace myFeed.Fave
{
    public sealed partial class FavePage
    {
        public FavePageViewModel ViewModel => DataContext as FavePageViewModel;

        public static Frame NavigationFrame { get; private set; }

        public FavePage()
        {
            InitializeComponent();
            NavigationFrame = ArticleFrame;
            ArticleFrame.Navigate(typeof(EmptyPage));

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

        private void DeleteRequested(object sender, object e) => 
            ViewModel.DeleteItem((FaveItemViewModel)
                ((FrameworkElement)sender).DataContext);

        private void OpenArticleUsingXamlWeeds(object sender, RoutedEventArgs e) => 
            ArticleFrame.Navigate(typeof(ArticlePage), 
                (FeedItemViewModel)((FrameworkElement)sender).DataContext);

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Go back and back and back and back...
            while (true)
                if (ArticleFrame.CanGoBack)
                    ArticleFrame.GoBack();
                else break;
        }
    }
}
