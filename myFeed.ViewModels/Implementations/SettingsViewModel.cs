using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(SettingsViewModel))]
    public sealed class SettingsViewModel
    {
        public ObservableProperty<string> Version { get; }
        public ObservableProperty<string> Theme { get; }
        public ObservableProperty<bool> LoadImages { get; }
        public ObservableProperty<bool> NeedBanners { get; }
        public ObservableProperty<int> NotifyPeriod { get; }
        public ObservableProperty<int> MaxArticlesPerFeed { get; }
        public ObservableProperty<int> FontSize { get; }

        public ObservableCommand LeaveFeedback { get; }
        public ObservableCommand LeaveReview { get; }
        public ObservableCommand ImportOpml { get; }
        public ObservableCommand ExportOpml { get; }
        public ObservableCommand Reset { get; }
        public ObservableCommand Load { get; }

        public SettingsViewModel(
            ITranslationsService translationsService,
            IFilePickerService filePickerService,
            IPackagingService packagingService,
            ISettingService settingsService,
            IPlatformService platformService,
            IDialogService dialogService,
            IOpmlService opmlService)
        {
            Theme = string.Empty;
            Version = packagingService.Version;
            (LoadImages, NeedBanners) = (true, true);
            (FontSize, NotifyPeriod, MaxArticlesPerFeed) = (0, 0, 0);
            LeaveFeedback = new ObservableCommand(packagingService.LeaveFeedback);
            LeaveReview = new ObservableCommand(packagingService.LeaveReview);
            ImportOpml = new ObservableCommand(async () =>
            {
                var stream = await filePickerService.PickFileForReadAsync();
                await opmlService.ImportOpmlFeedsAsync(stream);
            });
            ExportOpml = new ObservableCommand(async () =>
            {
                var stream = await filePickerService.PickFileForWriteAsync();
                await opmlService.ExportOpmlFeedsAsync(stream);
            });
            Reset = new ObservableCommand(async () =>
            {
                var response = await dialogService.ShowDialogForConfirmation(
                    translationsService.Resolve("ResetAppNoRestore"),
                    translationsService.Resolve("Notification"));
                if (response) await platformService.ResetApp();
            });
            Load = new ObservableCommand(async () =>
            {
                await Task.WhenAll(
                    StartTracking(NotifyPeriod, "NotifyPeriod", platformService.RegisterBackgroundTask),
                    StartTracking(MaxArticlesPerFeed, "MaxArticlesPerFeed", o => Task.CompletedTask),
                    StartTracking(NeedBanners, "NeedBanners", o => Task.CompletedTask),
                    StartTracking(LoadImages, "LoadImages", o => Task.CompletedTask),
                    StartTracking(FontSize, "FontSize", o => Task.CompletedTask),
                    StartTracking(Theme, "Theme", platformService.RegisterTheme)
                );
            });
            async Task StartTracking<T>(ObservableProperty<T> property, string key, 
                Func<T, Task> callback) where T : IConvertible
            {
                property.Value = await settingsService.GetAsync<T>(key);
                property.PropertyChanged += async (o, args) =>
                {
                    var value = property.Value;
                    await callback.Invoke(value);
                    await settingsService.SetAsync(key, value);
                };
            }
        }
    }
}