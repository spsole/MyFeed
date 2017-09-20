using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class MenuViewModel
    {
        public MenuViewModel(
            IPlatformService platformService,
            INavigationService navigationService,
            ITranslationsService translationsService)
        {
            SelectedIndex = new Property<int>();
            Items = new ObservableCollection<Tuple<string, object, Command>>();
            Load = new Command(() =>
            {
                var icons = platformService.GetIconsForViews();
                var pairs = new List<(ViewKey, string)>
                {
                    (ViewKey.FeedView, "FeedViewMenuItem"),
                    (ViewKey.FaveView, "FaveViewMenuItem"),
                    (ViewKey.SourcesView, "SourcesViewMenuItem"),
                    (ViewKey.SearchView, "SearchViewMenuItem"),
                    (ViewKey.SettingsView, "SettingsViewMenuItem")
                };
                foreach (var pair in pairs)
                {
                    var command = new Command(() => navigationService.Navigate(pair.Item1));
                    var translation = translationsService.Resolve(pair.Item2);
                    Items.Add((translation, icons[pair.Item1], command).ToTuple());
                }
                navigationService.Navigated += (sender, args) =>
                {
                    if (pairs.Exists(i => i.Item1 == args))
                        SelectedIndex.Value = pairs.FindIndex(i => i.Item1 == args);
                };
            });
        }

        /// <summary>
        /// Menu items implemented as tuple-based records.
        /// </summary>
        public ObservableCollection<Tuple<string, object, Command>> Items { get; }

        /// <summary>
        /// Selected item index.
        /// </summary>
        public Property<int> SelectedIndex { get; }

        /// <summary>
        /// Loads the menu.
        /// </summary>
        public Command Load { get; }
    }
}
