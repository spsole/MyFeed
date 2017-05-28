using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using myFeed.Extensions;
using myFeed.Feed;

namespace myFeed.Article
{
    public sealed partial class ArticlePage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(FeedItemViewModel),
                typeof(ArticlePage),
                new PropertyMetadata(null)
            );

        public FeedItemViewModel ViewModel
        {
            get => (FeedItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ArticlePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = (FeedItemViewModel) e.Parameter ?? throw new ArgumentNullException();
            var content = Regex.Replace(ViewModel.GetModel().Content, @"(&.*?;)", @" ");
                content = Regex.Replace(content, @"\r\n?|\n", string.Empty);

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
