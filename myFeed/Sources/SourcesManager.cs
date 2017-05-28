using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Serialization;

namespace myFeed.Sources
{
    /// <summary>
    /// Manages sources storage.
    /// </summary>
    public class SourcesManager
    {
        #region Singleton implementation

        private static SourcesManager _instance;
        private SourcesManager() { }

        public static SourcesManager GetInstance() => 
            _instance ?? (_instance = new SourcesManager());

        #endregion

        /// <summary>
        /// Adds a category to the list.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <returns>True if success.</returns>
        public async Task<bool> AddCategory(FeedCategoryModel model)
        {
            // Read categories.
            var categories = await ReadCategories();
            if (categories.Categories.Exists(i => i.Title == model.Title)) return false;

            // Add and save cats.
            categories.Categories.Add(model);
            await SaveCategories(categories);
            return true;
        }

        /// <summary>
        /// Adds new source and saves data.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public async Task<bool> AddSource(SourceItemModel model, string categoryName)
        {
            // Check if category exists.
            var categories = await ReadCategories();
            if (!categories.Categories.Exists(i => i.Title == categoryName)) return false;

            // Check if website exists.
            var category = categories.Categories.Find(i => i.Title == categoryName);
            if (category.Websites.Exists(i => i.Uri == model.Uri)) return false;

            // Add and save.
            category.Websites.Add(model);
            await SaveCategories(categories);
            return true;
        }

        /// <summary>
        /// Deletes source from defined category.
        /// </summary>
        /// <param name="model">Source item model</param>
        /// <param name="categoryName">Category name</param>
        /// <returns>True if success.</returns>
        public async Task<bool> DeleteSource(SourceItemModel model, string categoryName)
        {
            // Check if category exists.
            var categories = await ReadCategories();
            if (!categories.Categories.Exists(i => i.Title == categoryName)) return false;

            // Check if sourcefeed exists.
            var category = categories.Categories.Find(i => i.Title == categoryName);
            if (!category.Websites.Exists(i => i.Uri == model.Uri)) return false;

            // Remove by reference.
            var website = category.Websites.Find(i => i.Uri == model.Uri);
            category.Websites.Remove(website);

            // Serialize object.
            await SaveCategories(categories);
            return true;
        }

        /// <summary>
        /// Toggles notifications for particular feed in particular category.
        /// </summary>
        /// <param name="value">New value to write.</param>
        /// <param name="categoryName">Category name.</param>
        /// <param name="feedUri">Feed url</param>
        /// <returns>True if success.</returns>
        public async Task<bool> ToggleNotifications(bool value, string categoryName, string feedUri)
        {
            // Check for category existance.
            var categories = await ReadCategories();
            if (!categories.Categories.Exists(i => i.Title == categoryName)) return false;

            // Check for website existance.
            var category = categories.Categories.Find(i => i.Title == categoryName);
            if (!category.Websites.Exists(i => i.Uri == feedUri)) return false;

            // Get reference to website.
            var website = category.Websites.Find(i => i.Uri == feedUri);
            website.Notify = value;

            // Serialize object.
            await SaveCategories(categories);
            return true;
        }

        /// <summary>
        /// Does stuff for category renaming.
        /// </summary>
        /// <param name="category">Category to rename</param>
        /// <param name="newName">New name</param>
        /// <returns>True if success.</returns>
        public async Task<bool> RenameCategory(FeedCategoryModel category, string newName)
        {
            // Skip if this name already exists.
            var categories = await ReadCategories();
            if (categories.Categories.Exists(i => i.Title == newName)) return false;

            // Update cats.
            categories
                .Categories
                .Find(i => i.Title == category.Title)
                .Title = newName;

            // Serialize cats.
            await SaveCategories(categories);
            return true;
        }

        /// <summary>
        /// Deletes the category.
        /// </summary>
        /// <param name="category">Model to delete</param>
        /// <returns>True if success.</returns>
        public async Task<bool> DeleteCategory(FeedCategoryModel category)
        {
            var categories = await ReadCategories();
            if (!categories.Categories.Exists(i => i.Title == category.Title)) return false;

            // Find and remove.
            var reference = 
                categories
                    .Categories
                    .Find(i => i.Title == category.Title);
            categories.Categories.Remove(reference);

            // Serialize cats.
            await SaveCategories(categories);
            return true;
        }

        /// <summary>
        /// Reads sites file from hard disk.
        /// </summary>
        /// <returns>Deserialized categories model.</returns>
        public Task<FeedCategoriesModel> ReadCategories() => Task.Run(async () =>
        {
            // Deserialize and return categories.
            var sourcesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "sites", CreationCollisionOption.OpenIfExists);
            var categories = await GenericXmlSerializer.DeSerializeObject<FeedCategoriesModel>(sourcesFile);
            if (categories != null) return categories;

            // Serialize empty structure if file was empty.
            categories = new FeedCategoriesModel { Categories = new List<FeedCategoryModel>() };
            GenericXmlSerializer.SerializeObject(categories, sourcesFile);
            return categories;
        });

        /// <summary>
        /// Saves given FeedCategoriesModel.
        /// </summary>
        /// <param name="categories">Categories model</param>
        public async Task SaveCategories(FeedCategoriesModel categories)
        {
            var sourcesFile = await ApplicationData.Current.LocalFolder.GetFileAsync("sites");
            GenericXmlSerializer.SerializeObject(categories, sourcesFile);
            SourcesChanged = true;
        }

        /// <summary>
        /// Indicates if sources-related data has changed.
        /// </summary>
        public bool SourcesChanged { get; set; }
    }
}
