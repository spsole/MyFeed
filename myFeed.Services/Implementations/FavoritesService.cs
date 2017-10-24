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
            if (article.Fave) return;
            article.Fave = true;
            await _favoritesRepository.InsertAsync(article);
            await _categoriesRepository.UpdateArticleAsync(article);
        }

        public async Task Remove(Article article)
        {
            if (!article.Fave) return;
            article.Fave = false;
            await _favoritesRepository.RemoveAsync(article);
            await _categoriesRepository.UpdateArticleAsync(article);
        }
    }
}
