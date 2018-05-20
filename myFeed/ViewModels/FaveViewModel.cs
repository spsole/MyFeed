using System;
using System.Globalization;
using System.Linq;
using System.Reactive;
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
        private readonly Func<IGrouping<string, Article>, FaveGroupViewModel> _factory;
        private readonly IFavoriteManager _favoriteManager;
        private readonly ISettingManager _settingManager;

        public ReactiveList<FaveGroupViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> OrderByMonth { get; }
        public ReactiveCommand<Unit, Unit> OrderByDate { get; }
        public ReactiveCommand<Unit, Unit> OrderByFeed { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }
        public bool Images { get; private set; }

        public FaveViewModel(
            Func<IGrouping<string, Article>, FaveGroupViewModel> factory,
            IFavoriteManager favoriteManager, 
            ISettingManager settingManager)
        {
            _favoriteManager = favoriteManager;
            _settingManager = settingManager;
            _factory = factory;

            Items = new ReactiveList<FaveGroupViewModel> {ChangeTrackingEnabled = true};
            var month = CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern;
            var date = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

            Load = Filter(x => x.PublishedDate, x => x.ToString(date));
            OrderByDate = Filter(x => x.PublishedDate, x => x.ToString(date));
            OrderByMonth = Filter(x => x.PublishedDate, x => x.ToString(month));
            OrderByFeed = Filter(x => x.FeedTitle, x => x);
        }
        
        private ReactiveCommand<Unit, Unit> Filter<TProperty>(
            Func<Article, TProperty> sequenceOrderer,
            Func<TProperty, string> itemDisplay)
        {
            return ReactiveCommand.CreateFromTask(async () =>
            {
                IsLoading = true;
                var settings = await _settingManager.Read();
                var articles = await _favoriteManager.GetAll();
                var groupings = articles
                    .OrderByDescending(sequenceOrderer)
                    .GroupBy(x => itemDisplay(sequenceOrderer(x)))
                    .Select(_factory)
                    .ToList();
                await Task.Delay(300);
                Items.Clear();
                Items.AddRange(groupings);
                Images = settings.Images;
                IsEmpty = Items.Count == 0;
                IsLoading = false;
            },
            this.WhenAnyValue(x => x.IsLoading, loading => !loading));
        }
    }
}