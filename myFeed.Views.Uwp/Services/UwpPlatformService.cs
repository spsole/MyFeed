﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpPlatformService : IPlatformService
    {
        private readonly IDialogService _dialogService;

        public UwpPlatformService(IDialogService dialogService) => _dialogService = dialogService;

        public async Task LaunchUri(Uri uri) => await Launcher.LaunchUriAsync(uri);

        public Task Share(string content)
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (sender, args) =>
            {
                var request = args.Request;
                request.Data.SetText(content);
                request.Data.Properties.Title = "myFeed";
            };
            DataTransferManager.ShowShareUI();
            return Task.CompletedTask;
        }

        public Task CopyTextToClipboard(string text)
        {
            var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            return Task.CompletedTask;
        }

        public async Task ResetApp()
        {
            var database = await ApplicationData.Current.LocalFolder.GetFileAsync("MyFeed.db");
            await database.DeleteAsync(StorageDeleteOption.Default);
            Application.Current.Exit();
        }

        public IReadOnlyDictionary<string, string> GetDefaultSettings() => new Dictionary<string, string>
        {
            {"NeedBanners", "true"},
            {"LoadImages", "true"},
            {"NotifyPeriod", "14"},
            {"Theme", "default"},
            {"FontSize", "14"}
        };

        public IReadOnlyDictionary<ViewKey, object> GetIconsForViews() => new Dictionary<ViewKey, object>
        {
            {ViewKey.FaveView, Symbol.OutlineStar},
            {ViewKey.SettingsView, Symbol.Setting},
            {ViewKey.FeedView, Symbol.PostUpdate},
            {ViewKey.SourcesView, Symbol.List},
            {ViewKey.SearchView, Symbol.Zoom},
        };

        public async Task RegisterBackgroundTask(int freq)
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus != BackgroundAccessStatus.AlwaysAllowed &&
                backgroundAccessStatus != BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
                return;

            foreach (var task in BackgroundTaskRegistration.AllTasks.Values)
                if (task.Name == "myFeedNotify") task.Unregister(true);

            if (freq == 0) return;
            var builder = new BackgroundTaskBuilder();
            builder.SetTrigger(new TimeTrigger((uint)freq * 60, false)); // Note: minutes here; 30 = 30 mins
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.TaskEntryPoint = "myFeed.Views.Uwp.Notifications.Runner";
            builder.Name = "myFeedNotify";
            builder.Register();
        }

        public async Task RegisterTheme(string theme)
        {
            var response = await _dialogService.ShowDialogForConfirmation("restart", "need");
            if (response) Application.Current.Exit();
        }
    }
}