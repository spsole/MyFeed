using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml.Controls;
using myFeed.Extensions;
using myFeed.Extensions.ViewModels;
using myFeed.FeedModels.Models;
using myFeed.Sources.Controls;

namespace myFeed.Sources
{
    /// <summary>
    /// Sources page view model.
    /// </summary>
    public class SourcesPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of sources page view model.
        /// </summary>
        public SourcesPageViewModel() => LoadCollectionAsync();

        #region Properties

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public ObservableProperty<bool> IsLoadingProperty { get; } = 
            new ObservableProperty<bool>(true);

        /// <summary>
        /// A collection of inner models.
        /// </summary>
        public ObservableCollection<SourceCategoryViewModel> Items { get; } = 
            new DeeplyObservableCollection<SourceCategoryViewModel>();

        /// <summary>
        /// Indicates if welcome screen is visible.
        /// </summary>
        public ObservableProperty<bool> IsWelcomeVisibleProperty { get; } =
            new ObservableProperty<bool>(false);

        /// <summary>
        /// Indicates if selection mode is toggled.
        /// </summary>
        public ObservableProperty<bool> IsRearrangeEnabledProperty { get; } =
            new ObservableProperty<bool>(false);

        #endregion

        #region Methods

        /// <summary>
        /// Toggles rearrange mode.
        /// </summary>
        public void ToggleRearrange()
        {
            IsRearrangeEnabledProperty.Value = !IsRearrangeEnabledProperty.Value;
            if (IsRearrangeEnabledProperty.Value)
                Items.ToList().ForEach(i => i.IsContentPresent.Value = false);
        }

        /// <summary>
        /// Loads sources info from files.
        /// </summary>
        public async void LoadCollectionAsync()
        {
            // Read and add categories.
            var categories = await SourcesManager.GetInstance().ReadCategories();
            categories.Categories
                .Select(i => new SourceCategoryViewModel(i, this))
                .ToList()
                .ForEach(Items.Add);

            // Turn load ring off.
            IsLoadingProperty.Value = false;
            IsWelcomeVisibleProperty.Value = Items.Count == 0;
            Items.CollectionChanged += (s, a) =>
                IsWelcomeVisibleProperty.Value = 
                    Items.Count == 0;

            // Save new seq.
            Items.CollectionChanged += async (s, a) =>
            {
                if (a.Action != NotifyCollectionChangedAction.Add) return;
                var newModel = new FeedCategoriesModel { Categories = Items.Select(i => i.GetCategory()).ToList() };
                await SourcesManager.GetInstance().SaveCategories(newModel);
            };
        }

        /// <summary>
        /// Brings user to search page.
        /// </summary>
        public void NavigateToSearch() => Navigation
            .NavigationPage
            .NavigationFrame
            .Navigate(typeof(Search.SearchPage));

        /// <summary>
        /// Adds new category to list.
        /// </summary>
        public async void AddCategory()
        {
            // Show rename dialog and get results.
            var addDialog = new RenameDialog();
            if (await addDialog.ShowAsync() != ContentDialogResult.Primary) return;
            var newName = addDialog.GetNewCategoryTitle();
            if (string.IsNullOrWhiteSpace(newName)) return;

            // Add category.
            var category = new FeedCategoryModel { Title = newName, Websites = new List<SourceItemModel>() };
            await SourcesManager.GetInstance().AddCategory(category);

            // Update UI.
            Items.Add(new SourceCategoryViewModel(category, this));
        }

        #endregion
    }
}
