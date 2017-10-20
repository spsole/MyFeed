using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FaveViewModel
    {
        public ObservableCollection<ArticleViewModel> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand Load { get; }

        public FaveViewModel(
            IFavoritesRepository favoritesReposirory,
            IFactoryService factoryService)
        {
            IsEmpty = false;
            IsLoading = true;
            
            Items = new ObservableCollection<ArticleViewModel>();
            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var articles = await favoritesReposirory.GetAllAsync();
                Items.Clear();
                foreach (var article in articles)
                {
                    if (!article.Fave) continue;
                    var viewModel = factoryService.CreateInstance<ArticleViewModel>(article);
                    viewModel.IsFavorite.PropertyChanged += (o, args) =>
                    {
                        if (viewModel.IsFavorite.Value) Items.Add(viewModel);
                        else Items.Remove(viewModel);
                        IsEmpty.Value = Items.Count == 0;
                    };
                    Items.Add(viewModel);
                }
                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
        }
    }
}