using myFeed.Repositories.Models;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    public interface IFavoritesService
    {
        Task Insert(Article article);

        Task Remove(Article article);
    }
}
