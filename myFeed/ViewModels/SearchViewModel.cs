using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
        private readonly Func<FeedlyItem, SearchItemViewModel> _factory;
        private readonly ISearchService _searchService;
        
        public ReactiveList<SearchItemViewModel> Items { get; private set; }
        public ReactiveCommand<Unit, FeedlyRoot> Fetch { get; }
        public Interaction<Exception, bool> Error { get; }

        public string SearchQuery { get; set; } = string.Empty;
        public bool IsGreeting { get; private set; } = true;
        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }

        public SearchViewModel(
            Func<FeedlyItem, SearchItemViewModel> factory,
            ISearchService searchService)
        {
            _factory = factory;
            _searchService = searchService;
            Items = new ReactiveList<SearchItemViewModel>();
            Fetch = ReactiveCommand.CreateFromTask(() => _searchService.Search(SearchQuery));
            Fetch.Select(response => response.Results.Select(_factory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(models => Items.Clear())
                .Subscribe(Items.AddRange);

            this.WhenAnyValue(x => x.SearchQuery)
                .Throttle(TimeSpan.FromSeconds(0.8))
                .Select(x => x?.Trim())
                .DistinctUntilChanged()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Unit.Default)
                .InvokeCommand(Fetch);
            
            Items.IsEmptyChanged.Subscribe(x => IsEmpty = x);
            Fetch.IsExecuting
                .Skip(count: 1)
                .Do(x => IsGreeting = false)
                .Subscribe(x => IsLoading = x);

            Error = new Interaction<Exception, bool>();
            Fetch.ThrownExceptions
                .SelectMany(x => Error.Handle(x))
                .Where(retry => retry)
                .Select(x => Unit.Default)
                .InvokeCommand(Fetch);
        }
    }
}