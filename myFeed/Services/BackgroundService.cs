using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Platform;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IBackgroundService))]
    public sealed class BackgroundService : IBackgroundService
    {
        private readonly ICategoryManager _categoryManager;
        private readonly INotificationService _notificationService;
        private readonly IFeedStoreService _feedStoreService;
        private readonly ISettingManager _settingManager;

        public BackgroundService(
            ICategoryManager categoryManager,
            INotificationService notificationService,
            IFeedStoreService feedStoreService, 
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
                .SelectMany(i => i.Channels).Where(i => i.Notify));

            var lastFetched = await _settingManager.GetAsync<DateTime>("LastFetched");
            var recentItems = feed.Where(i => i.PublishedDate > lastFetched)
                .OrderByDescending(i => i.PublishedDate).Take(15).Reverse().ToList();

            await _notificationService.SendNotifications(recentItems);
            if (recentItems.Any()) await _settingManager.SetAsync("LastFetched", 
                dateTime.ToString(CultureInfo.InvariantCulture));
        }
    }
}