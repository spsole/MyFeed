using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface ISettingManager
    {
        Task<bool> Write(Settings settings);
        
        Task<Settings> Read();
    }
}
