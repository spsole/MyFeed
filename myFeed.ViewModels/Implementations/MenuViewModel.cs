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
            Items = new ObservableCollection<Tuple<string, object, ObservableCommand, Type>>();
            SelectedIndex = new ObservableProperty<int>();
            Load = new ObservableCommand(async () =>
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
                var command = new ObservableCommand(navigationService.Navigate<T>);
                var translation = translationsService.Resolve(key);
                var icon = navigationService.Icons[type];
                var tuple = (translation, icon, command, type).ToTuple();
                Items.Add(tuple);
            }
        }

        /// <summary>
        /// Menu items implemented as tuple-based records.
        /// </summary>
        public ObservableCollection<Tuple<string, object, ObservableCommand, Type>> Items { get; }

        /// <summary>
        /// Selected item index.
        /// </summary>
        public ObservableProperty<int> SelectedIndex { get; }

        /// <summary>
        /// Loads application settings into view.
        /// </summary>
        public ObservableCommand Load { get; }
    }
}
