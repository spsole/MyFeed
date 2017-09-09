using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public class FeedService : IFeedService
    {
        private readonly IArticlesRepository _articlesRepository;
        private readonly IHtmlParsingService _htmlParsingService;

        public FeedService(
            IHtmlParsingService htmlParsingService,
            IArticlesRepository articlesRepository)
        {
            _articlesRepository = articlesRepository;
            _htmlParsingService = htmlParsingService;
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
                var dictionary = sourcesList
                    .SelectMany(i => i.Articles)
                    .ToDictionary(i => (i.Title, i.PublishedDate));

                // Retrieve feed based on single fetcher implementation.
                var distinctArticles = new List<ArticleEntity>();
                var grouppedArticles = await Task.WhenAll(sourcesList.Select(RetrieveFeedAsync));
                foreach (var grouping in grouppedArticles)
                {
                    foreach (var article in grouping.Item2)
                    {
                        var compositeKey = (article.Title, article.PublishedDate);
                        if (dictionary.ContainsKey(compositeKey)) continue;
                        grouping.Item1.Articles.Add(article);
                        distinctArticles.Add(article);
                    }
                }

                // Write new articles into database and return global join.
                await _articlesRepository.InsertAsync(distinctArticles.ToArray());
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
        protected virtual async Task<Tuple<SourceEntity, IEnumerable<ArticleEntity>>> RetrieveFeedAsync(SourceEntity e)
        {
            try
            {
                var feed = await FeedReader.ReadAsync(e.Uri).ConfigureAwait(false);
                var items = feed.Items.Select(i => new ArticleEntity
                {
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
            }
            catch (Exception)
            {
                return Tuple.Create(e, new List<ArticleEntity>().AsEnumerable());
            }
        }
    }
}