using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface IFavoriteManager
    {
        Task<IEnumerable<Article>> GetAll();
        
        Task Insert(Article article);

        Task Remove(Article article);
    }
}
