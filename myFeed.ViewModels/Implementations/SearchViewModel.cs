using System.Collections.ObjectModel;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations {
    /// <summary>
    /// Search page ViewModel.
    /// </summary>
    public sealed class SearchViewModel {
        /// <summary>
        /// Instantiates new ViewModel.
        /// </summary>
        public SearchViewModel(
            ISourcesRepository sourcesRepository,
            IPlatformProvider platformProvider,
            ISearchService searchService) {

            Items = new ObservableCollection<SearchItemViewModel>();
            SearchQuery = new ObservableProperty<string>(string.Empty);
            IsLoading = new ObservableProperty<bool>(false);
            IsEmpty = new ObservableProperty<bool>(true);

            Fetch = new ActionCommand(async () => {
                IsLoading.Value = true;
                var query = SearchQuery.Value;
                var searchResults = await searchService.Search(query);
                Items.Clear();
                foreach (var result in searchResults.Results) {
                    var viewModel = new SearchItemViewModel(
                        result, platformProvider, sourcesRepository);
                    Items.Add(viewModel);
                }
                await Task.Delay(500);
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Loaded models.
        /// </summary>
        public ObservableCollection<SearchItemViewModel> Items { get; }

        /// <summary>
        /// Contains search query.
        /// </summary>
        public ObservableProperty<string> SearchQuery { get; }

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// Is collection empty or not?
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Fetches results from Feedly search engine.
        /// </summary>
        public ActionCommand Fetch { get; }
    }
}
