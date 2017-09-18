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
            Url = ObservableProperty.Of(entity.Website);
            Title = ObservableProperty.Of(entity.Title);
            ImageUri = ObservableProperty.Of(entity.IconUrl);
            Description = ObservableProperty.Of(entity.Description);
            FeedUrl = ObservableProperty.Of(entity.FeedId?.Substring(5));
            CopyLink = ActionCommand.Of(async () =>
            {
                await platformService.CopyTextToClipboard(entity.Website);
            });
            OpenInEdge = ActionCommand.Of(async () =>
            {
                if (Uri.IsWellFormedUriString(entity.Website, UriKind.Absolute))
                    await platformService.LaunchUri(new Uri(entity.Website));
            });
            AddToSources = ActionCommand.Of(async () =>
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
        public ActionCommand AddToSources { get; }

        /// <summary>
        /// Opens link to website in default browser.
        /// </summary>
        public ActionCommand OpenInEdge { get; }

        /// <summary>
        /// Copies website link to clipboard.
        /// </summary>
        public ActionCommand CopyLink { get; }
    }
}