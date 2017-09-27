using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Services
{
    public class UwpLegacyFileService
    {
        private readonly ISerializationService _serializationService;
        private readonly ITranslationsService _translationsService;
        private readonly IArticlesRepository _articlesRepository;
        private readonly ISourcesRepository _sourcesRepository;
        private readonly IDialogService _dialogService;

        public UwpLegacyFileService(
            IDialogService dialogService,
            ISourcesRepository sourcesRepository,
            IArticlesRepository articlesRepository,
            ITranslationsService translationsService,
            ISerializationService serializationService)
        {
            _serializationService = serializationService;
            _translationsService = translationsService;
            _articlesRepository = articlesRepository;
            _sourcesRepository = sourcesRepository;
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

        private async Task<bool> ProcessFeedsFromLegacyFormat()
        {
            var storageItem = await ApplicationData.Current.LocalFolder.TryGetItemAsync("sites");
            if (!(storageItem is IStorageFile storageFile)) return false;

            var stream = await storageFile.OpenStreamForReadAsync();
            var legacyCategories = _serializationService.Deserialize<LegacyCategories>(stream);
            if (legacyCategories?.Categories?.Any() == true)
            {
                var categories = legacyCategories.Categories.Select(category => new SourceCategoryEntity
                {
                    Title = category.Title,
                    Sources = category.Websites?.Select(source => new SourceEntity
                    {
                        Notify = source.Notify,
                        Uri = source.Uri
                    })
                    .ToList()
                });
                await _sourcesRepository.InsertAsync(categories.ToArray());
            }

            var files = new[] { "config", "datecutoff", "read.txt", "saved_cache", "sites" };
            foreach (var name in files)
            {
                var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(name);
                if (file != null) await file.DeleteAsync();
            }
            return true;
        }

        private async Task<bool> ProcessArticlesFromLegacyFormat()
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

            var articleEntities = models.Select(model => new ArticleEntity
            {
                Content = model.Content,
                FeedTitle = model.FeedTitle,
                ImageUri = model.ImageUri,
                PublishedDate = DateTime.TryParse(model.PublishedDate, out var o) ? o : DateTime.Now,
                Title = model.Title,
                Uri = model.Uri,
                Read = false,
                Fave = true
            });

            var temporarySource = new SourceEntity();
            var temporaryCategory = new SourceCategoryEntity();
            await _sourcesRepository.InsertAsync(temporaryCategory);
            await _sourcesRepository.AddSourceAsync(temporaryCategory, temporarySource);
            await _articlesRepository.InsertAsync(temporarySource, articleEntities.ToArray());
            await _sourcesRepository.RemoveAsync(temporaryCategory);
            await favoritesFolder.DeleteAsync();
            return true;
        }

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
