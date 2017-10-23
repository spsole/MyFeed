using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;

namespace myFeed.Views.Uwp.Notifications
{
    internal sealed class UwpNotificationService : INotificationService
    {
        private readonly ISettingsService _settingsService;

        public UwpNotificationService(ISettingsService settingsService) => _settingsService = settingsService;

        public async Task SendNotifications(IEnumerable<Article> articles)
        {
            // Prevent possible multiple enumeration of enumerable.
            var articlesList = articles.ToList();

            // Send windows tile notifications to display on home screen.
            var updateManager = TileUpdateManager.CreateTileUpdaterForApplication();
            updateManager.EnableNotificationQueue(true);
            updateManager.Clear();
            foreach (var article in articlesList.Take(5))
            {
                var template = GetTileTemplate(article.FeedTitle, article.Title);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(template);
                var notification = new TileNotification(xmlDocument);
                updateManager.Update(notification);
            }

            // Send toast notifications for notifications and action center.
            var needImages = await _settingsService.GetAsync<bool>("LoadImages");
            var needBanners = await _settingsService.GetAsync<bool>("NeedBanners");
            foreach (var article in articlesList)
            {
                var identifier = article.Id.ToString();
                var uri = needImages ? article.ImageUri : string.Empty;
                var template = GetNotificationTemplate(article.Title,
                    article.FeedTitle, uri, identifier);

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(template);
                var notification = new ToastNotification(xmlDocument) { SuppressPopup = !needBanners };
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }
        }

        private static string GetTileTemplate(string title, string message) => $@"
            <tile>
                <visual>
                    <binding template='TileMedium'>
                        <text hint-style='captionSubtle'>{title}</text>
                        <text hint-style='base' hint-wrap='true'>{message}</text>
                    </binding>
                    <binding template='TileWide'>
                        <text hint-style='captionSubtle'>{title}</text>
                        <text hint-style='base' hint-wrap='true'>{message}</text>
                    </binding>
                    <binding template='TileLarge'>
                        <text hint-style='captionSubtle'>{title}</text>
                        <text hint-style='base' hint-wrap='true'>{message}</text>
                    </binding>
                </visual>
            </tile>";

        private static string GetNotificationTemplate(string title, string message, string imageUri, string id) => $@"
            <toast launch='{id}'>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>{title}</text>
                        <text>{message}</text>
                        {(Uri.IsWellFormedUriString(imageUri, UriKind.Absolute) 
                          ? $@"<image src='{imageUri}' placement='appLogoOverride' hint-crop='circle'/>"
                          : string.Empty)}
                    </binding>
                </visual>
                <actions>
                    <action activationType='foreground' content='Read more' arguments='{id}'/>
                </actions>
            </toast>";
    }
}
