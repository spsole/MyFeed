using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Controls;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpPlatformService : IPlatformService
    {
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

        public Task RegisterBackgroundTask(int freq) => Task.Delay(1);
        public Task RegisterBanners(bool needBanners) => Task.Delay(1);
        public Task RegisterTheme(string theme) => Task.Delay(1);
    }
}