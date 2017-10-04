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
        private readonly ITranslationsService _translationsService;
        private readonly IArticlesRepository _articlesRepository;
        private readonly INavigationService _navigationService;
        private readonly IPlatformService _platformService;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;

        public UwpLauncherService(
            IDialogService dialogService,
            ISettingsService settingsService,
            IPlatformService platformService,
            INavigationService navigationService,
            IArticlesRepository articlesRepository,
            ITranslationsService translationsService)
        {
            _translationsService = translationsService;
            _articlesRepository = articlesRepository;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _platformService = platformService;
            _dialogService = dialogService;
        }

        public async Task LaunchArticleById(Guid guid)
        {
            var article = await _articlesRepository.GetByIdAsync(guid);
            if (article == null) return;
            var viewModel = new ArticleViewModel(article, _dialogService, _settingsService, 
                _platformService, _navigationService, _articlesRepository, _translationsService);
            if (UwpNavigationService.GetChild<Frame>(Window.Current.Content, 1) == null)
            {
                await _navigationService.Navigate<FeedViewModel>();
                await Task.Delay(150);
            }
            await _navigationService.Navigate(viewModel);
        }
    }
}
