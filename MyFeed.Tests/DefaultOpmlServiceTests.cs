using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Services;
using NSubstitute;
using Xunit;

namespace MyFeed.Tests
{
    public sealed class DefaultOpmlServiceTests
    {
        private readonly ISerializationService _serializationService = Substitute.For<ISerializationService>();
        private readonly ICategoryManager _categoryManager = Substitute.For<ICategoryManager>();
        private readonly IOpmlService _opmlService;

        public DefaultOpmlServiceTests() => _opmlService = new DefaultOpmlService(
            _serializationService, _categoryManager
        );
        
        [Theory]
        [InlineData("Bar", "https://", "site.com", "/feed")]
        [InlineData("Abc", "https://", "example.com", "/news")]
        [InlineData("Yii", "http://", "sub.domained.web", "/rss")]
        public async Task ShouldExportOpmlFeedsIntoXml(string category, string protocol, string domain, string path)
        {
            Opml opml = null;
            _serializationService
                .When(x => x.Serialize(Arg.Any<Opml>(), Arg.Any<Stream>()))
                .Do(callback => opml = callback.Arg<Opml>());
            
            var url = $"{protocol}{domain}{path}";
            var channels = new List<Channel> {new Channel {Uri = url}};
            var categories = new List<Category> {new Category {Title = category, Channels = channels}};
            _categoryManager.GetAll().Returns(categories);
            await _opmlService.ExportOpml(new MemoryStream());
            
            opml.Head.Should().NotBeNull();
            opml.Body.Count.Should().Be(1);
            opml.Body[0].Title.Should().Be(category);
            opml.Body[0].ChildOutlines.Count.Should().Be(1);
            opml.Body[0].ChildOutlines[0].Title.Should().Be(domain);
            opml.Body[0].ChildOutlines[0].HtmlUrl.Should().Be(protocol + domain);
            opml.Body[0].ChildOutlines[0].XmlUrl.Should().Be(url);
        }

        [Theory]
        [InlineData("http://foo.bar/rss")]
        [InlineData("https://buy.some.beer/feed")]
        [InlineData("https://long-domain.any/news")]
        public async Task ShouldBeAbleToImportOpmlFeeds(string url)
        {
            Category category = null;
            _categoryManager
                .When(x => x.Insert(Arg.Any<Category>()))
                .Do(callback => category = callback.Arg<Category>());
            
            var opml = new Opml {Body = new List<OpmlOutline> {new OpmlOutline {XmlUrl = url}}};
            _serializationService.Deserialize<Opml>(Arg.Any<Stream>()).Returns(opml);
            await _opmlService.ImportOpml(new MemoryStream());
            
            category.Channels.Count.Should().Be(1);
            category.Channels[0].Uri.Should().Be(url);
            category.Channels[0].Notify.Should().BeTrue();
        }
    }
}