using System;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations {
    /// <summary>
    /// Represents feed item view model.
    /// </summary>
    public sealed class FeedItemViewModel {
        /// <summary>
        /// Instantiates ViewModel.
        /// </summary>
        public FeedItemViewModel(
            ArticleEntity entity,
            IPlatformProvider platformProvider,
            IArticlesRepository articlesRepository) {

            PublishedDate = new ReadOnlyProperty<DateTime>(entity.PublishedDate);
            IsFavorite = new ObservableProperty<bool>(entity.Fave);
            Feed = new ReadOnlyProperty<string>(entity.FeedTitle);
            Image = new ReadOnlyProperty<string>(entity.ImageUri);
            Title = new ReadOnlyProperty<string>(entity.Title);
            IsRead = new ObservableProperty<bool>(entity.Read);

            Share = new ActionCommand(() => platformProvider.Share($"{entity.Title}\r\n{entity.Uri}"));
            CopyLink = new ActionCommand(() => platformProvider.CopyTextToClipboard(entity.Uri));
            LaunchUri = new ActionCommand(async () => {
                if (Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute))
                    await platformProvider.LaunchUri(new Uri(entity.Uri));
            });
            MarkRead = new ActionCommand(async () => {
                IsRead.Value = entity.Read = !IsRead.Value;
                await articlesRepository.UpdateAsync(entity);
            });
            AddToFavorites = new ActionCommand(async () => {
                if (IsFavorite.Value) return;
                IsFavorite.Value = entity.Fave = true;
                await articlesRepository.UpdateAsync(entity);
            });
        }

        /// <summary>
        /// Is article added to favorites or not?
        /// </summary>
        public ObservableProperty<bool> IsFavorite { get; }

        /// <summary>
        /// Is article read or not?
        /// </summary>
        public ObservableProperty<bool> IsRead { get; }

        /// <summary>
        /// Human-readable date.
        /// </summary>
        public ReadOnlyProperty<DateTime> PublishedDate { get; }

        /// <summary>
        /// Image url.
        /// </summary>
        public ReadOnlyProperty<string> Image { get; }

        /// <summary>
        /// Article title.
        /// </summary>
        public ReadOnlyProperty<string> Title { get; }

        /// <summary>
        /// Source feed title.
        /// </summary>
        public ReadOnlyProperty<string> Feed { get; }

        /// <summary>
        /// Adds article to favorites.
        /// </summary>
        public ActionCommand AddToFavorites { get; }

        /// <summary>
        /// Opens article in web browser.
        /// </summary>
        public ActionCommand LaunchUri { get; }

        /// <summary>
        /// Marks article as read.
        /// </summary>
        public ActionCommand MarkRead { get; }

        /// <summary>
        /// Copies link to clipboard.
        /// </summary>
        public ActionCommand CopyLink { get; }

        /// <summary>
        /// Shows share UI for article.
        /// </summary>
        public ActionCommand Share { get; }
    }
}
