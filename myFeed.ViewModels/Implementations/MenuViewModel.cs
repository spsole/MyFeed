using System;
using System.Collections.ObjectModel;
using System.Linq;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class MenuViewModel
    {
        public MenuViewModel(
            IPlatformService platformService,
            ISettingsService settingsService,
            INavigationService navigationService,
            ITranslationsService translationsService)
        {
            Items = new ObservableCollection<Tuple<string, object, Command, Type>>();
            SelectedIndex = new Property<int>();
            Load = new Command(async () =>
            {
                await navigationService.Navigate<FeedViewModel>();
                var theme = await settingsService.Get<string>("Theme");
                await platformService.RegisterTheme(theme);
                var freq = await settingsService.Get<int>("NotifyPeriod");
                await platformService.RegisterBackgroundTask(freq);
                await settingsService.Set("LastFetched", DateTime.Now);
            });
            
            CreateItem<FeedViewModel>("FeedViewMenuItem");
            CreateItem<FaveViewModel>("FaveViewMenuItem");
            CreateItem<SourcesViewModel>("SourcesViewMenuItem");
            CreateItem<SearchViewModel>("SearchViewMenuItem");
            CreateItem<SettingsViewModel>("SettingsViewMenuItem");
            navigationService.Navigated += (sender, args) =>
            {
                var first = Items.FirstOrDefault(x => x.Item4 == args);
                if (first == null) return;
                SelectedIndex.Value = Items.IndexOf(first);
            };

            void CreateItem<T>(string key) where T : class
            {
                var type = typeof(T);
                var command = new Command(navigationService.Navigate<T>);
                var translation = translationsService.Resolve(key);
                var icon = navigationService.Icons[type];
                var tuple = (translation, icon, command, type).ToTuple();
                Items.Add(tuple);
            }
        }

        /// <summary>
        /// Menu items implemented as tuple-based records.
        /// </summary>
        public ObservableCollection<Tuple<string, object, Command, Type>> Items { get; }

        /// <summary>
        /// Selected item index.
        /// </summary>
        public Property<int> SelectedIndex { get; }

        /// <summary>
        /// Loads application settings into view.
        /// </summary>
        public Command Load { get; }
    }
}
