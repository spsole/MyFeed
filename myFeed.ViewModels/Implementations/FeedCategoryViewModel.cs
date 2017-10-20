using System.Collections.ObjectModel;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedCategoryViewModel
    {
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
                (var errors, var articles) = await feedStoreService.LoadAsync(sources);
                Items.Clear();
                foreach (var article in articles)
                    Items.Add(factoryService.CreateInstance<
                        ArticleViewModel>(article));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Feed items received from fetcher.
        /// </summary>
        public ObservableCollection<ArticleViewModel> Items { get; }

        /// <summary>
        /// Indicates if fetcher is loading data right now.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Feed category title.
        /// </summary>
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Opens sources page as a proposal for user 
        /// to add new RSS channels into this category.
        /// </summary>
        public ObservableCommand OpenSources { get; }

        /// <summary>
        /// Fetches data for current feed.
        /// </summary>
        public ObservableCommand Fetch { get; }
    }
}