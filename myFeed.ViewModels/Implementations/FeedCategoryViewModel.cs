using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations {
    /// <summary>
    /// Represents feed items collection view model.
    /// </summary>
    public sealed class FeedCategoryViewModel {
        /// <summary>
        /// Instantiates ViewModel.
        /// </summary>
        public FeedCategoryViewModel(
            SourceCategoryEntity entity,
            IFeedService feedService,
            IPlatformProvider platformProvider,
            IArticlesRepository articlesRepository) {

            Items = new ObservableCollection<FeedItemViewModel>();
            Title = new ReadOnlyProperty<string>(entity.Title);
            IsLoading = new ObservableProperty<bool>(true);
            IsEmpty = new ObservableProperty<bool>(false);

            Fetch = new ActionCommand(async () => {
                IsLoading.Value = true;
                var orderedArticles = await feedService.RetrieveFeedsAsync(entity.Sources);
                Items.Clear();
                foreach (var article in orderedArticles) {
                    var viewModel = new FeedItemViewModel(
                        article, platformProvider, articlesRepository);
                    Items.Add(viewModel);
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Feed items received from fetcher.
        /// </summary>
        public ObservableCollection<FeedItemViewModel> Items { get; }

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
        public ReadOnlyProperty<string> Title { get; }

        /// <summary>
        /// Fetches data for current feed.
        /// </summary>
        public ActionCommand Fetch { get; }
    }
}