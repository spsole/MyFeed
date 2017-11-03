using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public sealed class ParallelFeedStoreService : IFeedStoreService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IFeedFetchService _feedFetchService;
        private readonly ISettingsService _settingsService;
        
        public ParallelFeedStoreService(
            ICategoriesRepository categoriesRepository,
            IFeedFetchService feedFetchService,
            ISettingsService settingsService)
        {
            _categoriesRepository = categoriesRepository;
            _feedFetchService = feedFetchService;
            _settingsService = settingsService;
        }
        
        public Task<Tuple<IEnumerable<Exception>, IOrderedEnumerable<Article>>> LoadAsync(
            IEnumerable<Channel> fetchableChannelsEnumerable) => Task.Run(async () =>
        {
            var maxArticleCount = await _settingsService.GetAsync<int>("MaxArticlesPerFeed");
            var channels = fetchableChannelsEnumerable.Select(i => 
            {
                // Remove extra saved articles and save changes.
                if (i.Articles.Count <= maxArticleCount) return i;
                i.Articles = i.Articles
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(maxArticleCount).ToList();
                _categoriesRepository.UpdateChannelAsync(i);
                return i;
            });

            // Read items into lookup using title and date as keys.
            var existingLookup = channels.SelectMany(i => i.Articles)
                .ToLookup(i => (i.Title?.Trim(), i.FeedTitle?.Trim()));

            // Retrieve feed based on single fetcher implementation.
            var fetchTasks = channels.Select(i => FetchAsync(i, maxArticleCount));
            var grouppedArticles = await Task.WhenAll(fetchTasks);
            var distinctGroupping = grouppedArticles
                .Select(i => (i.Item1, i.Item3
                    .Where(x => !existingLookup.Contains(
                        (x.Title?.Trim(), x.FeedTitle?.Trim())))
                    .ToArray()))
                .ToList();
                
            // Save received distinct items into database.
            foreach (var grouping in distinctGroupping) 
                await _categoriesRepository.InsertArticleRangeAsync(
                    grouping.Item1, grouping.Item2);

            // Return global join with both old and new articles.
            var flatternedArticles = distinctGroupping.SelectMany(i => i.Item2);
            var errors = grouppedArticles.Select(i => i.Item2);
            return new Tuple<IEnumerable<Exception>, IOrderedEnumerable<Article>>(
                errors, existingLookup
                .SelectMany(i => i)
                .Concat(flatternedArticles)
                .OrderByDescending(i => i.PublishedDate));
        });

        private Task<Tuple<Channel, Exception, IEnumerable<Article>>> FetchAsync(
            Channel fetchableChannel, int maxArticleCount) => Task.Run(async () =>
        {
            // Fetches single feed in a separate thread.
            var uriToFetch = fetchableChannel.Uri;
            (var exception, var articles) = await _feedFetchService.FetchAsync(uriToFetch);
            return new Tuple<Channel, Exception, IEnumerable<Article>>(
                fetchableChannel, exception, articles
                .OrderByDescending(i => i.PublishedDate)
                .Take(maxArticleCount));
        });
    }
}