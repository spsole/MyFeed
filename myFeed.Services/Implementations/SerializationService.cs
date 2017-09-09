using System;
using System.IO;
using System.Xml.Serialization;
using myFeed.Services.Abstractions;

namespace myFeed.Services.Implementations
{
    public class SerializationService : ISerializationService
    {
        public void Serialize<T>(T instance, Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (stream) serializer.Serialize(stream, instance);
            }
            catch (Exception)
            {
                // ignore
            }
            finally
            {
                stream.Dispose();
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            try
            {
                T objectOut;
                var serializer = new XmlSerializer(typeof(T));
                using (stream) objectOut = (T) serializer.Deserialize(stream);
                return objectOut;
            }
            catch (Exception)
            {
                return default(T);
            }
            finally
            {
                stream.Dispose();
            }
        }
    }
}