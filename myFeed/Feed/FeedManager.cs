using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Syndication;
using myFeed.Extensions;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Serialization;

namespace myFeed.Feed
{
    /// <summary>
    /// Contains Feed-based operations.
    /// </summary>
    public class FeedManager
    {
        #region Singleton implementation

        private static FeedManager _instance;
        private FeedManager() { }

        public static FeedManager GetInstance() => 
            _instance ?? (_instance = new FeedManager());

        #endregion

        /// <summary>
        /// Reads categories.
        /// </summary>
        public Task<FeedCategoriesModel> ReadCategories() => 
            Sources.SourcesManager.GetInstance().ReadCategories();

        /// <summary>
        /// Retrieves feed on ThreadPool.
        /// </summary>
        /// <param name="category">Category to work with</param>
        /// <returns>Task of type IOrderedEnumerable</returns>
        public async Task<IOrderedEnumerable<FeedItemViewModel>> RetrieveFeedAsync(
            FeedCategoryModel category) => await Task.Run(() => RetrieveFeed(category));

        /// <summary>
        /// Marks article as read by passing model.
        /// </summary>
        /// <param name="model">FeedItemModel</param>
        /// <returns>True if success</returns>
        public async Task<bool> MarkArticleAsRead(FeedItemModel model)
        {
            var id = model.GetTileId();
            var readFile = await ApplicationData.Current.LocalFolder.GetFileAsync("read.txt");
            var contents = await FileIO.ReadTextAsync(readFile);

            // If does'nt contain, append string.
            if (contents.Contains(id)) return true;
            await FileIO.AppendTextAsync(readFile, $"{id};");
            return true;
        }

        /// <summary>
        /// Marks article as unread by passing model.
        /// </summary>
        /// <param name="model">FeedItemModel</param>
        /// <returns>True if success</returns>
        public async Task<bool> MarkArticleAsUnread(FeedItemModel model)
        {
            var id = model.GetTileId();
            var readFile = await ApplicationData.Current.LocalFolder.GetFileAsync("read.txt");
            var contents = await FileIO.ReadTextAsync(readFile);

            // If contains, remove and update UI.
            if (!contents.Contains(id)) return true;
            await FileIO.WriteTextAsync(readFile, contents.Replace($"{id};", string.Empty));
            return true;
        }

        /// <summary>
        /// Adds article to favorites.
        /// </summary>
        /// <param name="model">Model to add</param>
        /// <returns>True if success</returns>
        public async Task<bool> AddArticleToFavorites(FeedItemModel model)
        {
            var cacheFile = await ApplicationData.Current.LocalFolder.GetFileAsync("saved_cache");
            var cacheString = await FileIO.ReadTextAsync(cacheFile);
            if (cacheString.Contains(model.Uri)) return true;

            var favoritesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("favorites");
            var filecount = (await favoritesFolder.GetFilesAsync()).Count;

            GenericXmlSerializer.SerializeObject(model,
                await favoritesFolder.CreateFileAsync(filecount.ToString()));
            await FileIO.AppendTextAsync(cacheFile, model.Uri + ";");
            return true;
        }

        #region Helper methods

        /// <summary>
        /// Retrieves feed synchroniously.
        /// </summary>
        /// <param name="category">Category to work with</param>
        /// <returns>IOrderedEnumerable</returns>
        private static async Task<IOrderedEnumerable<FeedItemViewModel>> RetrieveFeed(FeedCategoryModel category)
        {
            // Get read articles info.
            var folder = ApplicationData.Current.LocalFolder;
            var readFile = await folder.GetFileAsync("read.txt");
            var faveFile = await folder.GetFileAsync("saved_cache");

            // Read files.
            var faveContents = await FileIO.ReadTextAsync(faveFile);
            var readContents = await FileIO.ReadTextAsync(readFile);

            // This list will contain all items retrieved and managed.
            var unorderedViewModels = new List<FeedItemViewModel>();
            Parallel.ForEach(category.Websites, website =>
            {
                // Retrieve feed async.
                var syndicationFeed = Tools.TryAsync(
                    SyndicationConverter.GetInstance().RetrieveFeedAsync(website.Uri),
                    new SyndicationFeed()
                ).Result;

                // Add items if they exist.
                unorderedViewModels.AddRange(
                    from item in syndicationFeed.Items
                    select FeedItemModel.FromSyndicationItem(item, syndicationFeed.Title.Text)
                    into model
                    let isRead = readContents.Contains(model.GetTileId())
                    let isFave = faveContents.Contains(model.Uri)
                    select new FeedItemViewModel(model, isRead, isFave)
                );
            });
            return unorderedViewModels.Where(i => i != null).OrderByDescending(i => i.PublishedDate);
        }

        #endregion
    }
}

