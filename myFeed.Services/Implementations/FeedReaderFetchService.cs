using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    public sealed class FeedReaderFetchService : IFeedFetchService
    {
        private readonly IImageService _imageService;

        public FeedReaderFetchService(IImageService imageService) => _imageService = imageService;

        public async Task<Tuple<Exception, IEnumerable<Article>>> FetchAsync(string uri) 
        {
            try
            {
                var feed = await FeedReader.ReadAsync(uri);
                var articles =
                    from feedItem in feed.Items
                    let content = feedItem.Content
                    let contents = string.IsNullOrWhiteSpace(content) ? feedItem.Description : content
                    let publishedDate = feedItem.PublishingDate ?? DateTime.MinValue
                    let imageUri = _imageService.ExtractImageUri(contents)
                    select new Article
                    {
                        ImageUri = imageUri,
                        PublishedDate = publishedDate,
                        Content = WebUtility.HtmlDecode(contents),
                        FeedTitle = WebUtility.HtmlDecode(feed.Title) ?? string.Empty,
                        Title = WebUtility.HtmlDecode(feedItem.Title) ?? string.Empty,
                        Uri = feedItem.Link,
                        Read = false,
                        Fave = false
                    };
                return new Tuple<Exception, IEnumerable<Article>>(default(Exception), articles);
            }
            catch (Exception exception)
            {
                return new Tuple<Exception, IEnumerable<Article>>(exception, new List<Article>());
            }
        }
    }
}