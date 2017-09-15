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
using HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Windows.UI.Xaml.VerticalAlignment;

namespace myFeed.Views.Uwp.Platform
{
    public static class UwpParser
    {
        public static async Task<IEnumerable<Block>> GetBlocksTree(string html)
        {
            var parser = new HtmlParser();
            var document = await parser.ParseAsync(html);
            var paragraph = new Paragraph();
            AttachChildren(paragraph.Inlines, document.DocumentElement);
            return new List<Block> {paragraph};
        }

        private static Inline GenerateImage(IElement element)
        {
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var imageElement = (IHtmlImageElement) element;
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
            span.Inlines.Add(inlineUiContainer);
            return span;
        }

        private static Inline GenerateHyperLink(IElement element)
        {
            var span = new Span();
            var hyperlink = new Hyperlink();
            var linkElement = (IHtmlAnchorElement) element;
            if (Uri.IsWellFormedUriString(linkElement.Href, UriKind.Absolute))
                hyperlink.NavigateUri = new Uri(linkElement.Href, UriKind.Absolute);
            hyperlink.Inlines.Add(new Run { Text = element.TextContent });
            span.Inlines.Add(hyperlink);
            return span;
        }

        private static Inline GenerateLi(IElement element)
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
            span.Inlines.Add(GenerateBreak(element));
            span.Inlines.Add(inlineUiContainer);
            AttachChildren(span.Inlines, element);
            span.Inlines.Add(GenerateBreak(element));
            return span;
        }

        private static Inline GenerateBreak(IElement element) => new LineBreak();

        private static Inline GenerateHeader(IElement element)
        {
            var span = new Span();
            span.Inlines.Add(GenerateBreak(element));
            span.Inlines.Add(GenerateBold(element));
            span.Inlines.Add(GenerateBreak(element));
            return span;
        }

        private static Inline GenerateBold(IElement element)
        {
            var bold = new Bold();
            AttachChildren(bold.Inlines, element);
            return bold;
        }

        private static Inline GenerateItalic(IElement element)
        {
            var italic = new Italic();
            AttachChildren(italic.Inlines, element);
            return italic;
        }

        private static Inline GenerateSpan(IElement element)
        {
            var span = new Span();
            AttachChildren(span.Inlines, element);
            return span;
        }

        private static void AttachChildren(InlineCollection collection, IElement element)
        {
            element.Children.Select(GenerateNode).ToList().ForEach(collection.Add);
            if (!collection.Any()) collection.Add(new Run {Text = element.TextContent});
        }

        private static Inline GenerateNode(IElement element)
        {
            switch (element.TagName.ToLowerInvariant())
            {
                case "a":
                    return GenerateHyperLink(element);
                case "img":
                    return GenerateImage(element);
                case "br":
                    return GenerateBreak(element);
                case "i":
                case "em":
                case "blockquote":
                    return GenerateItalic(element);
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
                default:
                    return GenerateSpan(element);
            }
        }
    }
}
