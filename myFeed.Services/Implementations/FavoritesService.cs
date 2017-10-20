using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.Repositories.Models;
using System.Threading.Tasks;

namespace myFeed.Services.Implementations
{
    public sealed class FavoritesService : IFavoritesService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IFavoritesRepository _favoritesRepository;

        public FavoritesService(
            ICategoriesRepository categoriesRepository,
            IFavoritesRepository favoritesRepository)
        {
            _categoriesRepository = categoriesRepository;
            _favoritesRepository = favoritesRepository;
        }

        public async Task Insert(Article article)
        {
            if (article.Fave == true) return;
            article.Fave = true;
            await _favoritesRepository.InsertAsync(article);
            await UpdateIfExists(article);
        }

        public async Task Remove(Article article)
        {
            if (article.Fave == false) return;
            article.Fave = false;
            await _favoritesRepository.RemoveAsync(article);
            await UpdateIfExists(article);
        }

        private async Task UpdateIfExists(Article article)
        {
            try 
            {
                await _categoriesRepository.UpdateArticleAsync(article);
            }
            catch  
            {
                // ignored
            }
        }
    }
}
