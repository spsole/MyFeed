using System.Collections.ObjectModel;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
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
            IsLoading = true;
            IsEmpty = false;
            
            Items = new ObservableCollection<ArticleViewModel>();
            OpenSources = new ObservableCommand(navigationService.Navigate<ChannelsViewModel>);
            Fetch = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var sources = category.Channels;
                (var _, var articles) = await feedStoreService.LoadAsync(sources);
                Items.Clear();
                foreach (var article in articles)
                    Items.Add(factoryService.CreateInstance<
                        ArticleViewModel>(article));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}