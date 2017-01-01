using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;

namespace myFeed
{
    public static class SerializerExtensions
    {
        public static async void SerializeObject<T>(T serializableObject, StorageFile file)
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringWriter stringWriter = new StringWriter())

                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, serializableObject);
                    string ready = stringWriter.ToString();
                    await FileIO.WriteTextAsync(file, ready);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.Write(ex.ToString());
#endif
            }
        }

        public static async Task<T> DeSerializeObject<T>(StorageFile file)
        {
            string filestring = await FileIO.ReadTextAsync(file);
            T objectOut = default(T);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StringReader(filestring))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.Write(ex.ToString());
#endif
            }
            return objectOut;
        }
    }
}
