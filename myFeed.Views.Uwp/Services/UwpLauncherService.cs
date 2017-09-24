using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Implementations;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpLauncherService
    {
        private readonly IArticlesRepository _articlesRepository;
        private readonly INavigationService _navigationService;
        private readonly IPlatformService _platformService;
        private readonly ISettingsService _settingsService;

        public UwpLauncherService(
            ISettingsService settingsService,
            IPlatformService platformService,
            INavigationService navigationService,
            IArticlesRepository articlesRepository)
        {
            _articlesRepository = articlesRepository;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _platformService = platformService;
        }

        public async Task LaunchArticleById(Guid guid)
        {
            var article = await _articlesRepository.GetByIdAsync(guid);
            if (article == null) return;
            var viewModel = new FeedItemViewModel(article, _settingsService, 
                _platformService, _navigationService, _articlesRepository);
            if (UwpNavigationService.GetChild<Frame>(Window.Current.Content, 1) == null)
            {
                await _navigationService.Navigate(typeof(FeedViewModel));
                await Task.Delay(150);
            }
            await _navigationService.Navigate(typeof(ArticleViewModel), viewModel);
        }
    }
}
