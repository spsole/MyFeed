using System;
using System.Linq;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.UI.Animations;

namespace MyFeed.Uwp.Controls
{
    public sealed class HtmlBlock : ContentControl
    {
        public HtmlBlock() => DefaultStyleKey = typeof(HtmlBlock);

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
            nameof(Html), typeof(string), typeof(HtmlBlock), new PropertyMetadata(
                null, OnHtmlPropertyChanged));

        public static readonly DependencyProperty ImagesProperty = DependencyProperty.Register(
            nameof(Images), typeof(bool), typeof(HtmlBlock), new PropertyMetadata(
                null, OnHtmlPropertyChanged));

        public static readonly DependencyProperty HtmlFontSizeProperty = DependencyProperty.Register(
            nameof(HtmlFontSize), typeof(double), typeof(HtmlBlock), new PropertyMetadata(
                null, OnHtmlPropertyChanged));

        public string Html
        {
            get => (string)GetValue(HtmlProperty);
            set => SetValue(HtmlProperty, value);
        }

        public bool Images
        {
            get => (bool)GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }

        public double HtmlFontSize
        {
            get => (double)GetValue(HtmlFontSizeProperty);
            set => SetValue(HtmlFontSizeProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            GenerateHtmlContents();
        }

        private static void OnHtmlPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var sender = (HtmlBlock)o;
            var child = sender.GetTemplateChild("ContentRichTextBlock");
            if (child == null) return;
            sender.GenerateHtmlContents();
        }

        private void GenerateHtmlContents()
        {
            var richTextBlock = (RichTextBlock)GetTemplateChild("ContentRichTextBlock");
            if (richTextBlock == null || Html == null || HtmlFontSize < 1) return;

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(Html);

            var paragraph = new Paragraph {FontSize = HtmlFontSize, LineHeight = HtmlFontSize * 1.5};
            paragraph.Inlines.Add(AddChildren(new Span(), htmlDocument.DocumentNode));
            richTextBlock.Blocks.Add(paragraph);
        }

        private Span AddChildren(Span span, HtmlNode node)
        {
            var xamlNodesList = node.ChildNodes.Select(GenerateBlockForNode).Where(i => i != null).ToList();
            foreach (var xamlNode in xamlNodesList) span.Inlines.Add(xamlNode);
            if (xamlNodesList.Count == 0) span.Inlines.Add(new Run { Text = node.InnerText });
            return span;
        }

        private Inline GenerateLi(HtmlNode node)
        {
            var span = new Span();
            var accent = (SolidColorBrush) Resources["SystemControlBackgroundAccentBrush"];
            var ellipse = new Ellipse {Fill = accent, Width = 6, Height = 6, Margin = new Thickness(0, 0, 9, 2)};
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(new InlineUIContainer {Child = ellipse});
            AddChildren(span, node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private Inline GenerateH(HtmlNode node)
        {
            var span = new Span {FontSize = FontSize * 1.5, FontWeight = FontWeights.SemiBold};
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(new Run {Text = node.InnerText});
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

        private Inline GenerateHyperLink(HtmlNode node)
        {
            var link = new Hyperlink();
            var reference = node.Attributes["href"]?.Value;
            if (Uri.IsWellFormedUriString(reference, UriKind.Absolute))
                try { link.NavigateUri = new Uri(reference, UriKind.Absolute); } catch { /* ignored */ }
            link.Inlines.Add(new Run { Text = node.InnerText });
            var span = new Span();
            span.Inlines.Add(link);
            return span;
        }

        private Inline GenerateImage(HtmlNode node)
        {
            if (!Images) return new Span();
            var reference = node.Attributes["src"]?.Value;
            if (!Uri.IsWellFormedUriString(reference, UriKind.Absolute)) return new Span();
            var sourceUri = new Uri(reference, UriKind.Absolute);
            var image = new Image
            {
                Source = new BitmapImage(sourceUri) { CreateOptions = BitmapCreateOptions.IgnoreImageCache },
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxHeight = 500,
                Margin = new Thickness(-12, 12, -12, 12),
                Opacity = 0
            };
            image.ImageOpened += (sender, e) =>
            {
                var bmp = (BitmapImage)image.Source;
                if (bmp.PixelHeight >= bmp.PixelWidth) image.Margin = new Thickness(0, 12, 0, 12);
                if (bmp.PixelWidth < 300)
                {
                    image.HorizontalAlignment = HorizontalAlignment.Center;
                    image.Margin = new Thickness(0, 12, 0, 12);
                    image.Stretch = Stretch.None;
                }
                image.Fade(1, 300, 100).Start();
            };
            image.RightTapped += async (sender, e) => await Launcher.LaunchUriAsync(sourceUri);
            var inlineUiContainer = new InlineUIContainer { Child = image };
            var span = new Span();
            span.Inlines.Add(inlineUiContainer);
            span.Inlines.Add(new LineBreak());
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
    }
}
