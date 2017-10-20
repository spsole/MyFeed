using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedViewModel
    {
        public FeedViewModel(
            ICategoriesRepository categoriesRepository,
            INavigationService navigationService,
            IFactoryService factoryService)
        {
            IsEmpty = false;
            IsLoading = true;
            
            OpenSources = new ObservableCommand(navigationService.Navigate<ChannelsViewModel>);
            Items = new ObservableCollection<FeedCategoryViewModel>();
            Load = new ObservableCommand(async () =>
            {
                IsEmpty.Value = false;
                IsLoading.Value = true;
                Items.Clear();
                var categories = await categoriesRepository.GetAllAsync();
                foreach (var category in categories)
                    Items.Add(factoryService.CreateInstance<
                        FeedCategoryViewModel>(category));
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }

        /// <summary>
        /// Feed categories viewmodels.
        /// </summary>
        public ObservableCollection<FeedCategoryViewModel> Items { get; }

        /// <summary>
        /// Indicates if viewmodel is loading items from disk.
        /// </summary>
        public ObservableProperty<bool> IsLoading { get; }

        /// <summary>
        /// True if collection is empty.
        /// </summary>
        public ObservableProperty<bool> IsEmpty { get; }

        /// <summary>
        /// Opens sources page as a proposal for user 
        /// to add new RSS categories into myFeed.
        /// </summary>
        public ObservableCommand OpenSources { get; }

        /// <summary>
        /// Loads all feeds into items property.
        /// </summary>
        public ObservableCommand Load { get; }
    }
}