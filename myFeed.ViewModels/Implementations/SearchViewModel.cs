using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SearchViewModel
    {
        public SearchViewModel(
            ISourcesRepository sourcesRepository,
            IPlatformService platformService,
            IDialogService dialogService,
            ISearchService searchService)
        {
            Items = new ObservableCollection<SearchItemViewModel>();
            SearchQuery = new ObservableProperty<string>(string.Empty);
            IsGreeting = new ObservableProperty<bool>(true);
            IsLoading = new ObservableProperty<bool>(false);
            IsEmpty = new ObservableProperty<bool>(true);
            Fetch = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var query = SearchQuery.Value;
                var searchResults = await searchService.Search(query);
                IsGreeting.Value = false;
                Items.Clear();
                foreach (var result in searchResults.Results)
                {
                    var viewModel = new SearchItemViewModel(result, 
                        dialogService, platformService, sourcesRepository);
                    Items.Add(viewModel);
                }
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
        /// Indicates if ViewModel is showing greeting.
        /// </summary>
        public ObservableProperty<bool> IsGreeting { get; }

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
        public ObservableCommand Fetch { get; }
    }
}