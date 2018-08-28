using System.Threading.Tasks;
using MyFeed.Models;

namespace MyFeed.Interfaces
{
    public interface ISettingManager
    {
        Task Write(Settings settings);
        
        Task<Settings> Read();
    }
}
