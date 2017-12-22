using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface IFavoriteManager
    {
        Task<IEnumerable<Article>> GetAllAsync();
        
        Task InsertAsync(Article article);

        Task RemoveAsync(Article article);
    }
}
