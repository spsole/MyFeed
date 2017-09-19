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
            IsRead = new Property<bool>(entity.Read);
            Title = new Property<string>(entity.Title);
            IsFavorite = new Property<bool>(entity.Fave);
            Feed = new Property<string>(entity.FeedTitle);
            Content = new Property<string>(entity.Content);
            PublishedDate = new Property<DateTime>(entity.PublishedDate);
            Image = new Property<string>(async () => await settingsService
                .Get<bool>("LoadImages") 
                ? entity.ImageUri
                : string.Empty);

            Share = new Command(() => platformService.Share($"{entity.Title}\r\n{entity.Uri}"));
            CopyLink = new Command(() => platformService.CopyTextToClipboard(entity.Uri));
            LaunchUri = new Command(async () =>
            {
                if (Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute)) 
                    await platformService.LaunchUri(new Uri(entity.Uri));
            });
            MarkRead = new Command(async () =>
            {
                IsRead.Value = entity.Read = !IsRead.Value;
                await articlesRepository.UpdateAsync(entity);
            });
            MarkFavorite = new Command(async () =>
            {
                IsFavorite.Value = entity.Fave = !IsFavorite.Value;
                await articlesRepository.UpdateAsync(entity);
            });
        }
        
        /// <summary>
        /// Human-readable date.
        /// </summary>
        public Property<DateTime> PublishedDate { get; }

        /// <summary>
        /// Is article added to favorites or not?
        /// </summary>
        public Property<bool> IsFavorite { get; }
        
        /// <summary>
        /// Is article read or not?
        /// </summary>
        public Property<bool> IsRead { get; }

        /// <summary>
        /// Contains article content.
        /// </summary>
        public Property<string> Content { get; }

        /// <summary>
        /// Image url.
        /// </summary>
        public Property<string> Image { get; }

        /// <summary>
        /// Article title.
        /// </summary>
        public Property<string> Title { get; }

        /// <summary>
        /// Source feed title.
        /// </summary>
        public Property<string> Feed { get; }

        /// <summary>
        /// Adds article to favorites.
        /// </summary>
        public Command MarkFavorite { get; }

        /// <summary>
        /// Opens article in web browser.
        /// </summary>
        public Command LaunchUri { get; }

        /// <summary>
        /// Marks article as read.
        /// </summary>
        public Command MarkRead { get; }

        /// <summary>
        /// Copies link to clipboard.
        /// </summary>
        public Command CopyLink { get; }

        /// <summary>
        /// Shows share UI for article.
        /// </summary>
        public Command Share { get; }
    }
}