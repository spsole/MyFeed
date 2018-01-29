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
        public ReactiveList<FeedItemViewModel> Items { get; }
        public ReactiveCommand Modify { get; }
        public ReactiveCommand Fetch { get; }

        public string Title { get; }
        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }

        public FeedGroupViewModel(
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            IFactoryService factoryService,
            Category category)
        {
            (IsLoading, Title) = (true, category.Title);
            Items = new ReactiveList<FeedItemViewModel>();
            Modify = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<ChannelViewModel>());
            Fetch = ReactiveCommand.CreateFromTask(async () =>
            {
                IsLoading = true;
                var response = await feedStoreService.LoadAsync(category.Channels);
                var factory = factoryService.Create<Func<Article, FeedItemViewModel>>();
                var viewModels = response.Select(x => factory(x));
                Items.Clear();
                Items.AddRange(viewModels);
                IsEmpty = Items.Count == 0;
                IsLoading = false;
            });
        }
    }
}