using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Bindables;
using DryIocAttributes;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace myFeed.ViewModels.Implementations
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(FaveViewModel))]
    public sealed class FaveViewModel
    {
        public ObservableCollection<ObservableGrouping<string, ArticleViewModel>> Items { get; }

        public ObservableProperty<bool> IsLoading { get; }
        public ObservableProperty<bool> IsEmpty { get; }

        public ObservableCommand OrderByMonth { get; }
        public ObservableCommand OrderByDate { get; }
        public ObservableCommand OrderByFeed { get; }
        public ObservableCommand Load { get; }

        public FaveViewModel(
            IFavoriteStoreService favoriteStoreService,
            IFactoryService factoryService)
        {
            (IsEmpty, IsLoading) = (false, true);
            var month = CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern;
            var date = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

            Items = new ObservableCollection<ObservableGrouping<string, ArticleViewModel>>();
            OrderByDate = new ObservableCommand(() => OrderBy(
                i => i.PublishedDate.Value, 
                i => i.PublishedDate.Value.ToString(date)));
            OrderByMonth = new ObservableCommand(() => OrderBy(
                i => i.PublishedDate.Value.Month, 
                i => i.PublishedDate.Value.ToString(month)));
            OrderByFeed = new ObservableCommand(() => OrderBy(
                i => i.Feed.Value, 
                i => i.Feed.Value.ToString()));

            Load = new ObservableCommand(async () =>
            {
                IsLoading.Value = true;
                var articles = await favoriteStoreService.GetAllAsync();
                Items.Clear();
                var groupings = articles
                    .Select(i => factoryService.CreateInstance<ArticleViewModel>(i))
                    .OrderByDescending(i => i.PublishedDate.Value)
                    .GroupBy(i => i.PublishedDate.Value.ToString(date))
                    .Select(i => new ObservableGrouping<string, ArticleViewModel>(i))
                    .ToList();

                groupings.ForEach(Items.Add);
                foreach (var grouping in groupings)
                foreach (var viewModel in grouping)
                    viewModel.IsFavorite.PropertyChanged += (o, args) => 
                        RemoveOrRestore(viewModel);

                IsEmpty.Value = Items.Count == 0;
                IsLoading.Value = false;
            });

            Task OrderBy<T>(Func<ArticleViewModel, T> order, Func<ArticleViewModel, string> display)
            {
                IsLoading.Value = true;
                var groupings = Items.SelectMany(i => i).OrderByDescending(order).GroupBy(display)
                    .Select(i => new ObservableGrouping<string, ArticleViewModel>(i)).ToList();
                Items.Clear();
                groupings.ForEach(Items.Add);
                IsLoading.Value = false;
                return Task.CompletedTask;
            }
            
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
                    if (existing == null) Items.Add(new ObservableGrouping<string,
                        ArticleViewModel>(restored, new[] {viewModel}));
                    else existing.Add(viewModel);
                }
            }
        }
    }
}