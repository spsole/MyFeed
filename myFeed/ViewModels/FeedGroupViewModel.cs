using System;
using System.Collections.Generic;
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
        public IReactiveDerivedList<FeedItemViewModel> Items { get; }
        public Interaction<Exception, bool> Error { get; }
        public ReactiveCommand<Unit, Unit> Modify { get; }
        public ReactiveCommand<Unit, Unit> Fetch { get; }

        public bool IsLoading { get; private set; } = true;
        public bool ShowRead { get; private set; } = true;
        public bool IsEmpty { get; private set; }
        public string Title { get; }

        public FeedGroupViewModel(
            Func<Article, FeedItemViewModel> factory,
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            ISettingManager settingManager,
            Category category)
        {
            Title = category.Title;
            var cache = new ReactiveList<FeedItemViewModel> {ChangeTrackingEnabled = true};
            Items = cache.CreateDerivedCollection(x => x, x => !(!ShowRead && x.Read));            
            Modify = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<ChannelViewModel>());
            Fetch = ReactiveCommand.CreateFromTask(
                () => DoFetch(factory, cache, feedStoreService, settingManager, category.Channels)
            );

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
        
        private async Task DoFetch(
            Func<Article, FeedItemViewModel> factory,
            IReactiveList<FeedItemViewModel> cache,
            IFeedStoreService feedStoreService,
            ISettingManager settingManager,
            IEnumerable<Channel> channels)
        {
            var settings = await settingManager.Read();
            var response = await feedStoreService.Load(channels);
            var viewModels = response.Select(factory).ToList();
            ShowRead = settings.Read;
            cache.Clear();
            cache.AddRange(viewModels);
        }
    }
}