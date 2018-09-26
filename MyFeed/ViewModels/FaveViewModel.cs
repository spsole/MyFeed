using System;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace MyFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FaveViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FaveViewModel : ISupportsActivation
    {
        private readonly Func<IGrouping<string, Article>, FaveGroupViewModel> _factory;
        private readonly INavigationService _navigationService;
        private readonly IFavoriteManager _favoriteManager;
        private readonly ISettingManager _settingManager;

        public ReactiveCommand<Unit, Unit> OrderByMonth { get; }
        public ReactiveCommand<Unit, Unit> OrderByDate { get; }
        public ReactiveCommand<Unit, Unit> OrderByFeed { get; }

        public ReactiveList<FaveGroupViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> ReadFeeds { get; }
        public ViewModelActivator Activator { get; }

        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }
        public bool Images { get; private set; }

        public FaveViewModel(
            Func<IGrouping<string, Article>, FaveGroupViewModel> factory,
            INavigationService navigationService,
            IFavoriteManager favoriteManager, 
            ISettingManager settingManager)
        {
            _navigationService = navigationService;
            _favoriteManager = favoriteManager;
            _settingManager = settingManager;
            _factory = factory;

            ReadFeeds = ReactiveCommand.CreateFromTask(_navigationService.Navigate<FeedViewModel>);
            Items = new ReactiveList<FaveGroupViewModel> { ChangeTrackingEnabled = true };

            var month = CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern;
            var date = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

            OrderByDate = CreateFilter(x => x.PublishedDate, x => x.ToString(date));
            OrderByMonth = CreateFilter(x => x.PublishedDate, x => x.ToString(month));
            OrderByFeed = CreateFilter(x => x.FeedTitle, x => x);

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) => 
                OrderByDate.Execute().Subscribe());
        }
        
        private ReactiveCommand<Unit, Unit> CreateFilter<TProperty>(
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