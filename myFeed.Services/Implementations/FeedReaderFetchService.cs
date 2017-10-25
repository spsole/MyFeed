using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public sealed class FeedReaderFetchService : IFeedFetchService
    {
        private readonly IImageService _imageService;

        public FeedReaderFetchService(IImageService imageService) => _imageService = imageService;

        public async Task<(Exception, IEnumerable<Article>)> FetchAsync(string uri) 
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
                        FeedTitle = WebUtility.HtmlDecode(feed.Title),
                        Title = WebUtility.HtmlDecode(feedItem.Title),
                        Uri = feedItem.Link,
                        Read = false,
                        Fave = false
                    };
                return (default(Exception), articles);
            }
            catch (Exception exception)
            {
                return (exception, new List<Article>());
            }
        }
    }
}