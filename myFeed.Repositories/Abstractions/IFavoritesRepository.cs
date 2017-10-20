using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Repositories.Models;

namespace myFeed.Repositories.Abstractions
{
    public interface IFavoritesRepository
    {
        Task<IEnumerable<Article>> GetAllAsync();
        Task InsertAsync(Article article);
        Task RemoveAsync(Article article);
        Task UpdateAsync(Article article);
    }
}