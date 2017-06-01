using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Windows.Web.Syndication;

namespace myFeed.FeedModels.Models
{
    /// <summary>
    /// News feed item. Represents saved article.
    /// </summary>
    [XmlType("PFeedItem")]
    public sealed class FeedItemModel
    {
        /// <summary>
        /// Article title.
        /// </summary>
        [XmlElement("title")] 
        public string Title { get; set; }

        /// <summary>
        /// Feed title.
        /// </summary>
        [XmlElement("feed")]
        public string FeedTitle { get; set; }

        /// <summary>
        /// Article Uri.
        /// </summary>
        [XmlElement("link")]
        public string Uri { get; set; }

        /// <summary>
        /// Preview image uri.
        /// </summary>
        [XmlElement("image")]
        public string ImageUri { get; set; }

        /// <summary>
        /// Content of the article.
        /// </summary>
        [XmlElement("content")]
        public string Content { get; set; }

        /// <summary>
        /// Date when this article was published.
        /// </summary>
        [XmlElement("PublishedDate")]
        public string PublishedDate { get; set; }

        /// <summary>
        /// Creates news item view model from syndication item.
        /// </summary>
        public static FeedItemModel FromSyndicationItem(SyndicationItem item, string feedTitle)
        {
            // Init model with easy-get properties. 
            var model = new FeedItemModel
            {
                FeedTitle = feedTitle,
                Title = WebUtility.HtmlDecode(item.Title.Text),
                Content = item.Summary?.Text ?? string.Empty,
                Uri = item.Links.FirstOrDefault()?.Uri.ToString(),
                PublishedDate = item.PublishedDate.ToString(@"yyyy-MM-dd HH:mm:ss")
            };

            // Init image properties.
            var match = Regex.Match(model.Content, @"<img(.*?)>", RegexOptions.Singleline);
            if (match.Success)
            {
                var val = match.Groups[1].Value;
                var match2 = Regex.Match(val, @"src=\""(.*?)\""", RegexOptions.Singleline);
                if (match2.Success)
                {
                    var uri = match2.Groups[1].Value;
                    if (uri.Length > 3 && uri[0] == '/' && uri[1] == '/')
                        uri = $"http:{uri}";
                    model.ImageUri = uri;
                }
            }

            return model;
        }

        /// <summary>
        /// Returns emulated tile ID.
        /// </summary>
        public string GetTileId()
        {
            var date = DateTime.Parse(PublishedDate);
            return + Title.First()
                   + date.Second.ToString()
                   + date.Minute
                   + date.Hour
                   + date.Day;
        }
    }
}
