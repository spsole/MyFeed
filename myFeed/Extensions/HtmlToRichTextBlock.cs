using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using myFeed.Settings;

namespace myFeed.Extensions
{
    public static class HtmlToRichTextBlock
    {
        public static List<Block> GenerateBlocksForHtml(string xhtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(xhtml);
            return new List<Block> { GenerateParagraph(doc.DocumentNode) };
        }


        private static Paragraph AddChildren(Paragraph paragraph, HtmlNode node)
        {
            var isAdded = false;
            foreach (var child in node.ChildNodes)
            {
                var inline = GenerateBlockForNode(child);
                if (inline == null) continue;
                paragraph.Inlines.Add(inline);
                isAdded = true;
            }
            if (!isAdded) paragraph.Inlines.Add(new Run { Text = node.InnerText });
            return paragraph;
        }

        private static Span AddChildren(Span span, HtmlNode node)
        {
            var isAdded = false;
            foreach (var child in node.ChildNodes)
            {
                var inline = GenerateBlockForNode(child);
                if (inline == null) continue;
                span.Inlines.Add(inline);
                isAdded = true;
            }
            if (!isAdded) span.Inlines.Add(new Run { Text = node.InnerText });
            return span;
        }

        private static Inline GenerateBlockForNode(HtmlNode node)
        {
            switch (node.Name.ToLower())
            {
                case "p":
                case "div":
                    return GenerateInnerParagraph(node);
                case "a":
                    if (node.ChildNodes.Count >= 1 && node.FirstChild.Name.ToLower() == "img")
                        return GenerateImage(node.FirstChild);
                    return GenerateHyperLink(node);
                case "img":
                    return GenerateImage(node);
                case "br":
                    return new LineBreak();
                case "i":
                case "em":
                case "blockquote":
                    return GenerateItalic(node);
                case "b":
                case "strong":
                    return GenerateBold(node);
                case "li":
                case "dt":
                    return GenerateLi(node);
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    return GenerateH(node, SettingsManager
                        .GetInstance().GetSettings().ArticleFontSize);
                default:
                    return GenerateSpan(node);
            }
        }

        private static Block GenerateParagraph(HtmlNode node) => AddChildren(new Paragraph(), node);

        private static Inline GenerateItalic(HtmlNode node) => AddChildren(new Italic(), node);

        private static Inline GenerateBold(HtmlNode node) => AddChildren(new Bold(), node);

        private static Inline GenerateSpan(HtmlNode node) => AddChildren(new Span(), node);

        private static Inline GenerateLi(HtmlNode node)
        {
            var span = new Span();
            var iui = new InlineUIContainer();
            var ellipse = new Ellipse
            {
                Fill = (Windows.UI.Xaml.Media.SolidColorBrush)
                    Application.Current.Resources["SystemControlBackgroundAccentBrush"],
                Width = 6, Height = 6, Margin = new Thickness(0, 0, 9, 2)
            };
            iui.Child = ellipse;
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(iui);
            AddChildren(span, node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateH(HtmlNode node, double size)
        {
            var span = new Span { FontSize = size, FontWeight = Windows.UI.Text.FontWeights.SemiBold };
            var run = new Run { Text = node.InnerText };
            span.Inlines.Add(new LineBreak());
            span.Inlines.Add(run);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateInnerParagraph(HtmlNode node)
        {
            var span = new Span();
            if (!node.HasChildNodes && string.IsNullOrWhiteSpace(node.InnerText)) return null;
            if (node.ChildNodes.Count == 1 && !node.FirstChild.HasChildNodes)
            {
                switch (node.FirstChild.Name.ToLower())
                {
                    case "span":
                    case "br":
                        if (string.IsNullOrWhiteSpace(node.FirstChild.InnerText)) return null;
                        break;
                    case "img":
                        AddChildren(span, node);
                        return span;
                }
            }

            if (node.Name.ToLower() != "div") span.Inlines.Add(new LineBreak());
            AddChildren(span, node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateHyperLink(HtmlNode node)
        {
            var span = new Span();
            var link = new Hyperlink();
            try 
            {
                link.NavigateUri = new Uri(node.Attributes["href"].Value, UriKind.Absolute);
            }
            catch
            {
                // ignored
            }
            link.Inlines.Add(new Run { Text = node.InnerText });
            span.Inlines.Add(link);
            return span;
        }

        private static Inline GenerateImage(HtmlNode node)
        {
            var span = new Span();
            if (!SettingsManager.GetInstance().GetSettings().DownloadImages) return span; 
            try
            {
                var iui = new InlineUIContainer();
                var sourceUri = System.Net.WebUtility.HtmlDecode(node.Attributes["src"].Value);
                if (sourceUri.Length > 3 && sourceUri[0] == '/' && sourceUri[1] == '/')
                    sourceUri = $"http:{sourceUri}"; // fix for hi-news.ru
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(sourceUri, UriKind.Absolute))
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache
                    },
                    Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(-12, 12, -12, 12),
                    Opacity = 0,
                    MaxHeight = 500
                };

                img.ImageOpened += (sender, e) =>
                {
                    var bmp = (BitmapImage)img.Source;
                    if (bmp.PixelHeight >= bmp.PixelWidth) img.Margin = new Thickness(0, 12, 0, 12);
                    img.FadeIn();
                };

                img.RightTapped += (sender, e) =>
                {
                    var menu = new MenuFlyout();
                    var rl = new ResourceLoader();

                    var menuitem = new MenuFlyoutItem { Text = rl.GetString("OpenFullSize") };
                    menuitem.Click += async (s, o) =>
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(sourceUri));
                    };
                    menu.Items.Add(menuitem);

                    var menuitem2 = new MenuFlyoutItem { Text = rl.GetString("CopyLinkToImage") };
                    menuitem2.Click += (s, o) =>
                    {
                        var dataPackage = new DataPackage();
                        dataPackage.SetText(sourceUri);
                        Clipboard.SetContent(dataPackage);
                    };
                    menu.Items.Add(menuitem2);
                    menu.ShowAt((Image)sender);
                };

                iui.Child = img;
                span.Inlines.Add(iui);
            }
            catch
            {
                // ignored
            }
            return span;
        }
    }
}