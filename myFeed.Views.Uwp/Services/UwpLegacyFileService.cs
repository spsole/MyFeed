using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.Services.Models;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace myFeed.Views.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(UwpLegacyFileService))]
    public sealed class UwpLegacyFileService
    {
        private readonly ISerializationService _serializationService;
        private readonly ICategoryManager _categoryManager;
        private readonly ITranslationsService _translationsService;
        private readonly IFavoriteManager _favoriteManager;
        private readonly IDialogService _dialogService;

        public UwpLegacyFileService(
            ICategoryManager categoryManager,
            ISerializationService serializationService,
            ITranslationsService translationsService,
            IFavoriteManager favoriteManager,
            IDialogService dialogService)
        {
            _favoriteManager = favoriteManager;
            _translationsService = translationsService;
            _serializationService = serializationService;
            _categoryManager = categoryManager;
            _dialogService = dialogService;
        }

        public async Task ImportFeedsFromLegacyFormat()
        {
            try
            {
                var imported = await ProcessFeedsFromLegacyFormat();
                if (imported) await _dialogService.ShowDialog(
                    _translationsService.Resolve("SourcesMigrateSuccess"),
                    _translationsService.Resolve("SettingsNotification"));
            }
            catch (Exception exception)
            {
                var translation = _translationsService.Resolve("SourcesMigrateFailure");
                var errorMessage = string.Format(translation, exception.Message);
                await _dialogService.ShowDialog(errorMessage,
                    _translationsService.Resolve("SettingsNotification"));
            }
        }

        public async Task ImportArticlesFromLegacyFormat()
        {
            try
            {
                var imported = await ProcessArticlesFromLegacyFormat();
                if (imported) await _dialogService.ShowDialog(
                    _translationsService.Resolve("ArticlesMigrateSuccess"),
                    _translationsService.Resolve("SettingsNotification"));
            }
            catch (Exception exception)
            {
                var translation = _translationsService.Resolve("ArticlesMigrateFailure");
                var errorMessage = string.Format(translation, exception.Message);
                await _dialogService.ShowDialog(errorMessage,
                    _translationsService.Resolve("SettingsNotification"));
            }
        }

        private Task<bool> ProcessFeedsFromLegacyFormat() => Task.Run(async () =>
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var storageItem = await localFolder.TryGetItemAsync("sites");
            if (!(storageItem is IStorageFile storageFile)) return false;

            var stream = await storageFile.OpenStreamForReadAsync();
            var legacyCategories = _serializationService.Deserialize<LegacyCategories>(stream);
            if (legacyCategories?.Categories?.Any() == true)
            {
                var categories = legacyCategories.Categories.Select(category => new Category
                {
                    Title = category.Title,
                    Channels = category.Websites?.Select(source => new Channel {Notify = source.Notify, Uri = source.Uri}).ToList()
                });
                foreach (var category in categories) await _categoryManager.InsertAsync(category);
            }

            var files = new[] {"config", "datecutoff", "read.txt", "saved_cache", "sites"};
            foreach (var name in files)
            {
                var file = await localFolder.TryGetItemAsync(name);
                if (file != null) await file.DeleteAsync();
            }
            return true;
        });

        private Task<bool> ProcessArticlesFromLegacyFormat() => Task.Run(async () =>
        {
            var favoritesFolder = await ApplicationData.Current.LocalFolder.TryGetItemAsync("favorites");
            if (!(favoritesFolder is IStorageFolder storageFolder)) return false;

            var files = await storageFolder.GetFilesAsync();
            var models = new List<FeedItemModel>();
            foreach (var file in files)
            {
                var stream = await file.OpenStreamForReadAsync();
                var model = _serializationService.Deserialize<FeedItemModel>(stream);
                models.Add(model);
            }

            var articleEntities = models.Select(model => new Article
            {
                Content = model.Content, FeedTitle = model.FeedTitle, ImageUri = model.ImageUri,
                PublishedDate = DateTime.TryParse(model.PublishedDate, out var o) ? o : DateTime.Now,
                Title = model.Title, Uri = model.Uri
            });

            foreach (var article in articleEntities) await _favoriteManager.InsertAsync(article);
            await favoritesFolder.DeleteAsync();
            return true;
        });

        [XmlType("PFeedItem")]
        public sealed class FeedItemModel
        {
            [XmlElement("link")]
            public string Uri { get; set; }
            [XmlElement("title")]
            public string Title { get; set; }
            [XmlElement("content")]
            public string Content { get; set; }
            [XmlElement("image")]
            public string ImageUri { get; set; }
            [XmlElement("feed")]
            public string FeedTitle { get; set; }
            [XmlElement("PublishedDate")]
            public string PublishedDate { get; set; }
        }

        [XmlType("Categories")]
        public sealed class LegacyCategories
        {
            [XmlArray("categories")]
            public List<Category> Categories { get; set; }
            [XmlType("Category")]
            public sealed class Category
            {
                [XmlElement("title")]
                public string Title { get; set; }
                [XmlArray("websites")]
                public List<Website> Websites { get; set; }
                [XmlType("Website")]
                public sealed class Website
                {
                    [XmlElement("url")]
                    public string Uri { get; set; }
                    [XmlElement("notify")]
                    public bool Notify { get; set; }
                }
            }
        }
    }
}
