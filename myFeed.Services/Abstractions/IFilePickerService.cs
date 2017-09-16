using System.IO;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Platform-specific file picker.
    /// </summary>
    public interface IFilePickerService
    {
        /// <summary>
        /// Picks single file for read.
        /// </summary>
        Task<Stream> PickFileForReadAsync();

        /// <summary>
        /// Picks single file for write.
        /// </summary>
        Task<Stream> PickFileForWriteAsync();
    }
}