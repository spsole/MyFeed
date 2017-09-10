using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    /// <summary>
    /// All feeds ViewModel.
    /// </summary>
    public sealed class FeedViewModel
    {
        /// <summary>
        /// Instantiates ViewModel.
        /// </summary>
        public FeedViewModel(
            IFeedService feedService,
            IPlatformProvider platformProvider,
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
                        feedService, platformProvider, articlesRepository);
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