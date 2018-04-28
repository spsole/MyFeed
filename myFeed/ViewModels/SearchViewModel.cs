using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SearchViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SearchViewModel
    {
        public ReactiveList<SearchItemViewModel> Items { get; }
        public Interaction<Exception, bool> Error { get; }
        public ReactiveCommand<Unit, Unit> Fetch { get; }

        public string SearchQuery { get; set; } = string.Empty;
        public bool IsGreeting { get; private set; } = true;
        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }

        public SearchViewModel(
            Func<FeedlyItem, SearchItemViewModel> factory,
            ISearchService searchService)
        {
            Items = new ReactiveList<SearchItemViewModel>();
            Fetch = ReactiveCommand.CreateFromTask(() => DoFetch(factory, searchService));
            this.WhenAnyValue(x => x.SearchQuery)
                .Select(x => x?.Trim())
                .DistinctUntilChanged()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromSeconds(0.8))
                .Select(x => Unit.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(Fetch);

            Error = new Interaction<Exception, bool>();
            Fetch.ThrownExceptions
                .SelectMany(x => Error.Handle(x))
                .Where(retry => retry)
                .Select(x => Unit.Default)
                .InvokeCommand(Fetch);
        }

        private async Task DoFetch(
            Func<FeedlyItem, SearchItemViewModel> factory,
            ISearchService searchService)
        {
            IsLoading = true;
            var search = await searchService.Search(SearchQuery);
            var viewModels = search.Results.Select(factory);
            Items.Clear();
            Items.AddRange(viewModels);
            IsEmpty = Items.Count == 0;
            IsGreeting = IsLoading = false;
        }
    }
}