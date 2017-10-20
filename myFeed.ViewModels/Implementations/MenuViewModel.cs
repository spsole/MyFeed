using System;
using System.Collections.ObjectModel;
using System.Linq;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class MenuViewModel
    {
        public ObservableCollection<Tuple<string, object, ObservableCommand, Type>> Items { get; }

        public ObservableProperty<int> SelectedIndex { get; }

        public ObservableCommand Load { get; }

        public MenuViewModel(
            ITranslationsService translationsService,
            INavigationService navigationService,
            IPlatformService platformService,
            ISettingsService settingsService)
        {
            SelectedIndex = 0;
            Items = new ObservableCollection<Tuple<string, object, ObservableCommand, Type>>();
            Load = new ObservableCommand(async () =>
            {
                await navigationService.Navigate<FeedViewModel>();
                var theme = await settingsService.GetAsync<string>("Theme");
                await platformService.RegisterTheme(theme);
                var freq = await settingsService.GetAsync<int>("NotifyPeriod");
                await platformService.RegisterBackgroundTask(freq);
                await settingsService.SetAsync("LastFetched", DateTime.Now);
            });
            
            CreateItem<FeedViewModel>("FeedViewMenuItem");
            CreateItem<FaveViewModel>("FaveViewMenuItem");
            CreateItem<ChannelsViewModel>("SourcesViewMenuItem");
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
    }
}
