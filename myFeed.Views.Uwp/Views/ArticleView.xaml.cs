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
            var article = ((ArticleViewModel)DataContext).Article.Value = (FeedItemViewModel)e.Parameter;
            foreach(var block in await UwpViewModelLocator.Current.Resolve<UwpHtmlParserService>()
                .ParseAsync(article?.Content.Value)) RichContent.Blocks.Add(block);
            base.OnNavigatedTo(e);
        }
    }
}
