using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace myFeed.Views.Uwp.Views
{
    public sealed partial class MenuView : Page
    {
        public MenuView()
        {
            var loader = new ResourceLoader();
            DataContext = new[]
            {
                (Symbol.PostUpdate, loader.GetString("FeedViewMenuItem"), typeof(FeedView)),
                (Symbol.OutlineStar, loader.GetString("FaveViewMenuItem"), typeof(FaveView)),
                (Symbol.List, loader.GetString("SourcesViewMenuItem"), typeof(SourcesView)),
                (Symbol.Zoom, loader.GetString("SearchViewMenuItem"), typeof(SearchView)),
                (Symbol.Setting, loader.GetString("SettingsViewMenuItem"), typeof(SettingsView)),
            }
            .Select(i => i.ToTuple());
            InitializeComponent();
        }

        private async void OnNavigated(object sender, NavigationEventArgs e)
        {
            var items = ((IEnumerable<Tuple<Symbol, string, Type>>) DataContext).ToList();
            var item = items.First(i => i.Item3 == NavigationFrame.SourcePageType);
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