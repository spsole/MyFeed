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
            IPlatformService platformService,
            ISourcesRepository sourcesRepository,
            IArticlesRepository articlesRepository)
        {
            IsLoading = new ObservableProperty<bool>(false);
            Items = new ObservableCollection<FeedCategoryViewModel>();
            Load = new ActionCommand(async () =>
            {
                IsLoading.Value = true;
                var sources = await sourcesRepository.GetAllAsync();
                Items.Clear();
                foreach (var source in sources)
                {
                    var viewModel = new FeedCategoryViewModel(source,
                        feedService, platformService, articlesRepository);
                    Items.Add(viewModel);
                }
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
        /// Loads all feeds into items property.
        /// </summary>
        public ActionCommand Load { get; }
    }
}