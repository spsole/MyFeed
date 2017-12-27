using System.IO;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    public interface IOpmlService
    {
        Task<bool> ImportOpmlFeedsAsync(Stream stream);
        
        Task<bool> ExportOpmlFeedsAsync(Stream stream);
    }
}
