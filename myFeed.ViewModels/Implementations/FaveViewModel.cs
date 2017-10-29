using System.Collections.ObjectModel;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Bindables;
using System.Linq;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FaveViewModel
    {
        public ObservableCollection<ArticleViewModel> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand Load { get; }
        public ObservableCommand OrderByDate { get; }
        public ObservableCommand OrderByName { get; }

        public FaveViewModel(
            IFavoritesRepository favoritesReposirory,
            IFactoryService factoryService)
        {
            (IsEmpty, IsLoading) = (false, true);
            Items = new ObservableCollection<ArticleViewModel>();
            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var articles = await favoritesReposirory.GetAllAsync();
                Items.Clear();
                foreach (var article in articles.OrderByDescending(i => i.PublishedDate))
                {
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
            OrderByDate = new ObservableCommand(() =>
            {
                IsLoading.Value = true;
                var articles = Items.OrderByDescending(i => i.PublishedDate.Value).ToList();
                Items.Clear(); foreach (var article in articles) Items.Add(article);
                IsLoading.Value = false;
            });
            OrderByName = new ObservableCommand(() =>
            {
                IsLoading.Value = true;
                var articles = Items.OrderBy(i => i.Title.Value).ToList();
                Items.Clear(); foreach (var article in articles) Items.Add(article);
                IsLoading.Value = false;
            });
        }
    }
}