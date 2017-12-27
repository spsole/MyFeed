using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface IFavoriteManager
    {
        Task<IEnumerable<Article>> GetAllAsync();
        
        Task InsertAsync(Article article);

        Task RemoveAsync(Article article);
    }
}
