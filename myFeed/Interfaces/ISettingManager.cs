using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface ISettingManager
    {
        Task Write(Settings settings);
        
        Task<Settings> Read();
    }
}
