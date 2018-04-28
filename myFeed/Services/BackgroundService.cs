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
        private readonly ICategoryManager _categoryManager;
        private readonly INotificationService _notificationService;
        private readonly IFeedStoreService _feedStoreService;
        private readonly ISettingManager _settingManager;

        public BackgroundService(
            INotificationService notificationService,
            IFeedStoreService feedStoreService,
            ICategoryManager categoryManager,
            ISettingManager settingManager)
        {
            _categoryManager = categoryManager;
            _notificationService = notificationService;
            _feedStoreService = feedStoreService;
            _settingManager = settingManager;
        }
        
        public async Task<bool> CheckForUpdates(DateTime dateTime)
        {
            var categories = await _categoryManager.GetAll().ConfigureAwait(false);
            var articles = await _feedStoreService.Load(categories
                .SelectMany(i => i.Channels)
                .Where(i => i.Notify))
                .ConfigureAwait(false);

            var settings = await _settingManager.Read().ConfigureAwait(false);
            var recent = articles
                .Where(i => i.PublishedDate > settings.Fetched)
                .OrderByDescending(i => i.PublishedDate)
                .Take(15).Reverse().ToList();

            await _notificationService
                .SendNotifications(recent)
                .ConfigureAwait(false);
            
            if (!recent.Any()) return false;
            settings.Fetched = dateTime;
            await _settingManager
                .Write(settings)
                .ConfigureAwait(false);
            return true;
        }
    }
}