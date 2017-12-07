using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IBackgroundService))]
    public sealed class BackgroundService : IBackgroundService
    {
        private readonly ICategoryStoreService _categoriesRepository;
        private readonly INotificationService _notificationService;
        private readonly IFeedStoreService _feedStoreService;
        private readonly ISettingService _settingsService;

        public BackgroundService(
            ICategoryStoreService categoriesRepository,
            INotificationService notificationService,
            IFeedStoreService feedStoreService, 
            ISettingService settingsService)
        {
            _categoriesRepository = categoriesRepository;
            _notificationService = notificationService;
            _feedStoreService = feedStoreService;
            _settingsService = settingsService;
        }
        
        public async Task CheckForUpdates(DateTime dateTime)
        {
            var categories = await _categoriesRepository.GetAllAsync();
            var feed = await _feedStoreService.LoadAsync(categories
                .SelectMany(i => i.Channels).Where(i => i.Notify));

            var lastFetched = await _settingsService.GetAsync<DateTime>("LastFetched");
            var recentItems = feed.Where(i => i.PublishedDate > lastFetched)
                .OrderByDescending(i => i.PublishedDate).Take(15).Reverse().ToList();

            await _notificationService.SendNotifications(recentItems);
            if (recentItems.Any()) await _settingsService.SetAsync("LastFetched", 
                dateTime.ToString(CultureInfo.InvariantCulture));
        }
    }
}