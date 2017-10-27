using System;
using System.Threading.Tasks;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SettingsViewModel
    {
        public ObservableProperty<string> Theme { get; }
        public ObservableProperty<bool> LoadImages { get; }
        public ObservableProperty<bool> NeedBanners { get; }
        public ObservableProperty<int> NotifyPeriod { get; }
        public ObservableProperty<int> FontSize { get; }

        public ObservableCommand ImportOpml { get; }
        public ObservableCommand ExportOpml { get; }
        public ObservableCommand Reset { get; }
        public ObservableCommand Load { get; }

        public SettingsViewModel(
            ITranslationsService translationsService,
            IFilePickerService filePickerService,
            ISettingsService settingsService,
            IPlatformService platformService,
            IDialogService dialogService,
            IOpmlService opmlService)
        {
            Theme = string.Empty;
            (FontSize, NotifyPeriod) = (0, 0);
            (LoadImages, NeedBanners) = (true, true);
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
                    StartTracking(NotifyPeriod, nameof(NotifyPeriod), platformService.RegisterBackgroundTask),
                    StartTracking(NeedBanners, nameof(NeedBanners), o => Task.CompletedTask),
                    StartTracking(LoadImages, nameof(LoadImages), o => Task.CompletedTask),
                    StartTracking(FontSize, nameof(FontSize), o => Task.CompletedTask),
                    StartTracking(Theme, nameof(Theme), platformService.RegisterTheme)
                );
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
            });
        }
    }
}