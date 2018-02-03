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
        public ReactiveList<SearchItemViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> Fetch { get; }

        public string SearchQuery { get; set; }
        public bool IsGreeting { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }

        public SearchViewModel(
            IFactoryService factoryService,             
            ISearchService searchService)
        {
            (IsGreeting, SearchQuery) = (true, string.Empty);
            Items = new ReactiveList<SearchItemViewModel>();
            this.WhenAnyValue(x => x.SearchQuery)
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Select(x => x?.Trim())
                .DistinctUntilChanged()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Fetch.Execute());

            Fetch = ReactiveCommand.CreateFromTask(async () =>
            {
                (IsLoading, IsGreeting) = (true, false);
                var search = await searchService.SearchAsync(SearchQuery);
                var factory = factoryService.Create<Func<FeedlyItem, SearchItemViewModel>>();
                var viewModels = search.Results.Select(x => factory(x));
                Items.Clear();
                Items.AddRange(viewModels);
                IsEmpty = Items.Count == 0;
                IsLoading = false;
            });
        }
    }
}