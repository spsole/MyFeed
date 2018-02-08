using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FaveViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FaveViewModel
    {
        public ReactiveList<FaveGroupViewModel> Items { get; }
        public ReactiveCommand OrderByMonth { get; }
        public ReactiveCommand OrderByDate { get; }
        public ReactiveCommand OrderByFeed { get; }
        public ReactiveCommand Load { get; }

        public bool IsLoading { get; private set; }
        public bool IsEmpty { get; private set; }
        public bool Images { get; private set; }

        public FaveViewModel(
            IFavoriteManager favoriteManager,
            ISettingManager settingManager,
            IFactoryService factoryService)
        {
            IsLoading = true;
            Items = new ReactiveList<FaveGroupViewModel>();
            var month = CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern;
            var date = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

            Load = LoadAs(x => x.PublishedDate, x => x.ToString(date));
            OrderByMonth = LoadAs(x => x.PublishedDate, x => x.ToString(month));
            OrderByDate = LoadAs(x => x.PublishedDate, x => x.ToString(date));
            OrderByFeed = LoadAs(x => x.FeedTitle, x => x);
            
            ReactiveCommand LoadAs<T>(Func<Article, T> order, Func<T, string> display)
            {
                return ReactiveCommand.CreateFromTask(async () =>
                {
                    IsLoading = true;
                    var settings = await settingManager.Read();
                    var articles = await favoriteManager.GetAllAsync();
                    var factory = factoryService.Create<Func<
                        IGrouping<string, Article>,
                        FaveGroupViewModel>>();
                    var groupings = articles
                        .OrderByDescending(order)
                        .GroupBy(x => display(order(x)))
                        .Select(x => factory(x))
                        .ToList();
                    await Task.Delay(300);
                    Items.Clear();
                    Items.AddRange(groupings);
                    Images = settings.Images;
                    IsEmpty = Items.Count == 0;
                    IsLoading = false;
                },
                this.WhenAnyValue(x => x.IsLoading).Select(x => !x));
            }
        }
    }
}