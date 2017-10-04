using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public sealed class FeedStoreService : IFeedStoreService
    {
        private readonly IArticlesRepository _articlesRepository;
        private readonly IFeedFetchService _feedFetchService;
        
        public FeedStoreService(
            IArticlesRepository articlesRepository,
            IFeedFetchService feedFetchService)
        {
            _articlesRepository = articlesRepository;
            _feedFetchService = feedFetchService;
        }
        
        public Task<(IEnumerable<Exception>,
            IOrderedEnumerable<ArticleEntity>)> GetAsync(
            IEnumerable<SourceEntity> entities) => Task.Run(async () =>
        {
            // Read items into lookup using title and date as keys.
            var sourcesList = entities.ToList();
            var existingEntities = sourcesList
                .SelectMany(i => i.Articles)
                .ToLookup(i => (i.Title, i.PublishedDate));

            // Retrieve feed based on single fetcher implementation.
            var grouppedArticles = await Task.WhenAll(sourcesList.Select(FetchAsync));
            var distinctGroupping = grouppedArticles
                .Select(i => (i.Item1, i.Item3
                    .Where(x => !existingEntities.Contains((x.Title, x.PublishedDate)))
                    .ToArray()))
                .ToList();
                
            // Save received distinct items into database.
            foreach (var grouping in distinctGroupping) 
                await _articlesRepository.InsertAsync(
                    grouping.Item1, grouping.Item2);

            // Return global join with both old and new articles.
            var flatternedArticles = distinctGroupping.SelectMany(i => i.Item2);
            var errors = grouppedArticles.Select(i => i.Item2);
            return (errors, existingEntities
                .SelectMany(i => i)
                .Concat(flatternedArticles)
                .OrderByDescending(i => i.PublishedDate));
        });

        private Task<(SourceEntity, Exception,
            IEnumerable<ArticleEntity>)> FetchAsync(
            SourceEntity sourceEntity) => Task.Run(async () =>
        {
            // Fetches single feed in a separate thread.
            var uriToFetch = sourceEntity.Uri;
            (var exception, var articles) = await _feedFetchService.FetchAsync(uriToFetch);
            return (sourceEntity, exception, articles);
        });
    }
}