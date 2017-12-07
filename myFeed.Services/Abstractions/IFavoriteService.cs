using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface IFavoriteService
    {
        Task InsertAsync(Article article);

        Task RemoveAsync(Article article);
    }
}
