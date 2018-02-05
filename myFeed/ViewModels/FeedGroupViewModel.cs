using System;
using System.Linq;
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
        public ReactiveCommand Modify { get; }
        public ReactiveCommand Fetch { get; }

        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }
        public string Title { get; }

        public FeedGroupViewModel(
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            ISettingManager settingManager,
            IFactoryService factoryService,
            Category category)
        {
            var showRead = true;
            (IsLoading, Title) = (true, category.Title);
            var cache = new ReactiveList<FeedItemViewModel>() {ChangeTrackingEnabled = true};
            Items = cache.CreateDerivedCollection(x => x, x => !(!showRead && x.Read));
            Items.CountChanged.Subscribe(x => IsEmpty = x == 0);
            Modify = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<ChannelViewModel>());
            Fetch = ReactiveCommand.CreateFromTask(async () =>
            {
                IsLoading = true;
                var settings = await settingManager.Read();
                var response = await feedStoreService.LoadAsync(category.Channels);
                var factory = factoryService.Create<Func<Article, FeedItemViewModel>>();
                var viewModels = response.Select(x => factory(x)).ToList();
                showRead = settings.Read;
                cache.Clear();
                cache.AddRange(viewModels);
                IsLoading = false;
            });
        }
    }
}