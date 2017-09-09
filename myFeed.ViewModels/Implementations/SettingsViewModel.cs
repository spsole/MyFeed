using System;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    /// <summary>
    /// Settings ViewModel.
    /// </summary>
    public sealed class SettingsViewModel
    {
        /// <summary>
        /// Instantiates new ViewModel.
        /// </summary>
        public SettingsViewModel(
            IOpmlService opmlService,
            IPlatformProvider platformProvider,
            IConfigurationRepository configRepository)
        {
            Theme = new ObservableProperty<string>();
            FontSize = new ObservableProperty<int>();
            LoadImages = new ObservableProperty<bool>();
            NotifyPeriod = new ObservableProperty<int>();
            NeedBanners = new ObservableProperty<bool>();

            ImportOpml = new ActionCommand(opmlService.ImportOpmlFeeds);
            ExportOpml = new ActionCommand(opmlService.ExportOpmlFeeds);
            OpenCredits = new ActionCommand(() => platformProvider.LaunchUri(new Uri("https://worldbeater.github.io")));
            Load = new ActionCommand(async () =>
            {
                // Resolve all default settings.
                FontSize.Value = int.Parse(await Get("FontSize"));
                LoadImages.Value = bool.Parse(await Get("LoadImages"));
                NotifyPeriod.Value = int.Parse(await Get("NotifyPeriod"));
                NeedBanners.Value = bool.Parse(await Get("NeedBanners"));
                Theme.Value = await Get("Theme");

                // Subscribe on property change.
                Subscribe(FontSize, "FontSize");
                Subscribe(LoadImages, "LoadImages");
                Subscribe(NotifyPeriod, "NotifyPeriod", platformProvider.RegisterBackgroundTask);
                Subscribe(NeedBanners, "NeedBanners", platformProvider.RegisterBanners);
                Subscribe(Theme, "Theme", platformProvider.RegisterTheme);
            });

            void Subscribe<T>(ObservableProperty<T> property, string key, Func<T, Task> updater = null)
            {
                property.PropertyChanged += async (_, __) =>
                {
                    var value = property.Value;
                    var task = updater?.Invoke(value);
                    if (task != null) await task;
                    await configRepository.SetByNameAsync(key, value.ToString());
                };
            }

            Task<string> Get(string name) => configRepository.GetByNameAsync(name);
        }

        /// <summary>
        /// Selected font size.
        /// </summary>
        public ObservableProperty<int> FontSize { get; }

        /// <summary>
        /// Download images or not?
        /// </summary>
        public ObservableProperty<bool> LoadImages { get; }

        /// <summary>
        /// Selected theme.
        /// </summary>
        public ObservableProperty<string> Theme { get; }

        /// <summary>
        /// True if need banners.
        /// </summary>
        public ObservableProperty<bool> NeedBanners { get; }

        /// <summary> 
        /// True if notifications needed. 
        /// </summary>
        public ObservableProperty<int> NotifyPeriod { get; }

        /// <summary>
        /// Imports feeds from Opml.
        /// </summary>
        public ActionCommand ImportOpml { get; }

        /// <summary>
        /// Exports feeds to Opml.
        /// </summary>
        public ActionCommand ExportOpml { get; }

        /// <summary>
        /// Opens webpage with credits.
        /// </summary>
        public ActionCommand OpenCredits { get; }

        /// <summary>
        /// Loads settings into UI.
        /// </summary>
        public ActionCommand Load { get; }
    }
}