using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Common;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(FeedCategoryViewModel))]
    public sealed class FeedCategoryViewModel
    {
        public ObservableCollection<ArticleViewModel> Items { get; }

        public ObservableProperty<string> Title { get; }
        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand OpenSources { get; }
        public ObservableCommand Fetch { get; }

        public FeedCategoryViewModel(
            INavigationService navigationService,
            IFeedStoreService feedStoreService,
            IStateContainer stateContainer,
            IFactoryService factoryService)
        {
            var category = stateContainer.Pop<Category>();
            (IsLoading, IsEmpty) = (true, false);
            Title = category.Title;
            
            Items = new ObservableCollection<ArticleViewModel>();
            OpenSources = new ObservableCommand(navigationService.Navigate<ChannelsViewModel>);
            Fetch = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var sources = category.Channels;
                var response = await feedStoreService.LoadAsync(sources);
                Items.Clear();
                foreach (var article in response)
                    Items.Add(factoryService.CreateInstance<
                        ArticleViewModel>(article));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}