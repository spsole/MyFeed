using System.IO;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IFilePickerService
    {
        Task<Stream> PickFileForReadAsync();

        Task<Stream> PickFileForWriteAsync();
    }
}