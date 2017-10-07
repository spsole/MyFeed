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
            IDialogService dialogService,
            ISettingsService settingsService,
            IPlatformService platformService,
            INavigationService navigationService,
            IArticlesRepository articlesRepository,
            ITranslationsService translationsService)
        {
            IsRead = new ObservableProperty<bool>(article.Read);
            Title = new ObservableProperty<string>(article.Title);
            IsFavorite = new ObservableProperty<bool>(article.Fave);
            Feed = new ObservableProperty<string>(article.FeedTitle);
            Content = new ObservableProperty<string>(article.Content);
            PublishedDate = new ObservableProperty<DateTime>(article.PublishedDate);
            Image = new ObservableProperty<string>(async () => await settingsService
                .Get<bool>("LoadImages") ? article.ImageUri : null);

            Open = new ObservableCommand(() => navigationService.Navigate(this));
            Share = new ObservableCommand(() => platformService.Share(string.Concat(article.Title,
                Environment.NewLine, article.Uri, Environment.NewLine, "via myFeed for Windows")));
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
                await articlesRepository.UpdateAsync(article);
            });
            MarkFavorite = new ObservableCommand(async () =>
            {
                IsFavorite.Value = article.Fave = !IsFavorite.Value;
                await articlesRepository.UpdateAsync(article);
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