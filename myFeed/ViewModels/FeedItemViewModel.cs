using System;
using System.Reactive;
using System.Reactive.Linq;
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
        [DoNotCheckEquality]
        private Article Article { get; set; }
        
        public Interaction<Unit, bool> CopyConfirm { get; }
        public ReactiveCommand<Unit, Unit> MarkFave { get; }
        public ReactiveCommand<Unit, Unit> MarkRead { get; }
        public ReactiveCommand<Unit, Unit> Launch { get; }
        public ReactiveCommand<Unit, Unit> Share { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }

        public DateTime Published => Article.PublishedDate;
        public string Content => Article.Content;
        public string Image => Article.ImageUri;
        public string Title => Article.Title;
        public string Feed => Article.FeedTitle;
        public bool Fave => Article.Fave;
        public bool Read => Article.Read;

        public FeedItemViewModel(
            Func<FeedItemViewModel, ArticleViewModel> factory,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IFavoriteManager favoriteManager,
            IPlatformService platformService,
            Article article)
        {
            Article = article;
            CopyConfirm = new Interaction<Unit, bool>();
            Copy = ReactiveCommand.CreateFromTask(async () =>
            {
                await platformService.CopyTextToClipboard(Article.Uri);
                await CopyConfirm.Handle(Unit.Default);
            });
            Launch = ReactiveCommand.CreateFromTask(
                () => platformService.LaunchUri(new Uri(Article.Uri)),
                this.WhenAnyValue(x => x.Article)
                    .Select(x => Uri.IsWellFormedUriString(x.Uri, UriKind.Absolute))
            );
            Share = ReactiveCommand.CreateFromTask(
                () => platformService.Share($"{Article.Title} {Article.Uri}")
            );
            Open = ReactiveCommand.CreateFromTask(async () =>
            {
                Article.Read = true;
                await categoryManager.UpdateArticleAsync(Article);
                await navigationService.Navigate<ArticleViewModel>(factory(this));
                Article = Article;
            });
            MarkRead = ReactiveCommand.CreateFromTask(async () =>
            {
                Article.Read = !Article.Read;
                await categoryManager.UpdateArticleAsync(Article = Article);
            });
            MarkFave = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!Fave) await favoriteManager.InsertAsync(Article = Article);
                else await favoriteManager.RemoveAsync(Article = Article);
            });
        }
    }
}