using System;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class ArticleViewModel
    {
        public ArticleViewModel(
            ArticleEntity article, 
            ISettingsService settingsService,
            IPlatformService platformService,
            INavigationService navigationService,
            IArticlesRepository articlesRepository)
        {
            IsRead = new Property<bool>(article.Read);
            Title = new Property<string>(article.Title);
            IsFavorite = new Property<bool>(article.Fave);
            Feed = new Property<string>(article.FeedTitle);
            Content = new Property<string>(article.Content);
            PublishedDate = new Property<DateTime>(article.PublishedDate);
            Image = new Property<string>(async () => await settingsService
                .Get<bool>("LoadImages") ? article.ImageUri : null);

            Open = new Command(() => navigationService.Navigate(this));
            Share = new Command(() => platformService.Share($"{article.Title}\r\n{article.Uri}"));
            CopyLink = new Command(() => platformService.CopyTextToClipboard(article.Uri));
            LaunchUri = new Command(async () =>
            {
                if (Uri.IsWellFormedUriString(article.Uri, UriKind.Absolute)) 
                    await platformService.LaunchUri(new Uri(article.Uri));
            });
            MarkRead = new Command(async () =>
            {
                IsRead.Value = article.Read = !IsRead.Value;
                await articlesRepository.UpdateAsync(article);
            });
            MarkFavorite = new Command(async () =>
            {
                IsFavorite.Value = article.Fave = !IsFavorite.Value;
                await articlesRepository.UpdateAsync(article);
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

        /// <summary>
        /// Opens this FeedItem.
        /// </summary>
        public Command Open { get; }
    }
}