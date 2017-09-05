using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations {
    public class FeedService : IFeedService {
        private readonly IArticlesRepository _articlesRepository;
        private readonly IHtmlParsingService _htmlParsingService;

        public FeedService(
            IHtmlParsingService htmlParsingService,
            IArticlesRepository articlesRepository) {
            _articlesRepository = articlesRepository;
            _htmlParsingService = htmlParsingService;
        }

        public Task<IOrderedEnumerable<ArticleEntity>> RetrieveFeedsAsync(IEnumerable<SourceEntity> sourceEntities) {
            return Task.Run(async () => {

                // Get all articles and remove uncategorized ones.
                var articles = new List<ArticleEntity>(await _articlesRepository.GetAllAsync());
                var outdated = articles.Where(i => i.Source == null && !i.Fave);
                await _articlesRepository.RemoveRangeAsync(outdated);

                // Read items into dict using title and date fields as keys.
                var dictionary = articles
                    .Where(i => i.Source != null)
                    .ToDictionary(i => (i.Title, i.PublishedDate));

                // Retrieve feed based on single fetcher implementation.
                var grouppedArticles = await Task.WhenAll(sourceEntities.Select(RetrieveFeedAsync));
                var distinctArticles = grouppedArticles
                    .Select(i => {
                        foreach (var article in i.Item2)
                            article.Source = i.Item1;
                        return i;
                    })
                    .SelectMany(i => i.Item2)
                    .Where(i => !dictionary.ContainsKey((i.Title, i.PublishedDate)))
                    .ToList();

                // Write new articles into database and return global join.
                await _articlesRepository.InsertRangeAsync(distinctArticles);
                return dictionary.Values
                    .Concat(distinctArticles)
                    .OrderByDescending(i => i.PublishedDate);
            });
        }

        /// <summary>
        /// Retrieves feed using CodeHollow.FeedReader feed parser.
        /// Github: https://github.com/CodeHollow/FeedReader
        /// </summary>
        /// <param name="e">Uri to obtain.</param>
        protected virtual async Task<Tuple<SourceEntity, IEnumerable<ArticleEntity>>> RetrieveFeedAsync(SourceEntity e) {
            try {
                var feed = await FeedReader.ReadAsync(e.Uri).ConfigureAwait(false);
                var items = feed.Items.Select(i => new ArticleEntity {
                    ImageUri = _htmlParsingService.ExtractImageUrl(i.Content),
                    PublishedDate = i.PublishingDate ?? DateTime.MinValue,
                    FeedTitle = feed.Title,
                    Content = i.Content,
                    Title = i.Title,
                    Uri = i.Link,
                    Read = false,
                    Fave = false
                });
                return Tuple.Create(e, items);
            } catch (Exception) {
                return Tuple.Create(e, new List<ArticleEntity>().AsEnumerable());
            }
        }
    }
}