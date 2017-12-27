﻿using System;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Common;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(ArticleViewModel))]
    public sealed class ArticleViewModel
    {
        public ObservableProperty<DateTime> PublishedDate { get; }
        public ObservableProperty<string> Content { get; }
        public ObservableProperty<string> Image { get; }
        public ObservableProperty<string> Feed { get; }
        public ObservableProperty<string> Title { get; }
        public ObservableProperty<bool> IsFavorite { get; }
        public ObservableProperty<bool> IsRead { get; }

        public ObservableCommand MarkFavorite { get; }
        public ObservableCommand LaunchUri { get; }
        public ObservableCommand MarkRead { get; }
        public ObservableCommand CopyLink { get; }
        public ObservableCommand Share { get; }
        public ObservableCommand Open { get; }

        public ArticleViewModel(
            ITranslationService translationsService,
            INavigationService navigationService,
            ICategoryManager categoryManager,
            IFavoriteManager favoriteManager,
            IPlatformService platformService,
            ISettingManager settingManager,
            IStateContainer stateContainer,
            IDialogService dialogService)
        {
            var article = stateContainer.Pop<Article>();
            PublishedDate = article.PublishedDate;
            Content = article.Content;
            IsFavorite = article.Fave;
            Feed = article.FeedTitle;
            Title = article.Title; 
            IsRead = article.Read;
            
            Open = new ObservableCommand(() => navigationService.Navigate<ArticleViewModel>(this));
            Image = new ObservableProperty<string>(async () =>
            {
                var shouldLoadImages = await settingManager.GetAsync<bool>("LoadImages");
                return shouldLoadImages ? article.ImageUri : null;
            });
            Share = new ObservableCommand(() =>
            {
                var shareMessage = string.Concat(article.Title, Environment.NewLine,
                    article.Uri, Environment.NewLine, "via myFeed for Windows Universal");
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
                await categoryManager.UpdateArticleAsync(article);
            });
            MarkFavorite = new ObservableCommand(async () =>
            {
                IsFavorite.Value = !IsFavorite.Value;
                if (IsFavorite.Value) await favoriteManager.InsertAsync(article);
                else await favoriteManager.RemoveAsync(article);
            });
        }
    }
}