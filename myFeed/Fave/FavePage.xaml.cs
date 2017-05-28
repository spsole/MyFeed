using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using myFeed.Article;
using myFeed.Feed;
using myFeed.Navigation;

namespace myFeed.Fave
{
    public sealed partial class FavePage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(FavePageViewModel),
                typeof(FavePage),
                new PropertyMetadata(null)
            );

        public FavePageViewModel ViewModel
        {
            get => (FavePageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static Frame NavigationFrame { get; private set; }

        public FavePage()
        {
            ViewModel = new FavePageViewModel();
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

        private void ShowFlyout(object sender, RoutedEventArgs e) => 
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

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
