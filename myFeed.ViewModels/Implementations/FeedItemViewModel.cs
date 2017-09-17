using System;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class FeedItemViewModel
    {
        public FeedItemViewModel(
            ArticleEntity entity,
            ISettingsService settingsService,
            IPlatformService platformService,
            IArticlesRepository articlesRepository)
        {
            IsRead = new ObservableProperty<bool>(entity.Read);
            IsFavorite = new ObservableProperty<bool>(entity.Fave);
            PublishedDate = new ObservableProperty<DateTime>(entity.PublishedDate);
            Content = new ObservableProperty<string>(entity.Content);
            Feed = new ObservableProperty<string>(entity.FeedTitle);
            Title = new ObservableProperty<string>(entity.Title);
            Image = new ObservableProperty<string>(async () =>
            {
                var loadImages = await settingsService.Get<bool>("LoadImages");
                return loadImages ? entity.ImageUri : null;
            });
            Share = new ActionCommand(async () =>
            {
                var shareText = $"{entity.Title}\r\n{entity.Uri}";
                await platformService.Share(shareText);
            });
            CopyLink = new ActionCommand(async () =>
            {
                await platformService.CopyTextToClipboard(entity.Uri);
            });
            LaunchUri = new ActionCommand(async () =>
            {
                if (!Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute)) return;
                await platformService.LaunchUri(new Uri(entity.Uri));
            });
            MarkRead = new ActionCommand(async () =>
            {
                IsRead.Value = entity.Read = !IsRead.Value;
                await articlesRepository.UpdateAsync(entity);
            });
            MarkFavorite = new ActionCommand(async () =>
            {
                IsFavorite.Value = entity.Fave = !IsFavorite.Value;
                await articlesRepository.UpdateAsync(entity);
            });
        }
        
        /// <summary>
        /// Is article read or not?
        /// </summary>
        public ObservableProperty<bool> IsRead { get; }

        /// <summary>
        /// Is article added to favorites or not?
        /// </summary>
        public ObservableProperty<bool> IsFavorite { get; }

        /// <summary>
        /// Human-readable date.
        /// </summary>
        public ObservableProperty<DateTime> PublishedDate { get; }

        /// <summary>
        /// Contains article content.
        /// </summary>
        public ObservableProperty<string> Content { get; }

        /// <summary>
        /// Image url.
        /// </summary>
        public ObservableProperty<string> Image { get; }

        /// <summary>
        /// Article title.
        /// </summary>
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Source feed title.
        /// </summary>
        public ObservableProperty<string> Feed { get; }

        /// <summary>
        /// Adds article to favorites.
        /// </summary>
        public ActionCommand MarkFavorite { get; }

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