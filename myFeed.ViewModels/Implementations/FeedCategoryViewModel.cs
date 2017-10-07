using System.Collections.ObjectModel;
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
            OpenSources = new ObservableCommand(navigationService.Navigate<SourcesViewModel>);
            Items = new ObservableCollection<ArticleViewModel>();
            Title = new ObservableProperty<string>(entity.Title);
            IsLoading = new ObservableProperty<bool>(true);
            IsEmpty = new ObservableProperty<bool>(false);
            Fetch = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var sources = entity.Sources;
                (var errors, var articles) = await feedStoreService.GetAsync(sources);
                Items.Clear();
                foreach (var article in articles)
                {
                    var viewModel = new ArticleViewModel(article,
                        dialogService, settingsService, platformService,
                        navigationService, articlesRepository, translationsService);
                    Items.Add(viewModel);
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Feed items received from fetcher.
        /// </summary>
        public ObservableCollection<ArticleViewModel> Items { get; }

        /// <summary>
        /// Indicates if fetcher is loading data right now.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Feed category title.
        /// </summary>
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Opens sources page as a proposal for user 
        /// to add new RSS channels into this category.
        /// </summary>
        public ObservableCommand OpenSources { get; }

        /// <summary>
        /// Fetches data for current feed.
        /// </summary>
        public ObservableCommand Fetch { get; }
    }
}