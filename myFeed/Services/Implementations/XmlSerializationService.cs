using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Serialization;
using DryIocAttributes;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ISerializationService))]
    public sealed class XmlSerializationService : ISerializationService
    {
        public void Serialize<TObject>(TObject instance, Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(TObject));
                using (stream) serializer.Serialize(stream, instance);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public TObject Deserialize<TObject>(Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(TObject));
                using (stream) return (TObject) serializer.Deserialize(stream);
            }
            catch (Exception)
            {
                return default(TObject);
            }
        }
    }
}