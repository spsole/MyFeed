using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface IFavoriteService
    {
        Task Insert(Article article);

        Task Remove(Article article);
    }
}
