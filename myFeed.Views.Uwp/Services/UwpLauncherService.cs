using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Implementations;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpLauncherService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly INavigationService _navigationService;
        private readonly IFactoryService _factoryService;

        public UwpLauncherService(
            ICategoriesRepository categoriesRepository,
            INavigationService navigationService,
            IFactoryService factoryService)
        {
            _factoryService = factoryService;
            _categoriesRepository = categoriesRepository;
            _navigationService = navigationService;
        }

        public async Task LaunchArticleById(Guid guid)
        {
            var article = await _categoriesRepository.GetArticleByIdAsync(guid);
            if (article == null) return;
            var viewModel = _factoryService.CreateInstance<ArticleViewModel>(article);
            if (UwpNavigationService.GetChild<Frame>(Window.Current.Content, 1) == null)
            {
                await _navigationService.Navigate<FeedViewModel>();
                await Task.Delay(150);
            }
            await _navigationService.Navigate(viewModel);
        }
    }
}
