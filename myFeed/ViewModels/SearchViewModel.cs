using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
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
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;
        private readonly ISearchService _searchService;
        
        public ReactiveCommand<Unit, FeedlyRoot> Search { get; }
        public ReactiveList<SearchItemViewModel> Feeds { get; }
        public SearchItemViewModel SelectedFeed { get; set; }
        public ReactiveCommand<Unit, Unit> Add { get; }
        public Interaction<Unit, bool> Added { get; }
        
        public ReactiveCommand<Unit, IEnumerable<Category>> RefreshCategories { get; }
        public ReactiveCommand<Unit, Unit> ViewCategories { get; }
        public ReactiveList<Category> Categories { get; }
        public Category SelectedCategory { get; set; }
        public Interaction<Exception, bool> Error { get; }

        public string SearchQuery { get; set; } = string.Empty;
        public bool IsGreeting { get; private set; } = true;
        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }

        public SearchViewModel(
            Func<FeedlyItem, SearchItemViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            ISearchService searchService)
        {
            _factory = factory;
            _categoryManager = categoryManager;
            _navigationService = navigationService;
            _searchService = searchService;
            
            Feeds = new ReactiveList<SearchItemViewModel>();
            Search = ReactiveCommand.CreateFromTask(
                () => _searchService.Search(SearchQuery),
                this.WhenAnyValue(x => x.SearchQuery)
                    .Select(x => !string.IsNullOrWhiteSpace(x)));
            
            Search.Select(response => response.Results.Select(_factory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(feeds => Feeds.Clear())
                .Subscribe(Feeds.AddRange);

            Search.IsExecuting.Skip(1)
                .Do(x => IsGreeting = false)
                .Subscribe(x => IsLoading = x);
            Feeds.IsEmptyChanged
                .Subscribe(x => IsEmpty = x);
      
            Categories = new ReactiveList<Category>();
            ViewCategories = ReactiveCommand.CreateFromTask(_navigationService.Navigate<ChannelViewModel>);
            RefreshCategories = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            RefreshCategories
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(categories => Categories.Clear())
                .Subscribe(Categories.AddRange);
            
            Error = new Interaction<Exception, bool>();
            Search.ThrownExceptions
                .SelectMany(Error.Handle)
                .Where(retry => retry)
                .Select(x => Unit.Default)
                .InvokeCommand(Search);
            RefreshCategories.ThrownExceptions
                .SelectMany(Error.Handle)
                .Where(retry => retry)
                .Select(x => Unit.Default)
                .InvokeCommand(RefreshCategories);
            
            this.WhenAnyValue(x => x.SearchQuery)
                .Throttle(TimeSpan.FromSeconds(0.8))
                .Select(x => x?.Trim())
                .DistinctUntilChanged()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Unit.Default)
                .InvokeCommand(Search);

            this.WhenAnyValue(x => x.SelectedFeed)
                .Do(feed => Feeds.ToList().ForEach(x => x.IsSelected = false))
                .Where(selection => selection != null)
                .Subscribe(x => x.IsSelected = true);
            
            Added = new Interaction<Unit, bool>();
            Add = ReactiveCommand.CreateFromTask(DoAdd,
                this.WhenAnyValue(x => x.SelectedCategory, x => x.SelectedFeed)
                    .Select(sel => sel.Item1 != null && sel.Item2 != null)
                    .DistinctUntilChanged());

            Add.ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(Added.Handle)
                .Subscribe(x => SelectedFeed = null);
        }

        private async Task DoAdd()
        {
            var url = SelectedFeed.Feed?.Substring(5);
            var channel = new Channel {Notify = true, Uri = url};
            SelectedCategory.Channels.Add(channel);
            await _categoryManager.Update(SelectedCategory);
        }
    }
}