using System;
using System.Linq;
using System.Reactive.Disposables;
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
            ISettingManager settingManager,
            IFactoryService factoryService,
            Category category)
        {
            IsLoading = true;
            Title = category.Title;
            var disposable = new CompositeDisposable();
            Items = new ReactiveList<FeedItemViewModel>();
            Modify = ReactiveCommand.CreateFromTask(() => navigationService.Navigate<ChannelViewModel>());
            Fetch = ReactiveCommand.CreateFromTask(async () =>
            {
                IsLoading = true;
                disposable.Dispose();
                disposable = new CompositeDisposable();
                var settings = await settingManager.Read();
                var response = await feedStoreService.LoadAsync(category.Channels);
                var factory = factoryService.Create<Func<Article, FeedItemViewModel>>();
                var viewModels = response.Select(x => factory(x)).ToList();
                Items.Clear();
                Items.AddRange(viewModels.Where(x => !(!settings.Read && x.Read)));
                viewModels.ForEach((item, index) => item.WhenAnyValue(x => x.Read).Subscribe(read =>
                {
                    if (settings.Read) return;
                    var contains = Items.Contains(item);
                    if (!read && !contains) Items.Insert(index, item);
                    else if (read && contains) Items.Remove(item);
                    IsEmpty = Items.Count == 0;
                })
                .DisposeWith(disposable));
                IsEmpty = Items.Count == 0;
                IsLoading = false;
            });
        }
    }
}