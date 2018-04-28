using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IOpmlService))]
    public sealed class DefaultOpmlService : IOpmlService
    {
        private readonly ISerializationService _serializationService;
        private readonly ICategoryManager _categoryManager;

        public DefaultOpmlService(
            ISerializationService serializationService,
            ICategoryManager categoryManager)
        {
            _serializationService = serializationService;
            _categoryManager = categoryManager;
        }

        public async Task<bool> ExportOpml(Stream stream)
        {
            var opml = new Opml {Head = new OpmlHead {Title = "Feeds from myFeed App"}};
            var categories = await _categoryManager.GetAll().ConfigureAwait(false);
            var outlines = categories.Select(x => new OpmlOutline
            {
                Text = x.Title,
                Title = x.Title,
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
                .ToList()
            });

            opml.Body = new List<OpmlOutline>(outlines);
            if (stream == null) return false;
            await _serializationService.Serialize(opml, stream).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> ImportOpml(Stream stream)
        {
            var opml = await _serializationService.Deserialize<Opml>(stream).ConfigureAwait(false);
            if (opml == null) return false;
            var categories = opml.Body
                .Where(i => i.XmlUrl == null && i.HtmlUrl == null)
                .Select(i => new {Title = i.Title ?? i.Text, Outline = i})
                .Where(i => i.Title != null)
                .Select(i => new Category
                {
                    Title = i.Title,
                    Channels = i.Outline.ChildOutlines
                    .Select(o => new Channel { Uri = o.XmlUrl, Notify = true })
                    .ToList()
                })
                .ToList();

            var uncategorized = new Category
            {
                Title = "Unknown category",
                Channels = opml.Body
                .Where(i => Uri.IsWellFormedUriString(i.XmlUrl, UriKind.Absolute))
                .Select(i => new Channel {Uri = i.XmlUrl, Notify = true})
                .ToList()
            };
            if (uncategorized.Channels.Any()) categories.Add(uncategorized);
            foreach (var category in categories) 
                await _categoryManager.Insert(category).ConfigureAwait(false);
            return true;
        }
    }
}