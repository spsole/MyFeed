using System.Collections.ObjectModel;
using System.Globalization;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Bindables;
using System.Linq;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FaveViewModel
    {
        public ObservableCollection<ObservableGrouping<string, ArticleViewModel>> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand OrderByDate { get; }
        public ObservableCommand OrderByFeed { get; }
        public ObservableCommand Load { get; }

        public FaveViewModel(
            IFavoritesRepository favoritesReposirory,
            IFactoryService factoryService)
        {
            (IsEmpty, IsLoading) = (false, true);
            var longDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            Items = new ObservableCollection<ObservableGrouping<string, ArticleViewModel>>();
            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var articles = await favoritesReposirory.GetAllAsync();
                Items.Clear();
                var groupings = articles
                    .Select(i => factoryService.CreateInstance<ArticleViewModel>(i))
                    .OrderByDescending(i => i.PublishedDate.Value)
                    .GroupBy(i => i.PublishedDate.Value.ToString(longDatePattern))
                    .Select(i => new ObservableGrouping<string, ArticleViewModel>(i))
                    .ToList();

                groupings.ForEach(Items.Add);
                foreach (var grouping in groupings)
                foreach (var viewModel in grouping)
                viewModel.IsFavorite.PropertyChanged += (o, args) => RemoveOrRestore(viewModel);

                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });
            OrderByDate = new ObservableCommand(() =>
            {
                IsLoading.Value = true;
                var groupings = Items
                    .SelectMany(i => i)
                    .OrderByDescending(i => i.PublishedDate.Value)
                    .GroupBy(i => i.PublishedDate.Value.ToString(longDatePattern))
                    .Select(i => new ObservableGrouping<string, ArticleViewModel>(i))
                    .ToList();

                Items.Clear();
                groupings.ForEach(Items.Add);
                IsLoading.Value = false;
            });
            OrderByFeed = new ObservableCommand(() =>
            {
                IsLoading.Value = true;
                var groupings = Items
                    .SelectMany(i => i)
                    .OrderBy(i => i.Feed.Value)
                    .GroupBy(i => i.Feed.Value.ToString())
                    .Select(i => new ObservableGrouping<string, ArticleViewModel>(i))
                    .ToList();

                Items.Clear();
                groupings.ForEach(Items.Add);
                IsLoading.Value = false;
            });
            void RemoveOrRestore(ArticleViewModel viewModel) 
            {
                if (!viewModel.IsFavorite.Value) 
                {   
                    var related = Items.First(i => i.Contains(viewModel));
                    related.Remove(viewModel);
                    if (related.Count == 0) Items.Remove(related);
                }
                else 
                {
                    const string restored = "*Restored";
                    var existing = Items.FirstOrDefault(i => i.Key == restored);
                    if (existing == null)Items.Add(new ObservableGrouping<
                        string, ArticleViewModel>(restored, new[] {viewModel}));
                    else existing.Add(viewModel);
                }
            }
        }
    }
}