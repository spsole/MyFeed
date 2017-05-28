using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Extensions.ViewModels;

namespace myFeed.Feed
{
    /// <summary>
    /// Behavior for FeedCategoriesPage.
    /// </summary>
    public class FeedCategoriesViewModel
    {
        /// <summary>
        /// Initializes a new instance of FeedCategoriesViewModel.
        /// </summary>
        public FeedCategoriesViewModel() => LoadAsync();

        #region Properties

        /// <summary>
        /// Collection of categories pivots.
        /// </summary>
        public ObservableCollection<PivotItem> Items { get; } = 
            new ObservableCollection<PivotItem>();

        /// <summary>
        /// Selected item in Pivot items.
        /// </summary>
        public ObservableProperty<object> SelectedItem { get; } = 
            new ObservableProperty<object>(null);

        /// <summary>
        /// Indicates if received category hass no items inside.
        /// </summary>
        public ObservableProperty<bool> IsCategoryEmpty { get; } =
            new ObservableProperty<bool>(false);

        #endregion

        #region Methods

        /// <summary>
        /// Reads data from disk and builds collections UI.
        /// </summary>
        public async void LoadAsync()
        {
            var categories = await FeedManager.GetInstance().ReadCategories();
            IsCategoryEmpty.Value = categories.Categories.Count == 0;
            Items.Clear();
            categories.Categories
                .ToList()
                .ForEach(i => Items.Add(
                    new PivotItem
                    {
                        Header = i.Title,
                        Content = new Frame(),
                        Margin = new Thickness(0),
                        DataContext = i
                    })
                );

            // Load selected category.
            if (Items.Count == 0) return;
            SelectedItem.Value = Items.First();
            LoadCategory();
        }

        /// <summary>
        /// Loads category.
        /// </summary>
        public void LoadCategory()
        {
            // Get inner frame.
            if (SelectedItem.Value == null) return;
            var value = (PivotItem) SelectedItem.Value;
            var frame = (Frame) value.Content;

            // Navigate only when nothing is present.
            if (frame?.CurrentSourcePageType == null)
                frame?.Navigate(typeof(FeedPage), value.DataContext);
        }

        /// <summary>
        /// Navigates user to sources page if he is new to the app.
        /// </summary>
        public void NavigateToSources()
        {
            // Navigate using inner nav frame.
            Navigation.NavigationPage
                .NavigationFrame
                .Navigate(typeof(Sources.SourcesPage));
        }

        #endregion
    }
}
