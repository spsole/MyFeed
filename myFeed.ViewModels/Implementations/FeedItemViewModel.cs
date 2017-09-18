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
            PublishedDate = ObservableProperty.Of(entity.PublishedDate);
            IsFavorite = ObservableProperty.Of(entity.Fave);
            Content = ObservableProperty.Of(entity.Content);
            Feed = ObservableProperty.Of(entity.FeedTitle);
            IsRead = ObservableProperty.Of(entity.Read);
            Title = ObservableProperty.Of(entity.Title);
            Image = ObservableProperty.Of(async () =>
            {
                var loadImages = await settingsService.Get<bool>("LoadImages");
                return loadImages ? entity.ImageUri : null;
            });
            Share = ActionCommand.Of(async () =>
            {
                var shareText = $"{entity.Title}\r\n{entity.Uri}";
                await platformService.Share(shareText);
            });
            CopyLink = ActionCommand.Of(async () =>
            {
                await platformService.CopyTextToClipboard(entity.Uri);
            });
            LaunchUri = ActionCommand.Of(async () =>
            {
                if (!Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute)) return;
                await platformService.LaunchUri(new Uri(entity.Uri));
            });
            MarkRead = ActionCommand.Of(async () =>
            {
                IsRead.Value = entity.Read = !IsRead.Value;
                await articlesRepository.UpdateAsync(entity);
            });
            MarkFavorite = ActionCommand.Of(async () =>
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