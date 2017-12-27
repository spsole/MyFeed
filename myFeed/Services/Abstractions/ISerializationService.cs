using System.IO;

namespace myFeed.Services.Abstractions
{
    public interface ISerializationService
    {
        void Serialize<TObject>(TObject instance, Stream fileStream);

        TObject Deserialize<TObject>(Stream fileStream);
    }
}