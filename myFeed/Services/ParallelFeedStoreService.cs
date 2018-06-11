using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IFeedStoreService))]
    public sealed class ParallelFeedStoreService : IFeedStoreService
    {
        private readonly IFeedFetchService _feedFetchService;
        private readonly ICategoryManager _categoryManager;
        private readonly ISettingManager _settingManager;
        
        public ParallelFeedStoreService(
            IFeedFetchService feedFetchService,
            ICategoryManager categoryManager,
            ISettingManager settingManager)
        {
            _feedFetchService = feedFetchService;
            _categoryManager = categoryManager;
            _settingManager = settingManager;
        }
        
        public async Task<IEnumerable<Article>> Load(IEnumerable<Channel> channels) 
        {
            // Read settings max value and remove extra articles.
            var settings = await _settingManager.Read().ConfigureAwait(false);
            var fetchables = channels.ToList();
            foreach (var channel in fetchables)
            {
                if (channel.Articles.Count <= settings.Max) continue;
                channel.Articles = channel.Articles
                    .OrderByDescending(article => article.PublishedDate)
                    .Take(settings.Max).ToList();
                await _categoryManager.Update(channel).ConfigureAwait(false);
            }

            // Extract stored articles.
            var existing = fetchables.SelectMany(channel => channel.Articles)
                .ToLookup(article => (article.Title?.Trim(), article.FeedTitle?.Trim()));

            // Fetch new articles and remove existing ones.
            var tasks = fetchables.Select(channel => FetchAsync(channel, settings.Max));
            var groupped = await Task.WhenAll(tasks).ConfigureAwait(false);
            var distinct = groupped
                .Select(tuple => (tuple.Item1, tuple.Item2
                    .Where(article => !existing.Contains(
                        (article.Title?.Trim(), article.FeedTitle?.Trim())))
                    .ToArray()))
                .ToList();

            // Save fetched distinct articles to disk.
            foreach (var (channel, articles) in distinct)
            {
                channel.Articles.AddRange(articles);
                await _categoryManager.Update(channel).ConfigureAwait(false);
            }

            // Return all existing articles.
            return existing.SelectMany(grouping => grouping)
                .Concat(distinct.SelectMany(tuple => tuple.Item2))
                .OrderByDescending(article => article.PublishedDate)
                .ToList().AsEnumerable();
        }

        private async Task<Tuple<Channel, IEnumerable<Article>>> FetchAsync(Channel channel, int max)
        {
            var articles = await _feedFetchService.Fetch(channel.Uri).ConfigureAwait(false);
            return new Tuple<Channel, IEnumerable<Article>>(channel, articles
                .OrderByDescending(article => article.PublishedDate)
                .Take(max));
        }
    }
}