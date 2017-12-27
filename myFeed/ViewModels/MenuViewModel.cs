using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using DryIocAttributes;
using myFeed.Common;
using myFeed.Interfaces;
using myFeed.Platform;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(MenuViewModel))]
    public sealed class MenuViewModel
    {
        public ObservableCollection<Tuple<string, object, ObservableCommand, Type>> Items { get; }

        public ObservableProperty<int> SelectedIndex { get; }

        public ObservableCommand Load { get; }

        public MenuViewModel(
            ITranslationService translationsService,
            INavigationService navigationService,
            IPlatformService platformService,
            ISettingManager settingManager)
        {
            SelectedIndex = 0;
            Items = new ObservableCollection<Tuple<string, object, ObservableCommand, Type>>();
            Load = new ObservableCommand(async () =>
            {
                await navigationService.Navigate<FeedViewModel>();
                var theme = await settingManager.GetAsync<string>("Theme");
                await platformService.RegisterTheme(theme);
                var freq = await settingManager.GetAsync<int>("NotifyPeriod");
                await platformService.RegisterBackgroundTask(freq);
                await settingManager.SetAsync("LastFetched", DateTime.Now);
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
