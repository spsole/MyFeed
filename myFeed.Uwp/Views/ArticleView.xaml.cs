using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using myFeed.Uwp.Services;
using myFeed.ViewModels;

namespace myFeed.Uwp.Views
{
    public sealed partial class ArticleView : Page
    {
        public ArticleView() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            foreach(var block in await Services.Uwp.Current.Resolve<UwpHtmlParserService>()
                .ParseAsync(((ArticleViewModel)e.Parameter)?.Content.Value)) RichContent.Blocks.Add(block);
            base.OnNavigatedTo(e);
        }
    }
}
