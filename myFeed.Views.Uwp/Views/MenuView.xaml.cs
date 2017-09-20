using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using myFeed.Services.Abstractions;
using myFeed.Views.Uwp.Services;

namespace myFeed.Views.Uwp.Views
{
    public sealed partial class MenuView : Page
    {
        public MenuView()
        {
            var loader = new ResourceLoader();
            DataContext = new[]
            {
                (Symbol.PostUpdate, loader.GetString("FeedViewMenuItem"), ViewKey.FeedView),
                (Symbol.OutlineStar, loader.GetString("FaveViewMenuItem"), ViewKey.FaveView),
                (Symbol.List, loader.GetString("SourcesViewMenuItem"), ViewKey.SourcesView),
                (Symbol.Zoom, loader.GetString("SearchViewMenuItem"), ViewKey.SearchView),
                (Symbol.Setting, loader.GetString("SettingsViewMenuItem"), ViewKey.SettingsView),
            }
            .Select(i => i.ToTuple());
            InitializeComponent();
        }

        private async void OnNavigated(object sender, NavigationEventArgs e)
        {
            var items = ((IEnumerable<Tuple<Symbol, string, ViewKey>>) DataContext).ToList();
            var item = items.First(i => UwpNavigationService.Pages[i.Item3] == NavigationFrame.SourcePageType);
            var index = items.IndexOf(item);
            try
            {
                MenuListView.SelectedIndex = index;
            }
            catch (ArgumentException)
            {
                await Task.Delay(500);
                MenuListView.SelectedIndex = index;
            }
        }
    }
}