using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Repositories.Entities.Opml;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public class OpmlService : IOpmlService
    {
        private readonly ISerializationService _serializationService;
        private readonly ITranslationsService _translationsService;
        private readonly ISourcesRepository _sourcesRepository;
        private readonly IPlatformService _platformProvider;

        public OpmlService(
            IPlatformService platformService,
            ITranslationsService translationsService,
            ISerializationService serializationService,
            ISourcesRepository sourcesRepository)
        {
            _platformProvider = platformService;
            _sourcesRepository = sourcesRepository;
            _translationsService = translationsService;
            _serializationService = serializationService;
        }

        public async Task ExportOpmlFeeds()
        {
            // Init new Opml instance and read categories from db.
            var opml = new Opml {Head = new Head {Title = "Feeds from myFeed App"}};
            var categories = await _sourcesRepository.GetAllAsync();
            var outlines = categories.Select(x => new Outline
            {
                ChildOutlines = x.Sources
                    .Select(i => new {Entity = i, Uri = new Uri(i.Uri)})
                    .Select(y => new Outline
                    {
                        HtmlUrl = $"{y.Uri.Scheme}://{y.Uri.Host}",
                        XmlUrl = y.Uri.ToString(),
                        Title = y.Uri.Host,
                        Text = y.Uri.Host,
                        Version = "rss",
                        Type = "rss"
                    })
                    .ToList(),
                Title = x.Title,
                Text = x.Title
            });

            // Fill opml with categories.
            opml.Body = new List<Outline>(outlines);
            var stream = await _platformProvider.PickFileForWriteAsync();
            if (stream == null) return;

            // Serialize data into file picked by user.
            _serializationService.Serialize(opml, stream);
            await _platformProvider.ShowDialog(
                _translationsService.Resolve("ExportFeedsSuccess"),
                _translationsService.Resolve("SettingsNotification"));
        }

        public async Task ImportOpmlFeeds()
        {
            // Deserialize opml.
            var stream = await _platformProvider.PickFileForReadAsync();
            var opml = _serializationService.Deserialize<Opml>(stream);
            if (opml == null) return;

            // Process potential categories.
            var categories = new List<SourceCategoryEntity>();
            opml.Body
                .Where(i => i.XmlUrl == null && i.HtmlUrl == null)
                .Select(i => new {Title = i.Title ?? i.Text, Outline = i})
                .Where(i => i.Title != null)
                .Select(i => new SourceCategoryEntity
                {
                    Sources = i.Outline.ChildOutlines
                        .Select(o => new SourceEntity
                        {
                            Uri = o.XmlUrl,
                            Notify = true
                        })
                        .ToList(),
                    Title = i.Title
                })
                .ToList()
                .ForEach(i => categories.Add(i));

            // Process plain feeds.
            var uncategorized = new SourceCategoryEntity
            {
                Title = "Unknown category",
                Sources = opml.Body
                    .Where(i => Uri.IsWellFormedUriString(i.XmlUrl, UriKind.Absolute))
                    .Select(i => new SourceEntity {Uri = i.XmlUrl, Notify = true})
                    .ToList()
            };
            if (uncategorized.Sources.Any()) categories.Add(uncategorized);

            // Insert into database and notify user.
            foreach (var category in categories)
                await _sourcesRepository.InsertAsync(category);
            await _platformProvider.ShowDialog(
                _translationsService.Resolve("ImportFeedsSuccess"),
                _translationsService.Resolve("SettingsNotification"));
        }
    }
}