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
        
        public async Task CheckForUpdates(DateTime dateTime)
        {
            var categories = await _categoryManager.GetAllAsync();
            var feed = await _feedStoreService.LoadAsync(categories
                .SelectMany(i => i.Channels)
                .Where(i => i.Notify));

            var settings = await _settingManager.Read();
            var recentItems = feed
                .Where(i => i.PublishedDate > settings.Fetched)
                .OrderByDescending(i => i.PublishedDate)
                .Take(15).Reverse().ToList();

            await _notificationService.SendNotifications(recentItems);
            if (!recentItems.Any()) return;
            settings.Fetched = dateTime;
            await _settingManager.Write(settings);
        }
    }
}