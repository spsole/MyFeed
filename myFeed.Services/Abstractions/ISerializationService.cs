using System.IO;

namespace myFeed.Services.Abstractions {
    /// <summary>
    /// Provides xml serialization mechanics.
    /// </summary>
    public interface ISerializationService {
        /// <summary>
        /// Serializes object into Stream.
        /// </summary> 
        void Serialize<T>(T instance, Stream fileStream);

        /// <summary>
        /// Deserializes object from Stream.
        /// </summary>
        T Deserialize<T>(Stream fileStream);
    }
}