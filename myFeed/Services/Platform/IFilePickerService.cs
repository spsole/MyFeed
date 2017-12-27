using System.IO;
using System.Threading.Tasks;

namespace myFeed.Services.Platform
{
    public interface IFilePickerService
    {
        Task<Stream> PickFileForReadAsync();

        Task<Stream> PickFileForWriteAsync();
    }
}