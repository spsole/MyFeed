using System.IO;
using System.Threading.Tasks;

namespace MyFeed.Interfaces
{
    public interface ISerializationService
    {
        Task Serialize<TObject>(TObject instance, Stream stream);

        Task<TObject> Deserialize<TObject>(Stream stream);
    }
}