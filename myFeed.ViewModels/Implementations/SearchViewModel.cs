using System.Linq;
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
            Items = new Collection<SearchItemViewModel>();
            SearchQuery = new Property<string>(string.Empty);
            IsLoading = new Property<bool>(false);
            IsEmpty = new Property<bool>(true);
            Fetch = new Command(async () =>
            {
                IsLoading.Value = true;
                var query = SearchQuery.Value;
                var searchResults = await searchService.Search(query);
                var searchViewModels = searchResults.Results
                    .Select(i => new SearchItemViewModel(i, 
                        dialogService, platformService, sourcesRepository));

                Items.Clear();
                Items.AddRange(searchViewModels);
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Loaded models.
        /// </summary>
        public Collection<SearchItemViewModel> Items { get; }

        /// <summary>
        /// Contains search query.
        /// </summary>
        public Property<string> SearchQuery { get; }

        /// <summary>
        /// Is collection being loaded or not.
        /// </summary>
        public Property<bool> IsLoading { get; }

        /// <summary>
        /// Is collection empty or not?
        /// </summary>
        public Property<bool> IsEmpty { get; }

        /// <summary>
        /// Fetches results from Feedly search engine.
        /// </summary>
        public Command Fetch { get; }
    }
}