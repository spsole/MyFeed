using System.Linq;
using Windows.UI.Xaml.Controls;
using myFeed.Extensions.Mvvm;
using myFeed.Extensions.Mvvm.Implementation;

namespace myFeed.Navigation
{
    /// <summary>
    /// Represents navigation page view model.
    /// </summary>
    public class MenuViewModel : DeeplyObservableCollection<MenuItemViewModel>
    {
        /// <summary>
        /// Instantiates new MenuViewModel.
        /// </summary>
        public MenuViewModel()
        {
            new[]
            {
                (Symbol.PostUpdate, "FeedL", typeof(Feed.FeedCategoriesPage)),
                (Symbol.OutlineStar, "FavoritesL", typeof(Fave.FavePage)),
                (Symbol.List, "FeedlistL", typeof(Sources.SourcesPage)),
                (Symbol.Zoom, "FeedSearchL", typeof(Search.SearchPage)),
                (Symbol.Setting, "SettingsL", typeof(Settings.SettingsPage))
            }
            .Select(i => new MenuItemViewModel(i.Item1, i.Item2, i.Item3))
            .ToList()
            .ForEach(Add);

            // Register back button press event handler.
            NavigationManager.GetInstance().AddBackHandler(args =>
            {
                if (!IsMenuOpened.Value) return;
                IsMenuOpened.Value = false;
                args.Handled = true;
            }, 
            EventPriority.Highest);
        }

        /// <summary>
        /// Stores selected item.
        /// </summary>
        public IObservableProperty<object> SelectedItem { get; } =
            new ObservableProperty<object>(default(object));

        /// <summary>
        /// Indicates menu state.
        /// </summary>
        public IObservableProperty<bool> IsMenuOpened { get; } =
            new ObservableProperty<bool>(false);

        /// <summary>
        /// Navigates to initial page.
        /// </summary>
        public void LoadInitialPage() => SelectedItem.Value = this.First();

        /// <summary>
        /// Navigates user to selected page.
        /// </summary>
        public void NavigateToPage()
        {
            var frame = NavigationPage.NavigationFrame;
            var pageType = ((MenuItemViewModel) SelectedItem.Value).PageType;
            if (frame.CurrentSourcePageType == pageType) return;

            frame.Navigate(pageType);
            IsMenuOpened.Value = false;
        }

        /// <summary>
        /// Handles navigation.
        /// </summary>
        public void HandleNavigation() => 
            SelectedItem.Value = this.FirstOrDefault(
                i => i.PageType == NavigationPage.NavigationFrame?.SourcePageType);

        /// <summary>
        /// Toggles menu state.
        /// </summary>
        public void ToggleMenu() => IsMenuOpened.Value = !IsMenuOpened.Value;
    }
}