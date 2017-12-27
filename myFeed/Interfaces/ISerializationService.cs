using System.IO;

namespace myFeed.Interfaces
{
    public interface ISerializationService
    {
        void Serialize<TObject>(TObject instance, Stream fileStream);

        TObject Deserialize<TObject>(Stream fileStream);
    }
}