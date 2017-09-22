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
    public class FeedService : IFeedService
    {
        private readonly IArticlesRepository _articlesRepository;
        private readonly IHtmlService _htmlService;

        public FeedService(
            IHtmlService htmlService,
            IArticlesRepository articlesRepository)
        {
            _articlesRepository = articlesRepository;
            _htmlService = htmlService;
        }

        public Task<IOrderedEnumerable<ArticleEntity>> RetrieveFeedsAsync(IEnumerable<SourceEntity> sourceEntities)
        {
            return Task.Run(async () =>
            {
                // Remove articles not referenced by any source and not starred.
                var articles = await _articlesRepository.GetAllAsync();
                var outdated = articles.Where(i => i.Source == null && !i.Fave);
                await _articlesRepository.RemoveAsync(outdated.ToArray());

                // Read items into dict using title and date fields as keys.
                var sourcesList = sourceEntities.ToList();
                var existingEntities = sourcesList
                    .SelectMany(i => i.Articles)
                    .ToLookup(i => (i.Title, i.PublishedDate));

                // Retrieve feed based on single fetcher implementation.
                var grouppedArticles = await Task.WhenAll(sourcesList.Select(RetrieveFeedAsync));
                var distinctGroupping = grouppedArticles
                    .Select(i => (i.Item1, i.Item2
                        .Where(x => !existingEntities.Contains((x.Title, x.PublishedDate)))
                        .ToArray()))
                    .ToList();
                
                // Save received distinct items into database.
                foreach (var grouping in distinctGroupping) 
                    await _articlesRepository.InsertAsync(
                        grouping.Item1, grouping.Item2);

                // Return global join with both old and new articles.
                var flatternedArticles = distinctGroupping.SelectMany(i => i.Item2);
                return existingEntities
                    .SelectMany(i => i)
                    .Concat(flatternedArticles)
                    .OrderByDescending(i => i.PublishedDate);
            });
        }

        /// <summary>
        /// Retrieves feed using CodeHollow.FeedReader feed parser.
        /// Github: https://github.com/CodeHollow/FeedReader
        /// </summary>
        /// <param name="e">Uri to obtain.</param>
        protected virtual async Task<(SourceEntity, IEnumerable<ArticleEntity>)> RetrieveFeedAsync(SourceEntity e)
        {
            try
            {
                var feed = await FeedReader.ReadAsync(e.Uri).ConfigureAwait(false);
                var articles = 
                    from feedItem in feed.Items
                    let content = feedItem.Content
                    let contents = string.IsNullOrWhiteSpace(content) ? feedItem.Description : content
                    let publishedDate = feedItem.PublishingDate ?? DateTime.MinValue
                    let imageUri = _htmlService.ExtractImage(contents)
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
                return (e, articles);
            }
            catch (Exception)
            {
                return (e, new List<ArticleEntity>());
            }
        }
    }
}