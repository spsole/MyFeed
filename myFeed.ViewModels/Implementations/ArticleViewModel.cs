using System;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Abstractions;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class ArticleViewModel
    {
        public ArticleViewModel(
            ICategoriesRepository categoriesRepository,
            ITranslationsService translationsService,
            INavigationService navigationService,
            IFavoritesService favoritesService,
            ISettingsService settingsService,
            IPlatformService platformService,
            IDialogService dialogService,
            Article article)
        {
            Title = article.Title; 
            Feed = article.FeedTitle;
            Content = article.Content;
            PublishedDate = article.PublishedDate;
            IsFavorite = article.Fave;
            IsRead = article.Read;
            
            Open = new ObservableCommand(() => navigationService.Navigate(this));
            Image = new ObservableProperty<string>(async () =>
            {
                var shouldLoadImages = await settingsService.GetAsync<bool>("LoadImages");
                return shouldLoadImages ? article.ImageUri : null;
            });
            Share = new ObservableCommand(() =>
            {
                var shareMessage = string.Concat(
                    article.Title, Environment.NewLine,
                    article.Uri, Environment.NewLine,
                    "via myFeed for Windows Universal");
                return platformService.Share(shareMessage);
            });
            CopyLink = new ObservableCommand(async () => 
            {
                await platformService.CopyTextToClipboard(article.Uri);
                await dialogService.ShowDialog(
                    translationsService.Resolve("CopyLinkSuccess"),
                    translationsService.Resolve("SettingsNotification"));
            });
            LaunchUri = new ObservableCommand(async () =>
            {
                if (Uri.IsWellFormedUriString(article.Uri, UriKind.Absolute)) 
                    await platformService.LaunchUri(new Uri(article.Uri));
            });
            MarkRead = new ObservableCommand(async () =>
            {
                IsRead.Value = article.Read = !IsRead.Value;
                await categoriesRepository.UpdateArticleAsync(article);
            });
            MarkFavorite = new ObservableCommand(async () =>
            {
                IsFavorite.Value = !IsFavorite.Value;
                if (IsFavorite.Value) await favoritesService.Insert(article);
                else await favoritesService.Remove(article);
            });
        }
        
        /// <summary>
        /// Human-readable date.
        /// </summary>
        public ObservableProperty<DateTime> PublishedDate { get; }

        /// <summary>
        /// Is article added to favorites or not?
        /// </summary>
        public ObservableProperty<bool> IsFavorite { get; }
        
        /// <summary>
        /// Is article read or not?
        /// </summary>
        public ObservableProperty<bool> IsRead { get; }

        /// <summary>
        /// Contains article content.
        /// </summary>
        public ObservableProperty<string> Content { get; }

        /// <summary>
        /// Image url.
        /// </summary>
        public ObservableProperty<string> Image { get; }

        /// <summary>
        /// Source feed title.
        /// </summary>
        public ObservableProperty<string> Feed { get; }

        /// <summary>
        /// Article title.
        /// </summary>
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Adds article to favorites.
        /// </summary>
        public ObservableCommand MarkFavorite { get; }

        /// <summary>
        /// Opens article in web browser.
        /// </summary>
        public ObservableCommand LaunchUri { get; }

        /// <summary>
        /// Marks article as read.
        /// </summary>
        public ObservableCommand MarkRead { get; }

        /// <summary>
        /// Copies link to clipboard.
        /// </summary>
        public ObservableCommand CopyLink { get; }

        /// <summary>
        /// Shows share UI for article.
        /// </summary>
        public ObservableCommand Share { get; }

        /// <summary>
        /// Opens this FeedItem.
        /// </summary>
        public ObservableCommand Open { get; }
    }
}