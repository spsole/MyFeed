using System;
using myFeed.Entities.Feedly;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SearchItemViewModel
    {
        public SearchItemViewModel(
            SearchItemEntity entity,
            IDialogService dialogService,
            IPlatformService platformService,
            ISourcesRepository sourcesRepository)
        {
            Url = new Property<string>(entity.Website);
            Title = new Property<string>(entity.Title);
            ImageUri = new Property<string>(entity.IconUrl);
            Description = new Property<string>(entity.Description);
            FeedUrl = new Property<string>(entity.FeedId?.Substring(5));
            CopyLink = new Command(() => platformService.CopyTextToClipboard(entity.Website));
            OpenInEdge = new Command(async () =>
            {
                if (Uri.IsWellFormedUriString(entity.Website, UriKind.Absolute))
                    await platformService.LaunchUri(new Uri(entity.Website));
            });
            AddToSources = new Command(async () =>
            {
                if (!Uri.IsWellFormedUriString(FeedUrl.Value, UriKind.Absolute)) return;
                var categories = await sourcesRepository.GetAllAsync();
                var response = await dialogService.ShowDialogForSelection(categories);
                if (response is SourceCategoryEntity sourceCategoryEntity)
                {
                    var source = new SourceEntity {Notify = true, Uri = FeedUrl.Value};
                    await sourcesRepository.AddSourceAsync(sourceCategoryEntity, source);
                }
            });
        }

        /// <summary>
        /// Search result title.
        /// </summary>
        public Property<string> Title { get; }

        /// <summary>
        /// Favicon.
        /// </summary>
        public Property<string> ImageUri { get; }

        /// <summary>
        /// Search result description.
        /// </summary>
        public Property<string> Description { get; }

        /// <summary>
        /// Returns feed url.
        /// </summary>
        public Property<string> FeedUrl { get; }

        /// <summary>
        /// Full website url.
        /// </summary>
        public Property<string> Url { get; }

        /// <summary>
        /// Adds model to sources.
        /// </summary>
        public Command AddToSources { get; }

        /// <summary>
        /// Opens link to website in default browser.
        /// </summary>
        public Command OpenInEdge { get; }

        /// <summary>
        /// Copies website link to clipboard.
        /// </summary>
        public Command CopyLink { get; }
    }
}