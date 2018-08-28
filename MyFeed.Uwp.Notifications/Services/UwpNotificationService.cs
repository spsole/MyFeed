﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;

namespace MyFeed.Uwp.Notifications.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(INotificationService))]
    internal sealed class UwpNotificationService : INotificationService
    {
        private readonly ISettingManager _settingManager;

        public UwpNotificationService(ISettingManager settingManager) => _settingManager = settingManager;

        public async Task SendNotifications(IEnumerable<Article> articles)
        {
            var settings = await _settingManager.Read();
            var articlesList = articles.ToList();
            foreach (var article in articlesList)
            {
                var identifier = article.Id.ToString();
                var uri = settings.Images ? article.ImageUri : string.Empty;
                var template = GetNotificationTemplate(article.Title,
                    article.FeedTitle, uri, identifier);

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(template);
                var notification = new ToastNotification(xmlDocument) { SuppressPopup = !settings.Banners };
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }

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

        private static string GetNotificationTemplate(string title, string message, string image, string id) => $@"
        <toast launch='{id}'>
            <visual>
                <binding template='ToastGeneric'>
                    <text>{title}</text>
                    <text>{message}</text>
                    {(Uri.IsWellFormedUriString(image, UriKind.Absolute) 
                        ? $@"<image src='{image}' placement='appLogoOverride' hint-crop='circle'/>"
                        : string.Empty)}
                </binding>
            </visual>
            <actions>
                <action activationType='foreground' content='Read more' arguments='{id}'/>
            </actions>
        </toast>";
    }
}
