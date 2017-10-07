using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedViewModel
    {
        public FeedViewModel(
            IDialogService dialogService,
            ISettingsService settingsService,
            IPlatformService platformService,
            IFeedStoreService feedStoreService,
            INavigationService navigationService,
            ISourcesRepository sourcesRepository,
            IArticlesRepository articlesRepository,
            ITranslationsService translationsService)
        {
            OpenSources = new ObservableCommand(navigationService.Navigate<SourcesViewModel>);
            Items = new ObservableCollection<FeedCategoryViewModel>();
            IsLoading = new ObservableProperty<bool>(true);
            IsEmpty = new ObservableProperty<bool>(false);
            Load = new ObservableCommand(async () =>
            {
                IsEmpty.Value = false;
                IsLoading.Value = true;
                await articlesRepository.RemoveUnreferencedArticles();
                Items.Clear();
                var sources = await sourcesRepository.GetAllAsync();
                foreach (var source in sources)
                {
                    var viewModel = new FeedCategoryViewModel(source,
                        dialogService, settingsService, platformService,
                        feedStoreService, navigationService,
                        articlesRepository, translationsService);
                    Items.Add(viewModel);
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Feed categories viewmodels.
        /// </summary>
        public ObservableCollection<FeedCategoryViewModel> Items { get; }

        /// <summary>
        /// Indicates if viewmodel is loading items from disk.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// True if collection is empty.
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Opens sources page as a proposal for user 
        /// to add new RSS categories into myFeed.
        /// </summary>
        public ObservableCommand OpenSources { get; }

        /// <summary>
        /// Loads all feeds into items property.
        /// </summary>
        public ObservableCommand Load { get; }
    }
}