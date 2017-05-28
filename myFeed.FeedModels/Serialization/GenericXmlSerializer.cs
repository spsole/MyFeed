using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace myFeed.FeedModels.Serialization
{
    /// <summary>
    /// Serializes and deserializes objects.
    /// </summary>
    public static class GenericXmlSerializer
    {
        /// <summary>
        /// Dumps object of a given type to xml.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="serializableObject">Object to serialize</param>
        /// <param name="file">File to write</param>
        public static async void SerializeObject<T>(T serializableObject, StorageFile file)
        {
            try
            {
                // Clear the entire file.
                await FileIO.WriteTextAsync(file, string.Empty);

                // Serialize object to an empty file.
                var serializer = new XmlSerializer(typeof(T));
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    serializer.Serialize(stream, serializableObject);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
        }

        /// <summary>
        /// Deserializes object of a given type.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="file">Read from file</param>
        /// <returns>Task returning an object of T</returns>
        public static async Task<T> DeSerializeObject<T>(StorageFile file)
        {
            var filestring = await FileIO.ReadTextAsync(file);
            var objectOut = default(T);
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StringReader(filestring))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            return objectOut;
        }
    }
}
