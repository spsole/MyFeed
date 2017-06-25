using System.Collections.Generic;
using System.Linq;
using myFeed.Extensions.Mvvm.Implementation;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Models.Feedly;
using myFeed.Sources;

namespace myFeed.Search.Controls
{
    /// <summary>
    /// Represents search add dialog ViewModel.
    /// </summary>
    public class SearchAddDialogViewModel
    {
        private readonly SearchItemModel _model;
        private readonly SourcesManager _manager;
        public SearchAddDialogViewModel(SearchItemModel model) => 
            (_model, _manager) = (model, SourcesManager.GetInstance());

        #region Properties

        /// <summary>
        /// View model for categories selection.
        /// </summary>
        public ComboBoxViewModel<string, string> CategoriesViewModel { get; } = 
            new ComboBoxViewModel<string, string>();

        #endregion

        #region Methods

        /// <summary>
        /// Loads categories file.
        /// </summary>
        public async void LoadCategoriesAsync()
        {
            // Deserialize categories.
            var categories = await _manager.ReadCategories();
            if (categories.Categories.Count == 0)
            {
                categories.Categories.Add(new FeedCategoryModel
                {
                    Title = "Example category",
                    Websites = new List<SourceItemModel>()
                });
                await _manager.SaveCategories(categories);
            }

            // Add all categories as choices for selection.
            categories.Categories.ForEach(i => 
                CategoriesViewModel.Items.Add(
                        new KeyValuePair<string, string>(i.Title, i.Title)));

            // Set selected item.
            CategoriesViewModel.SelectedItem = CategoriesViewModel.Items.First();
        }

        /// <summary>
        /// Saves new source.
        /// </summary>
        public async void AddModel()
        {
            // Create source model.
            var categoryName = ((KeyValuePair<string, string>)CategoriesViewModel.SelectedItem).Value;
            var sourceModel = new SourceItemModel { Uri = _model.FeedId.Substring(5), Notify = true };
            await _manager.AddSource(sourceModel, categoryName);
        }


        #endregion
    }
}
