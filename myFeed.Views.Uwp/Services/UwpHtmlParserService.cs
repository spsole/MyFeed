using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using HtmlAgilityPack;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpHtmlParserService
    {
        private readonly ITranslationsService _translationsService;
        private readonly IPlatformService _platformService;
        private readonly ISettingsService _settingsService;

        private double _fontSize;
        private bool _loadImages;

        public UwpHtmlParserService(
            ITranslationsService translationsService,
            IPlatformService platformService,
            ISettingsService settingsService)
        {
            _translationsService = translationsService;
            _platformService = platformService;
            _settingsService = settingsService;
        }

        public async Task<IEnumerable<Block>> ParseAsync(string html)
        {
            if (html == null) return new Block[] {};
            var htmlDocument = new HtmlDocument();
            await Task.Run(async () =>
            {
                _loadImages = await _settingsService.GetAsync<bool>("LoadImages");
                _fontSize = await _settingsService.GetAsync<double>("FontSize");
                htmlDocument.LoadHtml(html);
            });
            var paragraph = new Paragraph { FontSize = _fontSize, LineHeight = _fontSize * 1.5 };
            paragraph.Inlines.Add(AddChildren(new Span(), htmlDocument.DocumentNode));
            return new List<Block> { paragraph };
        }

        private Span AddChildren(Span span, HtmlNode node)
        {
            var xamlNodesList = node.ChildNodes.Select(GenerateBlockForNode).Where(i => i != null).ToList();
            foreach (var xamlNode in xamlNodesList) span.Inlines.Add(xamlNode);
            if (xamlNodesList.Count == 0) span.Inlines.Add(new Run { Text = node.InnerText });
            return span;
        }

        private Inline GenerateBlockForNode(HtmlNode node)
        {
            switch (node.Name.ToLower())
            {
                case "p": 
                case "div":
                    return GenerateInnerParagraph(node);
                case "a":
                    if (node.ChildNodes.Count >= 1 && node.FirstChild?.Name?.ToLower() == "img")
                        return GenerateImage(node.FirstChild);
                    return GenerateHyperLink(node);
                case "img":
                    return GenerateImage(node);
                case "br":
                    return new Span();
                case "i":
                case "em":
                case "blockquote":
                    return AddChildren(new Italic(), node);
                case "b":
                case "strong":
                    return AddChildren(new Bold(), node);
                case "li":
                case "dt":
                    return GenerateLi(node);
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    return GenerateH(node);
                default:
                    return AddChildren(new Span(), node);
            }
        }

        private Inline GenerateLi(HtmlNode node)
        {
            var span = new Span();
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(new InlineUIContainer { Child = new Ellipse
            {
                Fill = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"],
                Width = 6, Height = 6, Margin = new Thickness(0, 0, 9, 2)
            }});
            AddChildren(span, node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private Inline GenerateH(HtmlNode node)
        {
            var span = new Span { FontSize = _fontSize * 1.5, FontWeight = FontWeights.SemiBold };
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(new Run { Text = node.InnerText });
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private Inline GenerateInnerParagraph(HtmlNode node)
        {
            if (!node.HasChildNodes && string.IsNullOrWhiteSpace(node.InnerText)) return null;
            var span = AddChildren(new Span(), node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateHyperLink(HtmlNode node)
        {
            var link = new Hyperlink();
            var reference = node.Attributes["href"]?.Value;
            if (Uri.IsWellFormedUriString(reference, UriKind.Absolute))
                link.NavigateUri = new Uri(reference, UriKind.Absolute);
            link.Inlines.Add(new Run { Text = node.InnerText });
            var span = new Span();
            span.Inlines.Add(link);
            return span;
        }

        private Inline GenerateImage(HtmlNode node)
        {
            if (!_loadImages) return new Span();
            var reference = node.Attributes["src"]?.Value; 
            if (!Uri.IsWellFormedUriString(reference, UriKind.Absolute)) return new Span();
            var sourceUri = new Uri(reference, UriKind.Absolute);
            var image = new Image
            {
                Source = new BitmapImage(sourceUri) { CreateOptions = BitmapCreateOptions.IgnoreImageCache },
                Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center, MaxHeight = 500,
                Margin = new Thickness(-12, 12, -12, 12), Opacity = 0
            };
            image.ImageOpened += (sender, e) =>
            {
                var bmp = (BitmapImage)image.Source;
                if (bmp.PixelHeight >= bmp.PixelWidth) image.Margin = new Thickness(0, 12, 0, 12);
                image.Fade(1, 300, 100).Start();
            };
            image.RightTapped += (sender, e) =>
            {
                var launcher = new MenuFlyoutItem { Text = _translationsService.Resolve("ImageOpenFullSize") };
                var copier = new MenuFlyoutItem { Text = _translationsService.Resolve("ImageCopyLink") };
                copier.Click += (s, o) => _platformService.CopyTextToClipboard(sourceUri.AbsoluteUri);
                launcher.Click += (s, o) => _platformService.LaunchUri(sourceUri);
                new MenuFlyout {Items = {launcher, copier}}.ShowAt(image);
            };
            var inlineUiContainer = new InlineUIContainer { Child = image };
            var span = new Span();
            span.Inlines.Add(inlineUiContainer);
            span.Inlines.Add(new LineBreak());
            return span;
        }
    }
}
