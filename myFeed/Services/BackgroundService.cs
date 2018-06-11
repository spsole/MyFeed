using System;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Platform;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IBackgroundService))]
    public sealed class BackgroundService : IBackgroundService
    {
        private readonly INotificationService _notificationService;
        private readonly IFeedStoreService _feedStoreService;
        private readonly ICategoryManager _categoryManager;
        private readonly ISettingManager _settingManager;

        public BackgroundService(
            INotificationService notificationService,
            IFeedStoreService feedStoreService,
            ICategoryManager categoryManager,
            ISettingManager settingManager)
        {
            _notificationService = notificationService;
            _feedStoreService = feedStoreService;
            _categoryManager = categoryManager;
            _settingManager = settingManager;
        }
        
        public async Task<bool> CheckForUpdates(DateTime dateTime)
        {
            var categories = await _categoryManager.GetAll().ConfigureAwait(false);
            var notifyables = categories
                .SelectMany(category => category.Channels)
                .Where(channel => channel.Notify);

            var articles = await _feedStoreService.Load(notifyables).ConfigureAwait(false);
            var settings = await _settingManager.Read().ConfigureAwait(false);
            var recentArticles = articles
                .Where(article => article.PublishedDate > settings.Fetched)
                .OrderByDescending(article => article.PublishedDate)
                .Take(15).Reverse().ToList();

            await _notificationService.SendNotifications(recentArticles).ConfigureAwait(false);
            if (!recentArticles.Any()) return false;
            settings.Fetched = dateTime;
            await _settingManager.Write(settings).ConfigureAwait(false);
            return true;
        }
    }
}