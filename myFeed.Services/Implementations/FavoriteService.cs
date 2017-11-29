using myFeed.Services.Abstractions;
using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    public sealed class FavoriteService : IFavoriteService
    {
        private readonly ICategoryStoreService _categoriesRepository;
        private readonly IFavoriteStoreService _favoritesRepository;

        public FavoriteService(
            ICategoryStoreService categoriesRepository,
            IFavoriteStoreService favoritesRepository)
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
