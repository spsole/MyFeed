using System.Linq;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedCategoryViewModel
    {
        public FeedCategoryViewModel(
            SourceCategoryEntity entity,
            IDialogService dialogService,
            ISettingsService settingsService,
            IPlatformService platformService,
            IFeedStoreService feedStoreService,
            INavigationService navigationService,
            IArticlesRepository articlesRepository,
            ITranslationsService translationsService)
        {
            OpenSources = new Command(navigationService.Navigate<SourcesViewModel>);
            Items = new Collection<ArticleViewModel>();
            Title = new Property<string>(entity.Title);
            IsLoading = new Property<bool>(true);
            IsEmpty = new Property<bool>(false);
            Fetch = new Command(async () =>
            {
                IsLoading.Value = true;
                var sources = entity.Sources;
                (var errors, var articles) = await feedStoreService.GetAsync(sources);
                var articleViewModels = articles
                    .Select(i => new ArticleViewModel(i, 
                        dialogService, settingsService, platformService, 
                        navigationService, articlesRepository, translationsService));

                Items.Clear();
                Items.AddRange(articleViewModels);
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Feed items received from fetcher.
        /// </summary>
        public Collection<ArticleViewModel> Items { get; }

        /// <summary>
        /// Indicates if fetcher is loading data right now.
        /// </summary>
        public Property<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public Property<bool> IsEmpty { get; }

        /// <summary>
        /// Feed category title.
        /// </summary>
        public Property<string> Title { get; }

        /// <summary>
        /// Opens sources page as a proposal for user 
        /// to add new RSS channels into this category.
        /// </summary>
        public Command OpenSources { get; }

        /// <summary>
        /// Fetches data for current feed.
        /// </summary>
        public Command Fetch { get; }
    }
}