using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;
using myFeed.Services.Models;

namespace myFeed.ViewModels.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(FeedCategoryViewModel))]
    public sealed class FeedCategoryViewModel
    {
        public ObservableCollection<ArticleViewModel> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }
        public ObservableProperty<string> Title { get; }

        public ObservableCommand OpenSources { get; }
        public ObservableCommand Fetch { get; }

        public FeedCategoryViewModel(
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            IFactoryService factoryService,
            Category category)
        {
            Title = category.Title;
            (IsLoading, IsEmpty) = (true, false);
            Items = new ObservableCollection<ArticleViewModel>();
            OpenSources = new ObservableCommand(navigationService.Navigate<ChannelsViewModel>);
            Fetch = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var sources = category.Channels;
                var response = await feedStoreService.LoadAsync(sources);
                Items.Clear();
                foreach (var article in response.Item2)
                    Items.Add(factoryService.CreateInstance<
                        ArticleViewModel>(article));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}