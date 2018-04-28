using System.IO;
using System.Threading.Tasks;

namespace myFeed.Interfaces
{
    public interface IOpmlService
    {
        Task<bool> ImportOpml(Stream stream);
        
        Task<bool> ExportOpml(Stream stream);
    }
}
