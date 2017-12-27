using System.IO;
using System.Threading.Tasks;

namespace myFeed.Interfaces
{
    public interface IOpmlService
    {
        Task<bool> ImportOpmlFeedsAsync(Stream stream);
        
        Task<bool> ExportOpmlFeedsAsync(Stream stream);
    }
}
