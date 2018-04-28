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
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(settings.Max).ToList();
                await _categoryManager.Update(channel).ConfigureAwait(false);
            }

            // Extract stored articles.
            var existing = fetchables.SelectMany(i => i.Articles)
                .ToLookup(i => (i.Title?.Trim(), i.FeedTitle?.Trim()));

            // Fetch new articles and remove existing ones.
            var tasks = fetchables.Select(i => FetchAsync(i, settings.Max));
            var groupped = await Task.WhenAll(tasks).ConfigureAwait(false);
            var distinct = groupped
                .Select(i => (i.Item1, i.Item2
                    .Where(x => !existing.Contains(
                        (x.Title?.Trim(), x.FeedTitle?.Trim())))
                    .ToArray()))
                .ToList();

            // Save fetched distinct articles to disk.
            foreach (var (channel, articles) in distinct)
            {
                channel.Articles.AddRange(articles);
                await _categoryManager.Update(channel).ConfigureAwait(false);
            }

            // Return all existing articles.
            return existing.SelectMany(i => i)
                .Concat(distinct.SelectMany(i => i.Item2))
                .OrderByDescending(i => i.PublishedDate)
                .ToList()
                .AsEnumerable();
        }

        private async Task<Tuple<Channel, IEnumerable<Article>>> FetchAsync(Channel channel, int max)
        {
            var articles = await _feedFetchService.Fetch(channel.Uri).ConfigureAwait(false);
            return new Tuple<Channel, IEnumerable<Article>>(channel, articles
                .OrderByDescending(i => i.PublishedDate)
                .Take(max));
        }
    }
}