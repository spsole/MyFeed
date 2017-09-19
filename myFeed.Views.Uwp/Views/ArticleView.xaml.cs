using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using myFeed.ViewModels.Implementations;
using myFeed.Views.Uwp.Services;

namespace myFeed.Views.Uwp.Views
{
    public sealed partial class ArticleView : Page
    {
        public ArticleView() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = e.Parameter;
            var viewModel = (FeedItemViewModel)e.Parameter;
            var size = await UwpLocator.Current.GetSetting<int>("FontSize");
            var xmlParserService = new UwpXmlParserService(size);
            var blocks = await xmlParserService.ParseAsync(viewModel?.Content.Value);
            foreach (var block in blocks) RichContent.Blocks.Add(block);
            base.OnNavigatedTo(e);
        }
    }
}
