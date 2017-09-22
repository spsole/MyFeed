using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Notifications.Services
{
    internal sealed class UwpFeedProcessor
    {
        private readonly ISourcesRepository _sourcesRepository;
        private readonly ISettingsService _settingsService;
        private readonly IFeedService _feedService;

        public UwpFeedProcessor(
            IFeedService feedService, 
            ISourcesRepository sourcesRepository,
            ISettingsService settingsService)
        {
            _sourcesRepository = sourcesRepository;
            _settingsService = settingsService;
            _feedService = feedService;
        }

        public async Task ProcessFeeds()
        {
            // Fetch feed for all existing sources where user allowed notifications.
            var categories = await _sourcesRepository.GetAllAsync().ConfigureAwait(false);
            var sources = categories.SelectMany(i => i.Sources).Where(i => i.Notify);
            var feed = await _feedService.RetrieveFeedsAsync(sources).ConfigureAwait(false);

            // Get preferences settings.
            var recentFetchDateTime = await _settingsService.Get<string>("LastFetched");
            var recentFetch = DateTime.Parse(recentFetchDateTime, CultureInfo.InvariantCulture);
            var needBanners = await _settingsService.Get<bool>("NeedBanners");
            var needImages = await _settingsService.Get<bool>("LoadImages");

            // Show notifications for revant articles.
            feed.Where(i => i.PublishedDate > recentFetch)
                .OrderByDescending(i => i.PublishedDate)
                .Take(15).Reverse().ToList()
                .ForEach(i => SendToastNotification(i, needBanners, needImages));

            // Update last fetch date.
            await _settingsService.Set("LastFetched", DateTime.Now.ToString(CultureInfo.InvariantCulture));
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
