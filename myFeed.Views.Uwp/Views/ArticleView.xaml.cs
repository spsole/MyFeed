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
            foreach(var block in await UwpViewModelLocator.Current.Resolve<UwpHtmlParserService>()
                .ParseAsync(((ArticleViewModel)e.Parameter)?.Content.Value)) RichContent.Blocks.Add(block);
            base.OnNavigatedTo(e);
        }
    }
}
