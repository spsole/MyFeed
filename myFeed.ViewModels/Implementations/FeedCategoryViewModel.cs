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
            IFeedService feedService,
            ISettingsService settingsService,
            IPlatformService platformService,
            IArticlesRepository articlesRepository)
        {
            Items = new ObservableCollection<FeedItemViewModel>();
            Title = new ObservableProperty<string>(entity.Title);
            IsLoading = new ObservableProperty<bool>(true);
            IsEmpty = new ObservableProperty<bool>(false);
            Fetch = new ActionCommand(async () =>
            {
                IsLoading.Value = true;
                var sources = entity.Sources;
                var orderedArticles = await feedService.RetrieveFeedsAsync(sources);
                Items.Clear();
                foreach (var article in orderedArticles)
                    Items.Add(new FeedItemViewModel(article, 
                        settingsService, platformService, articlesRepository));
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
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Fetches data for current feed.
        /// </summary>
        public ActionCommand Fetch { get; }
    }
}