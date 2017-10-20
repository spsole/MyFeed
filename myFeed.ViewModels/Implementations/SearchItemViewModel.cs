using System;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Models;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SearchItemViewModel
    {
        public SearchItemViewModel(
            ICategoriesRepository categoriesRepository,
            IPlatformService platformService,
            IDialogService dialogService,
            FeedlyItem feedlyItem)
        {
            Url = feedlyItem.Website;
            Title = feedlyItem.Title;
            ImageUri = feedlyItem.IconUrl;
            Description = feedlyItem.Description;
            FeedUrl = feedlyItem.FeedId?.Substring(5);
            
            CopyLink = new ObservableCommand(() => platformService.CopyTextToClipboard(feedlyItem.Website));
            OpenInEdge = new ObservableCommand(async () =>
            {
                if (Uri.IsWellFormedUriString(feedlyItem.Website, UriKind.Absolute))
                    await platformService.LaunchUri(new Uri(feedlyItem.Website));
            });
            AddToSources = new ObservableCommand(async () =>
            {
                if (!Uri.IsWellFormedUriString(FeedUrl.Value, UriKind.Absolute)) return;
                var categories = await categoriesRepository.GetAllAsync();
                var response = await dialogService.ShowDialogForSelection(categories);
                if (response is Category category)
                {
                    var source = new Channel {Notify = true, Uri = FeedUrl.Value};
                    await categoriesRepository.InsertChannelAsync(category, source);
                }
            });
        }

        /// <summary>
        /// Search result title.
        /// </summary>
        public ObservableProperty<string> Title { get; }

        /// <summary>
        /// Favicon.
        /// </summary>
        public ObservableProperty<string> ImageUri { get; }

        /// <summary>
        /// Search result description.
        /// </summary>
        public ObservableProperty<string> Description { get; }

        /// <summary>
        /// Returns feed url.
        /// </summary>
        public ObservableProperty<string> FeedUrl { get; }

        /// <summary>
        /// Full website url.
        /// </summary>
        public ObservableProperty<string> Url { get; }

        /// <summary>
        /// Adds model to sources.
        /// </summary>
        public ObservableCommand AddToSources { get; }

        /// <summary>
        /// Opens link to website in default browser.
        /// </summary>
        public ObservableCommand OpenInEdge { get; }

        /// <summary>
        /// Copies website link to clipboard.
        /// </summary>
        public ObservableCommand CopyLink { get; }
    }
}