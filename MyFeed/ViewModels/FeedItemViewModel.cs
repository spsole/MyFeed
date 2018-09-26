using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
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
    [ExportEx(typeof(FeedItemViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedItemViewModel : ISupportsActivation
    {
        private readonly INavigationService _navigationService;
        private readonly ICategoryManager _categoryManager;
        private readonly IFavoriteManager _favoriteManager;
        private readonly IPlatformService _platformService;
        private readonly ISettingManager _settingManager;
        private readonly Article _article;

        public ReactiveCommand<Unit, Unit> MarkFave { get; }
        public ReactiveCommand<Unit, Unit> MarkRead { get; }
        public ReactiveCommand<Unit, Unit> Launch { get; }
        public ReactiveCommand<Unit, Unit> Share { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }

        public Interaction<Unit, bool> Copied { get; }
        public ViewModelActivator Activator { get; }

        public DateTime Published => _article.PublishedDate;
        public string Image => Images ? _article.ImageUri : null;
        public string Content => _article.Content;
        public string Feed => _article.FeedTitle;
        public string Title => _article.Title;

        public double Font { get; private set; }
        public bool Images { get; private set; }
        public bool Fave { get; private set; }
        public bool Read { get; private set; }

        public FeedItemViewModel(
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IFavoriteManager favoriteManager,
            IPlatformService platformService,
            ISettingManager settingManager,
            Article article)
        {
            _navigationService = navigationService;
            _categoryManager = categoryManager;
            _favoriteManager = favoriteManager;
            _platformService = platformService;
            _settingManager = settingManager;
            _article = article;

            Share = ReactiveCommand.CreateFromTask(
                () => _platformService.Share($"{_article.Title} {_article.Uri}"));
            Open = ReactiveCommand.CreateFromTask(
                () => _navigationService.NavigateTo(this));

            Fave = _article.Fave;
            Read = _article.Read;
            Open.Subscribe(x => Read = true);
            this.ObservableForProperty(x => x.Read)
                .Select(property => property.Value)
                .Do(read => _article.Read = read)
                .Select(read => _article)
                .Select(_categoryManager.Update)
                .SelectMany(task => task.ToObservable())
                .Subscribe();

            Launch = ReactiveCommand.CreateFromTask(
                () => _platformService.LaunchUri(new Uri(_article.Uri)),
                Observable.Return(Uri.IsWellFormedUriString(_article.Uri, UriKind.Absolute)));

            Copy = ReactiveCommand.CreateFromTask(
                () => _platformService.CopyTextToClipboard(_article.Uri),
                Observable.Return(!string.IsNullOrWhiteSpace(_article.Uri)));
            
            MarkFave = ReactiveCommand.CreateFromTask(DoMarkFave);
            MarkRead = ReactiveCommand.Create(() => { Read = !Read; });
            Copied = new Interaction<Unit, bool>();
            Copy.ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(Copied.Handle)
                .Subscribe();

            Activator = new ViewModelActivator();
            this.WhenActivated(async (CompositeDisposable disposables) =>
            {
                var settings = await _settingManager.Read();
                Images = settings.Images;
                Font = settings.Font;
            });
        }

        private async Task DoMarkFave()
        {
            if (Fave) await _favoriteManager.Remove(_article);
            else await _favoriteManager.Insert(_article);
            Fave = _article.Fave;
        }
    }
}