using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;
using myFeed.Extensions;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Models.Opml;
using myFeed.FeedModels.Serialization;
using myFeed.Sources;

namespace myFeed.Settings
{
    /// <summary>
    /// OPML manager. Contains methods 
    /// to work with OPML format.
    /// </summary>
    public class OpmlManager
    {
        private readonly string[] _supportedExtensions;

        #region Lazy initialization

        private static OpmlManager _instance;
        private OpmlManager() => _supportedExtensions = new[] { ".opml", ".xml" };

        /// <summary>
        /// Returns an instance of OPML manager.
        /// </summary>
        /// <returns>Instance</returns>
        public static OpmlManager GetInstanse() => _instance ?? (_instance = new OpmlManager());

        #endregion

        #region OPML helpers

        /// <summary>
        /// Imports feeds from OPML format.
        /// </summary>
        public async void ImportFeedsFromOpml()
        {
            // Let user choose a place for OPML format storing.
            var openPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.Desktop };
            _supportedExtensions.ToList().ForEach(openPicker.FileTypeFilter.Add);

            // Request open async.
            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            // Deserialize.
            var opml = await GenericXmlSerializer.DeSerializeObject<Opml>(file);
            if (opml == null) return;

            // Temp var.
            var omplCategories = new List<FeedCategoryModel>();

            // Process potential categories.
            opml.Body
                .Where(i => (i.HtmlUrl == null || i.XmlUrl == null) && (i.Text != null || i.Title != null))
                .Select(i => new FeedCategoryModel(i.Title ?? i.Text, new List<SourceItemModel>(
                    i.ChildOutlines.Select(s => new SourceItemModel(s.XmlUrl, true)))))
                .ToList().ForEach(omplCategories.Add);

            // Process plain feeds.
            var uncategorized = new FeedCategoryModel("No category", opml.Body
                .Where(i => i.XmlUrl != null && i.HtmlUrl != null)
                .Select(i => new SourceItemModel(i.XmlUrl, true))
                .ToList());
            if (uncategorized.Websites.Count > 0)
                omplCategories.Add(uncategorized);

            // Serialize new object.
            var categories = await SourcesManager.GetInstance().ReadCategories();
            foreach (var feedCategoryModel in omplCategories)
            {
                // Add new category if its name is unique.
                var categoryTitle = feedCategoryModel.Title;
                var category = categories.Categories.FirstOrDefault(i => i.Title == categoryTitle);
                if (category == null)
                {
                    categories.Categories.Add(feedCategoryModel);
                    continue;
                }

                // Add appropriate websites if category already exists.
                feedCategoryModel.Websites.ForEach(category.Websites.Add);
                category.Websites = category.Websites.DistinctBy(i => i.Uri).ToList();
            }
            await SourcesManager.GetInstance().SaveCategories(categories);
            var resourceLoader = new ResourceLoader();
            Tools.ShowMessage(resourceLoader.GetString("ImportFeedsSuccess"), 
                resourceLoader.GetString("SettingsNotification"));
        }

        /// <summary>
        /// Exports settings to popular OPML format.
        /// </summary>
        public async void ExportFeedsToOpml()
        {
            // Create OPML.
            const string rss = "rss";
            var categories = await SourcesManager.GetInstance().ReadCategories();
            var opml = new Opml
            {
                Head = new Head { Title = "Feed list from myFeed for Windows 10" },
                Body = new List<Outline>()
            };

            // Select and add 
            // range for OPML.
            categories
                .Categories
                .Select(category => new Outline(
                    category.Title,
                    text: category.Title,
                    childOutlines: category.Websites.Select(i =>
                        {
                            var u = new Uri(i.Uri);
                            var hCntnt = $"{u.Scheme}://{u.Host}";
                            var title = u.Host.Capitalize();
                            return new Outline(title, rss, hCntnt, i.Uri, rss, title);
                        })
                        .ToList()
                ))
                .ToList()
                .ForEach(opml.Body.Add);

            // Let user choose a place for OPML format storing.
            var savePicker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.Desktop };
            savePicker.FileTypeChoices.Add("OPML", _supportedExtensions);
            savePicker.SuggestedFileName = "MyFeeds";

            // Request save async.
            var file = await savePicker.PickSaveFileAsync();
            if (file == null) return;

            // Serialize if not null.
            GenericXmlSerializer.SerializeObject(opml, file);
            var resourceLoader = new ResourceLoader();
            Tools.ShowMessage(resourceLoader.GetString("ExportFeedsSuccess"),
                resourceLoader.GetString("SettingsNotification"));
        }

        #endregion
    }
}
