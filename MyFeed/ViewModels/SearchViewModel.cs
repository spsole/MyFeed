using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive.Disposables;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using DryIocAttributes;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(SearchViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class SearchViewModel : ISupportsActivation
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
        public ViewModelActivator Activator { get; }

        public string SearchQuery { get; set; } = string.Empty;
        public bool IsGreeting { get; private set; } = true;
        public bool HasErrors { get; private set; }
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
            
            Feeds.IsEmptyChanged
                .Subscribe(x => IsEmpty = x);
            Search.Select(response => response.Results.Select(_factory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(feeds => Feeds.Clear())
                .Subscribe(Feeds.AddRange);
            
            Search.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Search.IsExecuting.Skip(1)
                .Select(executing => false)
                .Subscribe(x => IsGreeting = x);
            Search.IsExecuting
                .Where(executing => executing)
                .Subscribe(x => SelectedFeed = null);

            Search.IsExecuting
                .Where(executing => executing)
                .Select(executing => false)
                .Subscribe(x => HasErrors = x);
            Search.ThrownExceptions
                .Select(exception => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HasErrors = x);
            Search.ThrownExceptions
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Feeds.Clear());
      
            Categories = new ReactiveList<Category>();
            ViewCategories = ReactiveCommand.CreateFromTask(_navigationService.Navigate<ChannelViewModel>);
            RefreshCategories = ReactiveCommand.CreateFromTask(_categoryManager.GetAll);
            RefreshCategories
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(categories => Categories.Clear())
                .Subscribe(Categories.AddRange);
            
            this.WhenAnyValue(x => x.SearchQuery)
                .Throttle(TimeSpan.FromSeconds(0.8))
                .Select(query => query?.Trim())
                .DistinctUntilChanged()
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(query => Unit.Default)
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

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
                RefreshCategories.Execute().Subscribe());
        }

        private async Task DoAdd()
        {
            var url = SelectedFeed.Feed?.Substring(5);
            var channel = new Channel {Notify = true, Uri = url};
            SelectedCategory.Channels.Add(channel);

            await _categoryManager.Update(SelectedCategory);
            await Added.Handle(Unit.Default);
            SelectedFeed = null;
        }
    }
}