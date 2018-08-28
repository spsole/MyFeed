using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DryIocAttributes;
using MyFeed.Interfaces;

namespace MyFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(ISerializationService))]
    public sealed class XmlSerializationService : ISerializationService
    {
        public Task Serialize<TObject>(TObject instance, Stream stream) => Task.Run(() =>
        {
            var serializer = new XmlSerializer(typeof(TObject));
            using (stream) serializer.Serialize(stream, instance);
        });

        public Task<TObject> Deserialize<TObject>(Stream stream) => Task.Run(() =>
        {
            var serializer = new XmlSerializer(typeof(TObject));
            using (stream) return (TObject) serializer.Deserialize(stream);
        });
    }
}