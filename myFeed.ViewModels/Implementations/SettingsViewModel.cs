using System;
using System.Threading.Tasks;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SettingsViewModel
    {
        public SettingsViewModel(
            IOpmlService opmlService,
            ISettingsService settingsService,
            IPlatformService platformService)
        {
            Theme = new Property<string>();
            FontSize = new Property<int>();
            LoadImages = new Property<bool>();
            NotifyPeriod = new Property<int>();
            NeedBanners = new Property<bool>();
            ImportOpml = new Command(opmlService.ImportOpmlFeeds);
            ExportOpml = new Command(opmlService.ExportOpmlFeeds);
            Load = new Command(async () =>
            {
                NotifyPeriod.Value = await settingsService.Get<int>("NotifyPeriod");
                NeedBanners.Value = await settingsService.Get<bool>("NeedBanners"); 
                LoadImages.Value = await settingsService.Get<bool>("LoadImages");
                FontSize.Value = await settingsService.Get<int>("FontSize");
                Theme.Value = await settingsService.Get<string>("Theme");
                
                Subscribe(Theme, "Theme", platformService.RegisterTheme);
                Subscribe(FontSize, "FontSize", o => Task.CompletedTask);
                Subscribe(LoadImages, "LoadImages", o => Task.CompletedTask);
                Subscribe(NeedBanners, "NeedBanners", platformService.RegisterBanners);
                Subscribe(NotifyPeriod, "NotifyPeriod", platformService.RegisterBackgroundTask);
                
                void Subscribe<T>(Property<T> prop, string key, Func<T, Task> clb) where T : IConvertible
                {
                    prop.PropertyChanged += async (o, args) =>
                    {
                        await clb.Invoke(prop.Value);
                        await settingsService.Set(key, prop.Value);
                    };
                }
            });
        }

        /// <summary>
        /// Selected theme.
        /// </summary>
        public Property<string> Theme { get; }

        /// <summary>
        /// Download images or not?
        /// </summary>
        public Property<bool> LoadImages { get; }

        /// <summary>
        /// True if need banners.
        /// </summary>
        public Property<bool> NeedBanners { get; }

        /// <summary> 
        /// True if notifications needed. 
        /// </summary>
        public Property<int> NotifyPeriod { get; }

        /// <summary>
        /// Selected font size.
        /// </summary>
        public Property<int> FontSize { get; }

        /// <summary>
        /// Imports feeds from Opml.
        /// </summary>
        public Command ImportOpml { get; }

        /// <summary>
        /// Exports feeds to Opml.
        /// </summary>
        public Command ExportOpml { get; }

        /// <summary>
        /// Loads settings into UI.
        /// </summary>
        public Command Load { get; }
    }
}