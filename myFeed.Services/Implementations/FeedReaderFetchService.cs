using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public sealed class FeedReaderFetchService : IFeedFetchService
    {
        private readonly IExtractImageService _extractImageService;
        
        public FeedReaderFetchService(IExtractImageService extractImageService)
        {
            _extractImageService = extractImageService;
        }

        public async Task<(Exception, IEnumerable<ArticleEntity>)> FetchAsync(string uri) 
        {
            try
            {
                // Fetch RSS/Atom feeds using CodeHollow.FeedReader library.
                // Github: https://github.com/codehollow/FeedReader
                var feed = await FeedReader.ReadAsync(uri);
                var articles =
                    from feedItem in feed.Items
                    let content = feedItem.Content
                    let contents = string.IsNullOrWhiteSpace(content) ? feedItem.Description : content
                    let publishedDate = feedItem.PublishingDate ?? DateTime.MinValue
                    let imageUri = _extractImageService.ExtractImage(contents)
                    select new ArticleEntity
                    {
                        ImageUri = imageUri,
                        PublishedDate = publishedDate,
                        FeedTitle = feed.Title,
                        Content = contents,
                        Title = feedItem.Title,
                        Uri = feedItem.Link,
                        Read = false,
                        Fave = false
                    };
                return (default(Exception), articles);
            }
            catch (Exception exception)
            {
                return (exception, new List<ArticleEntity>());
            }
        }
    }
}