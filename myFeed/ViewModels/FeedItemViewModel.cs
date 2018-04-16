using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FeedItemViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedItemViewModel
    {
        public Interaction<Unit, bool> CopyConfirm { get; }
        public ReactiveCommand<Unit, Unit> MarkFave { get; }
        public ReactiveCommand<Unit, Unit> MarkRead { get; }
        public ReactiveCommand<Unit, Unit> Launch { get; }
        public ReactiveCommand<Unit, Unit> Share { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }

        public bool Fave { get; private set; }
        public bool Read { get; private set; }
        public DateTime Published { get; }
        public string Content { get; }
        public string Image { get; }
        public string Title { get; }
        public string Feed { get; }

        public FeedItemViewModel(
            Func<FeedItemViewModel, ArticleViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IFavoriteManager favoriteManager,
            IPlatformService platformService,
            Article article)
        {
            Published = article.PublishedDate;
            Content = article.Content;
            Image = article.ImageUri;
            Feed = article.FeedTitle;
            Title = article.Title;
            Fave = article.Fave;
            Read = article.Read;

            Launch = ReactiveCommand.CreateFromTask(
                () => platformService.LaunchUri(new Uri(article.Uri)),
                Observable.Return(Uri.IsWellFormedUriString(article.Uri, UriKind.Absolute))
            );
            Share = ReactiveCommand.CreateFromTask(
                () => platformService.Share($"{article.Title} {article.Uri}")
            );
            Open = ReactiveCommand.CreateFromTask(
                () => navigationService.Navigate<ArticleViewModel>(factory(this))
            );

            MarkRead = ReactiveCommand.Create(() => { Read = !Read; });
            Open.Subscribe(x => Read = true);
            this.WhenAnyValue(x => x.Read)
                .Skip(1).Do(x => article.Read = x)
                .SelectMany(x => categoryManager
                    .UpdateArticleAsync(article)
                    .ToObservable())
                .Subscribe();
            
            CopyConfirm = new Interaction<Unit, bool>();
            Copy = ReactiveCommand.CreateFromTask(async () =>
            {
                await platformService.CopyTextToClipboard(article.Uri);
                await CopyConfirm.Handle(Unit.Default);
            });
            MarkFave = ReactiveCommand.CreateFromTask(async () =>
            {
                if (Fave) await favoriteManager.RemoveAsync(article);
                else await favoriteManager.InsertAsync(article);
                Fave = article.Fave;
            });
        }
    }
}