using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace myFeed.Views.Uwp.Views
{
    public sealed partial class ArticleView : Page
    {
        public ArticleView() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e) => DataContext = e.Parameter;
    }
}
