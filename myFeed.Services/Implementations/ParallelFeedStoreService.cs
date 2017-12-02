using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFeedStoreService))]
    public sealed class ParallelFeedStoreService : IFeedStoreService
    {
        private readonly ICategoryStoreService _categoriesRepository;
        private readonly IFeedFetchService _feedFetchService;
        private readonly ISettingService _settingsService;
        
        public ParallelFeedStoreService(
            ICategoryStoreService categoriesRepository,
            IFeedFetchService feedFetchService,
            ISettingService settingsService)
        {
            _categoriesRepository = categoriesRepository;
            _feedFetchService = feedFetchService;
            _settingsService = settingsService;
        }
        
        public Task<Tuple<IEnumerable<Exception>, IEnumerable<Article>>> LoadAsync(
            IEnumerable<Channel> fetchableChannelsEnumerable) => Task.Run(async () =>
        {
            // Remove extra saved articles and save changes based on max articles allowed.
            var maxArticleCount = await _settingsService.GetAsync<int>("MaxArticlesPerFeed");
            var channels = fetchableChannelsEnumerable.ToList();
            await Task.WhenAll(channels
                .Where(i => i.Articles.Count > maxArticleCount)
                .Select(i => {
                    i.Articles = i.Articles
                        .OrderByDescending(x => x.PublishedDate)
                        .Take(maxArticleCount).ToList();
                    return _categoriesRepository.UpdateChannelAsync(i);
                }));

            // Form lookup with trimmed title and feed as keys.
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
            await Task.WhenAll(distinctGroupping.Select(i => _categoriesRepository
                .InsertArticleRangeAsync(i.Item1, i.Item2)));

            // Return global join with both old and new articles.
            var flatternedArticles = distinctGroupping.SelectMany(i => i.Item2);
            var errors = grouppedArticles.Select(i => i.Item2);
            return new Tuple<IEnumerable<Exception>, IEnumerable<Article>>(
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