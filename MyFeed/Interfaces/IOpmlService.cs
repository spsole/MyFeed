using System.IO;
using System.Threading.Tasks;

namespace MyFeed.Interfaces
{
    public interface IOpmlService
    {
        Task<bool> ImportOpml(Stream stream);
        
        Task<bool> ExportOpml(Stream stream);
    }
}
