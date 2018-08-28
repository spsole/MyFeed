using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DryIocAttributes;
using MyFeed.Platform;
using MyFeed.Uwp.Notifications;
using MyFeed.ViewModels;
using LiteDB;

namespace MyFeed.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IPlatformService))]
    public sealed class UwpPlatformService : IPlatformService
    {
        private readonly LiteDatabase _liteDatabase;
        private readonly ResourceLoader _resources = ResourceLoader.GetForViewIndependentUse();
        private readonly Dictionary<string, ElementTheme> _themes = new Dictionary<string, ElementTheme>
        {
            {"dark", ElementTheme.Dark}, 
            {"light", ElementTheme.Light},
            {"default", ElementTheme.Default}
        };

        public UwpPlatformService(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public IReadOnlyDictionary<Type, (string, object)> Icons => new Dictionary<Type, (string, object)>
        {
            {typeof(FeedViewModel), (_resources.GetString("FeedViewMenuItem"), Symbol.PostUpdate)},
            {typeof(FaveViewModel), (_resources.GetString("FaveViewMenuItem"), Symbol.OutlineStar)},
            {typeof(ChannelViewModel), (_resources.GetString("SourcesViewMenuItem"), Symbol.List)},
            {typeof(SearchViewModel), (_resources.GetString("SearchViewMenuItem"), Symbol.Zoom)},
            {typeof(SettingViewModel), (_resources.GetString("SettingsViewMenuItem"), Symbol.Setting)}
        };

        public async Task LaunchUri(Uri uri) => await Launcher.LaunchUriAsync(uri);

        public Task Share(string content)
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (sender, args) =>
            {
                args.Request.Data.SetText(content);
                args.Request.Data.Properties.Title = "MyFeed";
            };
            DataTransferManager.ShowShareUI();
            return Task.CompletedTask;
        }

        public Task CopyTextToClipboard(string text)
        {
            var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
            dataPackage.SetText(text);
            Clipboard.Clear();
            Clipboard.SetContent(dataPackage);
            return Task.CompletedTask;
        }

        public async Task ResetApp()
        {
            _liteDatabase.Dispose();
            var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            foreach (var file in files) await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            Application.Current.Exit();
        }
        
        public async Task RegisterBackgroundTask(int freq)
        {
            var _ = await BackgroundExecutionManager.RequestAccessAsync();
            foreach (var task in BackgroundTaskRegistration.AllTasks)
                if (task.Value.Name == "MyFeedNotify")
                    task.Value.Unregister(true);

            if (freq == 0) return;
            if (freq < 30) freq = 30;
            var builder = new BackgroundTaskBuilder {Name = "MyFeedNotify"};
            builder.SetTrigger(new TimeTrigger((uint) freq, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.TaskEntryPoint = typeof(Runner).FullName;
            builder.Register();
        }

        public Task RegisterTheme(string theme)
        {
            var contentElement = (Frame)Window.Current.Content;
            contentElement.RequestedTheme = _themes[theme];
            return Task.CompletedTask;
        }
    }
}