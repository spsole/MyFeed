using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
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
        
        public ReactiveCommand<Unit, IEnumerable<Article>> Fetch { get; }
        public IReactiveDerivedList<FeedItemViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> Modify { get; }
        
        public bool ShowRead { get; private set; } = true;
        public bool IsLoading { get; private set; } = true;
        public bool HasErrors { get; private set; }
        public bool IsEmpty { get; private set; }
        public string Title => _category.Title;

        public FeedGroupViewModel(
            Func<Article, FeedItemViewModel> factory,
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            ISettingManager settingManager,
            Category category)
        {
            _source = new ReactiveList<FeedItemViewModel> {ChangeTrackingEnabled = true};
            _navigationService = navigationService;
            _feedStoreService = feedStoreService;
            _settingManager = settingManager;
            _category = category;
            _factory = factory;
            
            Modify = ReactiveCommand.CreateFromTask(_navigationService.Navigate<ChannelViewModel>);
            Fetch = ReactiveCommand.CreateFromTask(() => _feedStoreService.Load(_category.Channels));
            Items = _source.CreateDerivedCollection(x => x, x => !(!ShowRead && x.Read));
            
            Fetch.Select(articles => articles.Select(_factory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(articles => _source.Clear())
                .Subscribe(_source.AddRange);
            Items.IsEmptyChanged
                .Subscribe(x => IsEmpty = x);
            
            Fetch.IsExecuting.Skip(1)
                .Subscribe(x => IsLoading = x);
            Fetch.IsExecuting
                .Where(executing => executing)
                .SelectMany(x => _settingManager.Read())
                .Select(settings => settings.Read)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ShowRead = x);
            
            Fetch.IsExecuting
                .Where(executing => executing)
                .Select(executing => false)
                .Subscribe(x => HasErrors = x);
            Fetch.ThrownExceptions
                .Select(exception => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HasErrors = x);
        }
    }
}