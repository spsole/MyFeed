using System;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SettingsViewModel
    {
        public SettingsViewModel(
            IOpmlService opmlService,
            IPlatformService platformService,
            IConfigurationRepository configRepository)
        {
            Theme = new ObservableProperty<string>();
            FontSize = new ObservableProperty<int>();
            LoadImages = new ObservableProperty<bool>();
            NotifyPeriod = new ObservableProperty<int>();
            NeedBanners = new ObservableProperty<bool>();
            ImportOpml = new ActionCommand(opmlService.ImportOpmlFeeds);
            ExportOpml = new ActionCommand(opmlService.ExportOpmlFeeds);
            Load = new ActionCommand(async () =>
            {
                // Resolve all default settings.
                Theme.Value = await Get("Theme", "default");
                FontSize.Value = int.Parse(await Get("FontSize", "14"));
                NotifyPeriod.Value = int.Parse(await Get("NotifyPeriod", "1"));
                LoadImages.Value = bool.Parse(await Get("LoadImages", "true"));
                NeedBanners.Value = bool.Parse(await Get("NeedBanners", "true"));

                // Subscribe on property change.
                Subscribe(FontSize, "FontSize");
                Subscribe(LoadImages, "LoadImages");
                Subscribe(NotifyPeriod, "NotifyPeriod", platformService.RegisterBackgroundTask);
                Subscribe(NeedBanners, "NeedBanners", platformService.RegisterBanners);
                Subscribe(Theme, "Theme", platformService.RegisterTheme);
            });
            OpenCredits = new ActionCommand(async () =>
            {
                await platformService.LaunchUri(new Uri("https://worldbeater.github.io"));
            });
            
            void Subscribe<T>(ObservableProperty<T> property, string key, Func<T, Task> updater = null)
            {
                property.PropertyChanged += async (o, args) =>
                {
                    var value = property.Value;
                    var task = updater?.Invoke(value);
                    if (task != null) await task;
                    await configRepository.SetByNameAsync(key, value.ToString());
                };
            }
            
            async Task<string> Get(string key, string fallback)
            {
                var value = await configRepository.GetByNameAsync(key);
                if (value != null) return value;
                await configRepository.SetByNameAsync(key, fallback);
                return fallback;
            }
        }

        /// <summary>
        /// Selected theme.
        /// </summary>
        public ObservableProperty<string> Theme { get; }

        /// <summary>
        /// Download images or not?
        /// </summary>
        public ObservableProperty<bool> LoadImages { get; }

        /// <summary>
        /// True if need banners.
        /// </summary>
        public ObservableProperty<bool> NeedBanners { get; }

        /// <summary> 
        /// True if notifications needed. 
        /// </summary>
        public ObservableProperty<int> NotifyPeriod { get; }

        /// <summary>
        /// Selected font size.
        /// </summary>
        public ObservableProperty<int> FontSize { get; }
        
        /// <summary>
        /// Opens webpage with credits.
        /// </summary>
        public ActionCommand OpenCredits { get; }

        /// <summary>
        /// Imports feeds from Opml.
        /// </summary>
        public ActionCommand ImportOpml { get; }

        /// <summary>
        /// Exports feeds to Opml.
        /// </summary>
        public ActionCommand ExportOpml { get; }

        /// <summary>
        /// Loads settings into UI.
        /// </summary>
        public ActionCommand Load { get; }
    }
}