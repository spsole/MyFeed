using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    public sealed class OpmlService : IOpmlService
    {
        private readonly ISerializationService _serializationService;
        private readonly ICategoriesRepository _categoriesRepository;

        public OpmlService(
            ICategoriesRepository categoriesRepository,
            ISerializationService serializationService)
        {
            _categoriesRepository = categoriesRepository;
            _serializationService = serializationService;
        }

        public async Task<bool> ExportOpmlFeedsAsync(Stream stream)
        {
            // Initialize new Opml instance and read categories from db.
            var opml = new Opml {Head = new OpmlHead {Title = "Feeds from myFeed App"}};
            var categories = await _categoriesRepository.GetAllAsync();
            var outlines = categories.Select(x => new OpmlOutline
            {
                ChildOutlines = x.Channels
                    .Select(i => new {Entity = i, Uri = new Uri(i.Uri)})
                    .Select(y => new OpmlOutline
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
            opml.Body = new List<OpmlOutline>(outlines);
            _serializationService.Serialize(opml, stream);
            return true;
        }

        public async Task<bool> ImportOpmlFeedsAsync(Stream stream)
        {
            // Deserialize object from file.
            var opml = _serializationService.Deserialize<Opml>(stream);
            if (opml == null) return false;

            // Process potential categories.
            var categories = new List<Category>();
            opml.Body
                .Where(i => i.XmlUrl == null && i.HtmlUrl == null)
                .Select(i => new {Title = i.Title ?? i.Text, Outline = i})
                .Where(i => i.Title != null)
                .Select(i => new Category
                {
                    Channels = i.Outline.ChildOutlines
                        .Select(o => new Channel
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
            var uncategorized = new Category
            {
                Title = "Unknown category",
                Channels = opml.Body
                    .Where(i => Uri.IsWellFormedUriString(i.XmlUrl, UriKind.Absolute))
                    .Select(i => new Channel {Uri = i.XmlUrl, Notify = true})
                    .ToList()
            };
            if (uncategorized.Channels.Any()) categories.Add(uncategorized);

            // Insert into database and notify user.
            foreach (var category in categories) 
                await _categoriesRepository.InsertAsync(category);
            return true;
        }
    }
}