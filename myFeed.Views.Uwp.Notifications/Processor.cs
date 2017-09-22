using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Notifications
{
    internal sealed class Processor
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ISourcesRepository _sourcesRepository;
        private readonly IFeedService _feedService;

        public Processor(
            IFeedService feedService, 
            ISourcesRepository sourcesRepository,
            IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _sourcesRepository = sourcesRepository;
            _feedService = feedService;
        }

        public async Task ProcessFeeds()
        {
            // Fetch feed for all existing sources where user allowed notifications.
            var categories = await _sourcesRepository.GetAllAsync().ConfigureAwait(false);
            var sources = categories.SelectMany(i => i.Sources).Where(i => i.Notify);
            var feed = await _feedService.RetrieveFeedsAsync(sources).ConfigureAwait(false);

            // Get preferences settings.
            var recentFetch = await GetStoredDatetime().ConfigureAwait(false);
            var needBanners = await GetStoredBannersPreference();
            var needImages = await GetStoredImagesPreference();

            // Show notifications for revant articles.
            feed.Where(i => i.PublishedDate > recentFetch)
                .OrderByDescending(i => i.PublishedDate)
                .Take(15).Reverse().ToList()
                .ForEach(i => SendToastNotification(i, needBanners, needImages));

            // Update last fetch date.
            await SetCurrentDateTime().ConfigureAwait(false);
        }

        private async Task<bool> GetStoredBannersPreference()
        {
            var needBanners = await _configurationRepository.GetByNameAsync("NeedBanners");
            return !bool.TryParse(needBanners, out var result) || result;
        }

        private async Task<bool> GetStoredImagesPreference()
        {
            var needImages = await _configurationRepository.GetByNameAsync("LoadImages");
            return !bool.TryParse(needImages, out var result) || result;
        }

        private async Task<DateTime> GetStoredDatetime()
        {
            var lastFetched = await _configurationRepository.GetByNameAsync("LastFetched");
            return lastFetched != null ? DateTime.Parse(lastFetched, CultureInfo.InvariantCulture) : DateTime.MinValue;
        }

        private Task SetCurrentDateTime()
        {
            var stringifiedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            return _configurationRepository.SetByNameAsync("LastFetched", stringifiedDate);
        }

        private static void SendToastNotification(ArticleEntity articleEntity, bool needBanners, bool needImages)
        {
            var imageString =
                Uri.IsWellFormedUriString(articleEntity.ImageUri, UriKind.Absolute) && needImages
                    ? $@"<image src='{articleEntity.ImageUri}' placement='appLogoOverride' hint-crop='circle'/>"
                    : string.Empty;
            var tileXmlString = $@"
                <toast launch='{articleEntity.Id}'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{articleEntity.Title}</text>
                            <text>{articleEntity.FeedTitle}</text>
                            {imageString}
                        </binding>
                    </visual>
                    <actions>
                        <action activationType='foreground' content='Read more' arguments='{articleEntity.Id}'/>
                    </actions>
                </toast>";
            var xmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDocument.LoadXml(tileXmlString);
            var notification = new ToastNotification(xmlDocument) { SuppressPopup = !needBanners };
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
