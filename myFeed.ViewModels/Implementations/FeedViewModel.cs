using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedViewModel
    {
        public FeedViewModel(
            IFeedService feedService,
            ISettingsService settingsService,
            IPlatformService platformService,
            INavigationService navigationService,
            ISourcesRepository sourcesRepository,
            IArticlesRepository articlesRepository)
        {
            Items = new ObservableCollection<FeedCategoryViewModel>();
            IsLoading = new Property<bool>(false);
            Load = new Command(async () =>
            {
                IsLoading.Value = true;
                var sources = await sourcesRepository.GetAllAsync();
                Items.Clear();
                foreach (var source in sources)
                    Items.Add(new FeedCategoryViewModel(source, feedService, settingsService, 
                        platformService, navigationService, articlesRepository));
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
        /// Loads all feeds into items property.
        /// </summary>
        public Command Load { get; }
    }
}