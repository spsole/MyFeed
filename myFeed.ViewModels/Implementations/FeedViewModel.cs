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
            OpenSources = new Command(navigationService.Navigate<SourcesViewModel>);
            Items = new ObservableCollection<FeedCategoryViewModel>();
            IsLoading = new Property<bool>(true);
            IsEmpty = new Property<bool>(false);
            Load = new Command(async () =>
            {
                IsEmpty.Value = false;
                IsLoading.Value = true;
                await articlesRepository.RemoveUnreferencedArticles();
                var sources = await sourcesRepository.GetAllAsync();
                Items.Clear();
                foreach (var source in sources)
                    Items.Add(new FeedCategoryViewModel(source, dialogService, settingsService, 
                        platformService, feedStoreService, navigationService, 
                        articlesRepository, translationsService));
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
        public Property<bool> IsLoading { get; }

        /// <summary>
        /// True if collection is empty.
        /// </summary>
        public Property<bool> IsEmpty { get; }

        /// <summary>
        /// Opens sources page as a proposal for user 
        /// to add new RSS categories into myFeed.
        /// </summary>
        public Command OpenSources { get; }

        /// <summary>
        /// Loads all feeds into items property.
        /// </summary>
        public Command Load { get; }
    }
}