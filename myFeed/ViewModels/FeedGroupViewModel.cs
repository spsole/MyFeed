using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(FeedGroupViewModel))]
    public sealed class FeedGroupViewModel
    {
        private readonly Func<Article, FeedItemViewModel> _factory;
        private readonly IReactiveList<FeedItemViewModel> _source;
        private readonly INavigationService _navigationService;
        private readonly IFeedStoreService _feedStoreService;
        private readonly ISettingManager _settingManager;
        private readonly Category _category;

        public IReactiveDerivedList<FeedItemViewModel> Items { get; }
        public Interaction<Exception, bool> Error { get; }
        public ReactiveCommand<Unit, Unit> Modify { get; }
        public ReactiveCommand<Unit, Unit> Fetch { get; }

        public bool IsLoading { get; private set; } = true;
        public bool ShowRead { get; private set; } = true;
        public bool IsEmpty { get; private set; }
        public string Title => _category.Title;

        public FeedGroupViewModel(
            Func<Article, FeedItemViewModel> factory,
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            ISettingManager settingManager,
            Category category)
        {
            _navigationService = navigationService;
            _feedStoreService = feedStoreService;
            _settingManager = settingManager;
            _category = category;
            _factory = factory;

            _source = new ReactiveList<FeedItemViewModel> {ChangeTrackingEnabled = true};
            Items = _source.CreateDerivedCollection(x => x, x => !(!ShowRead && x.Read));            
            Modify = ReactiveCommand.CreateFromTask(
                () => _navigationService.Navigate<ChannelViewModel>()
            );

            Fetch = ReactiveCommand.CreateFromTask(DoFetch);
            Fetch.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Items.CountChanged
                .Select(count => count == 0)
                .Subscribe(x => IsEmpty = x);

            Error = new Interaction<Exception, bool>();
            Fetch.ThrownExceptions
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(error => Error.Handle(error))
                .Where(retryRequested => retryRequested)
                .Select(x => Unit.Default)
                .InvokeCommand(Fetch);
        }
        
        private async Task DoFetch()
        {
            var settings = await _settingManager.Read();
            var response = await _feedStoreService.Load(_category.Channels);
            var viewModels = response.Select(_factory).ToList();
            ShowRead = settings.Read;
            _source.Clear();
            _source.AddRange(viewModels);
        }
    }
}