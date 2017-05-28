using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.StartScreen;
using myFeed.Feed;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Serialization;

namespace myFeed.Fave
{
    /// <summary>
    /// Contains methods for favorite articles storage management.
    /// </summary>
    public class FaveManager
    {
        #region Singleton implementation

        private static FaveManager _instance;
        private FaveManager() { }

        public static FaveManager GetInstance() =>
            _instance ?? (_instance = new FaveManager());

        #endregion

        /// <summary>
        /// Deletes an article from disk.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="index">Article id.</param>
        /// <returns>True if success.</returns>
        public async Task<bool> DeleteArticle(FeedItemViewModel model, int index)
        {
            // Remove model data from cache.
            var cacheFile = await ApplicationData.Current.LocalFolder.GetFileAsync("saved_cache");
            var cache = await FileIO.ReadTextAsync(cacheFile);
            await FileIO.WriteTextAsync(cacheFile, cache.Replace($"{model.GetModel().Uri};", string.Empty));

            // Manage favorites deletion.
            var faveFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("favorites");
            var files = await faveFolder.GetFilesAsync();
            var fileCount = files.Count;

            // Delete item from collection.
            await files[index].DeleteAsync();

            // Iterate and rename.
            if (fileCount > 1 && (fileCount - 1) != index)
                for (var x = index + 1; x < fileCount; x++)
                    await files[x].RenameAsync((x - 1).ToString());

            // Remove all related tiles.
            var tilesLeft = await SecondaryTile.FindAllAsync();
            var tile = tilesLeft.FirstOrDefault(t => t.TileId == model.GetModel().GetTileId());
            if (tile != null) await tile.RequestDeleteAsync();
            return true;
        }

        /// <summary>
        /// Deserializes and returns all articles from disk.
        /// </summary>
        /// <returns>Article bundle.</returns>
        public async Task<IEnumerable<FaveItemViewModel>> LoadArticles()
        {
            var favoritesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("favorites");
            var files = await favoritesFolder.GetFilesAsync();
            var models = new List<FaveItemViewModel>();
            foreach (var file in files)
            {
                var newsItemModel = await GenericXmlSerializer.DeSerializeObject<FeedItemModel>(file);
                var viewModel = new FaveItemViewModel(newsItemModel, false, true);
                models.Add(viewModel);
            }
            return models;
        }
    }
}
