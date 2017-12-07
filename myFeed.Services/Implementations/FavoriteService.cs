using System.ComponentModel.Composition;
using myFeed.Services.Abstractions;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFavoriteService))]
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

        public async Task InsertAsync(Article article)
        {
            if (article.Fave) return;
            article.Fave = true;
            await _favoritesRepository.InsertAsync(article);
            await _categoriesRepository.UpdateArticleAsync(article);
        }

        public async Task RemoveAsync(Article article)
        {
            if (!article.Fave) return;
            article.Fave = false;
            await _favoritesRepository.RemoveAsync(article);
            await _categoriesRepository.UpdateArticleAsync(article);
        }
    }
}
