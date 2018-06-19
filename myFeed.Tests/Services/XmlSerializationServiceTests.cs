using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using myFeed.Models;
using myFeed.Services;
using myFeed.Tests.Attributes;
using Xunit;

namespace myFeed.Tests.Services
{
    public sealed class XmlSerializationServiceTests
    {
        [Theory]
        [InlineData("Word")]
        [InlineData("Phrase with spaces")]
        [CleanUpFile("xml-serialization")]
        public async Task ShouldSerializeTypedObjectsIntoXml(string value)
        {
            const string file = "xml-serialization";
            var serializationService = new XmlSerializationService();
            
            var fileStream = File.OpenWrite(file);
            var opmlInstance = new Opml {Head = new OpmlHead {Title = value}};
            await serializationService.Serialize(opmlInstance, fileStream);
            
            var contents = await File.ReadAllTextAsync(file);
            var part = $"<title>{value}</title>";
            contents.Should().Contain(part);
        }

        [Theory]
        [InlineData("Word")]
        [InlineData("Phrase with spaces")]
        [CleanUpFile("xml-serialization")]
        public async Task ShouldDeserializeTypedObjectsFromXml(string value)
        {
            const string file = "xml-serialization";
            var serializationService = new XmlSerializationService();
            
            var write = File.OpenWrite(file);
            var writeInstance = new Opml {Head = new OpmlHead {Title = value}};
            await serializationService.Serialize(writeInstance, write);

            var read = File.OpenRead(file);
            var readInstance = await serializationService.Deserialize<Opml>(read);
            readInstance.Head.Title.Should().Be(value);
        }
    }
}
