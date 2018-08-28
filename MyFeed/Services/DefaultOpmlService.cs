using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;

namespace MyFeed.Services
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
            var opml = new Opml { Head = new OpmlHead { Title = "Feeds from MyFeed App" } };
            var categories = await _categoryManager.GetAll().ConfigureAwait(false);
            var outlines = categories.Select(category => new OpmlOutline
            {
                Text = category.Title,
                Title = category.Title,
                ChildOutlines = category.Channels
                .Select(channel => new { Entity = channel, Uri = new Uri(channel.Uri) })
                .Select(entityWithUri => new OpmlOutline
                {
                    HtmlUrl = $"{entityWithUri.Uri.Scheme}://{entityWithUri.Uri.Host}",
                    XmlUrl = entityWithUri.Uri.ToString(),
                    Title = entityWithUri.Uri.Host,
                    Text = entityWithUri.Uri.Host,
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
                .Where(outline => outline.XmlUrl == null && outline.HtmlUrl == null)
                .Select(outline => new { Title = outline.Title ?? outline.Text, Outline = outline })
                .Where(namedOutline => namedOutline.Title != null)
                .Select(namedOutline => new Category
                {
                    Title = namedOutline.Title,
                    Channels = namedOutline.Outline.ChildOutlines
                    .Select(o => new Channel { Uri = o.XmlUrl, Notify = true })
                    .ToList()
                })
                .ToList();

            var uncategorized = new Category
            {
                Title = "Unknown",
                Channels = opml.Body
                .Where(outline => Uri.IsWellFormedUriString(outline.XmlUrl, UriKind.Absolute))
                .Select(outline => new Channel { Uri = outline.XmlUrl, Notify = true })
                .ToList()
            };
            if (uncategorized.Channels.Any()) categories.Add(uncategorized);
            foreach (var category in categories) 
                await _categoryManager.Insert(category).ConfigureAwait(false);
            return true;
        }
    }
}