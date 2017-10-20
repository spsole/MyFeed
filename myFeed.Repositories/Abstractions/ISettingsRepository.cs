using System.Threading.Tasks;
using myFeed.Repositories.Models;

namespace myFeed.Repositories.Abstractions
{
    public interface ISettingsRepository
    {
        Task<Setting> GetByKeyAsync(string key);
        Task InsertAsync(Setting setting);
        Task UpdateAsync(Setting setting);
    }
}