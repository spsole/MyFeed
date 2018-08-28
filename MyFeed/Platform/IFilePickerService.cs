using System.IO;
using System.Threading.Tasks;

namespace MyFeed.Platform
{
    public interface IFilePickerService
    {
        Task<Stream> PickFileForReadAsync();

        Task<Stream> PickFileForWriteAsync();
    }
}