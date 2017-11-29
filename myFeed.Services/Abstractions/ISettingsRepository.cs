using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface ISettingsRepository
    {
        Task<Setting> GetByKeyAsync(string key);
        Task InsertAsync(Setting setting);
        Task UpdateAsync(Setting setting);
    }
}