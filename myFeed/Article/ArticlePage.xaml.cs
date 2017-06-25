using System;
using System.Net;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Navigation;
using myFeed.Extensions;
using myFeed.Feed;

namespace myFeed.Article
{
    public sealed partial class ArticlePage
    {
        public ArticlePage() => InitializeComponent();
        
        public FeedItemViewModel ViewModel => DataContext as FeedItemViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = (FeedItemViewModel) e.Parameter ?? throw new ArgumentNullException();
            var content = WebUtility.HtmlDecode(ViewModel.GetModel().Content);
                content = Regex.Replace(content, @"([\r\n]+)|([\n]+)", string.Empty);

            // Manage font size.
            RichContent.FontSize = Settings
                .SettingsManager
                .GetInstance()
                .GetSettings()
                .ArticleFontSize;
            RichContent.LineHeight = RichContent.FontSize * 1.5;

            // Generate blocks.
            HtmlToRichTextBlock
                .GenerateBlocksForHtml(content)
                .ForEach(i => RichContent.Blocks.Add(i));
            base.OnNavigatedTo(e);
        }
    }
}
