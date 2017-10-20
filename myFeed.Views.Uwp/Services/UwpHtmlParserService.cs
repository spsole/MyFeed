using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using myFeed.Services.Abstractions;
using HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Windows.UI.Xaml.VerticalAlignment;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpHtmlParserService
    {
        private readonly ISettingsService _service;
        private readonly HtmlParser _htmlParser;

        public UwpHtmlParserService(ISettingsService settingsService)
        {
            _service = settingsService;
            _htmlParser = new HtmlParser();
        }

        public async Task<IEnumerable<Block>> ParseAsync(string html)
        {
            var size = await _service.GetAsync<int>("FontSize");
            var document = await Task.Run(() => _htmlParser.ParseAsync(html));
            var paragraph = new Paragraph
            {
                FontSize = size,
                LineHeight = size * 1.5
            };
            var node = GenerateNode(document.DocumentElement);
            paragraph.Inlines.Add(node);
            return new[] {paragraph};
        }

        private Inline GenerateHeader(IElement element)
        {
            var span = new Span();
            var bold = GenerateBold(element);
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(bold);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private Inline GenerateImage(IElement element)
        {
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var imageElement = (IHtmlImageElement)element;
            if (!Uri.IsWellFormedUriString(
                imageElement.Source, UriKind.Absolute))
                return span;

            var bitmap = new BitmapImage(new Uri(imageElement.Source));
            var image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(-12, 12, -12, 12),
                MaxHeight = 500
            };
            inlineUiContainer.Child = image;
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(inlineUiContainer);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private Inline GenerateHyperLink(IElement element)
        {
            var span = new Span();
            var hyperlink = new Hyperlink();
            var linkElement = (IHtmlAnchorElement)element;
            if (Uri.IsWellFormedUriString(linkElement.Href, UriKind.Absolute))
                hyperlink.NavigateUri = new Uri(linkElement.Href, UriKind.Absolute);
            hyperlink.Inlines.Add(new Run { Text = element.TextContent });
            if (!string.IsNullOrWhiteSpace(element.TextContent))
                span.Inlines.Add(hyperlink);
            return span;
        }

        private Inline GenerateLi(IElement element)
        {
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var brush = (SolidColorBrush)Application.Current
                .Resources["SystemControlBackgroundAccentBrush"];
            var ellipse = new Ellipse
            {
                Fill = brush,
                Width = 6,
                Height = 6,
                Margin = new Thickness(0, 0, 9, 2)
            };
            inlineUiContainer.Child = ellipse;
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(inlineUiContainer);
            AttachChildren(span.Inlines, element);
            return span;
        }

        private Inline GenerateBlockQuote(IElement element)
        {
            var italic = new Italic();
            italic.Inlines.Add(new LineBreak());
            AttachChildren(italic.Inlines, element);
            italic.Inlines.Add(new LineBreak());
            return italic;
        }

        private Inline GenerateBold(IElement element)
        {
            var bold = new Bold();
            AttachChildren(bold.Inlines, element);
            return bold;
        }

        private Inline GenerateItalic(IElement element)
        {
            var italic = new Italic();
            AttachChildren(italic.Inlines, element);
            return italic;
        }

        private Inline GenerateSpan(IElement element)
        {
            var span = new Span();
            AttachChildren(span.Inlines, element);
            return span;
        }

        private Inline GenerateFrame(IElement element)
        {
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var frameElement = (IHtmlInlineFrameElement)element;
            if (!Uri.IsWellFormedUriString(
                frameElement.Source, UriKind.Absolute))
                return span;
            var webView = new WebView(WebViewExecutionMode.SeparateThread)
            {
                Width = frameElement.DisplayWidth,
                Height = frameElement.DisplayHeight
            };
            inlineUiContainer.Child = webView;
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(inlineUiContainer);
            span.Inlines.Add(new LineBreak());
            webView.Navigate(new Uri(frameElement.Source));
            return span;
        }

        private void AttachChildren(InlineCollection collection, INode element)
        {
            foreach (var node in element.ChildNodes) collection.Add(GenerateNode(node));
            if (!collection.Any()) collection.Add(new Run {Text = element.TextContent});
        }

        private Inline GenerateNode(INode node)
        {
            if (node is IElement element)
                return GenerateElement(element);
            return new Run {Text=node.TextContent};
        }

        private Inline GenerateElement(IElement element)
        {
            switch (element.TagName.ToLowerInvariant())
            {
                case "a":
                    return GenerateHyperLink(element);
                case "img":
                    return GenerateImage(element);
                case "br":
                    return new Span();
                case "i":
                case "em":
                    return GenerateItalic(element);
                case "blockquote":
                    return GenerateBlockQuote(element);
                case "b":
                case "strong":
                    return GenerateBold(element);
                case "li":
                case "dt":
                    return GenerateLi(element);
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    return GenerateHeader(element);
                case "iframe":
                    return GenerateFrame(element);
                default:
                    return GenerateSpan(element);
            }
        }
    }
}
