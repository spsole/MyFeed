using System;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Feedly;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations {
    /// <summary>
    /// Represents search item view model.
    /// </summary>
    public sealed class SearchItemViewModel {
        /// <summary>
        /// Instantiates new ViewModel.
        /// </summary>
        public SearchItemViewModel(
            SearchItemEntity entity,
            IPlatformProvider platformProvider,
            ISourcesRepository sourcesRepository) {

            Url = new ReadOnlyProperty<string>(entity.Website);
            Title = new ReadOnlyProperty<string>(entity.Title);
            ImageUri = new ReadOnlyProperty<string>(entity.IconUrl);
            Description = new ReadOnlyProperty<string>(entity.Description);
            FeedUrl = new ReadOnlyProperty<string>(entity.FeedId?.Substring(5));

            CopyLink = new ActionCommand(() => platformProvider.CopyTextToClipboard(entity.Website));
            OpenInEdge = new ActionCommand(async () => {
                if (Uri.IsWellFormedUriString(entity.Website, UriKind.Absolute))
                    await platformProvider.LaunchUri(new Uri(entity.Website));
            });
            AddToSources = new ActionCommand(async () => {
                if (!Uri.IsWellFormedUriString(FeedUrl.Value, UriKind.Absolute)) return;
                var categories = await sourcesRepository.GetAllOrderedAsync();
                var response = await platformProvider.ShowDialogForSelection(categories);
                if (response is SourceCategoryEntity sourceCategoryEntity) {
                    sourceCategoryEntity.Sources.Add(new SourceEntity {
                        Notify = true, Uri = FeedUrl.Value
                    });
                    await sourcesRepository.UpdateAsync(sourceCategoryEntity);
                }
            });
        }

        /// <summary>
        /// Search result title.
        /// </summary>
        public ReadOnlyProperty<string> Title { get; }

        /// <summary>
        /// Favicon.
        /// </summary>
        public ReadOnlyProperty<string> ImageUri { get; }

        /// <summary>
        /// Search result description.
        /// </summary>
        public ReadOnlyProperty<string> Description { get; }

        /// <summary>
        /// Returns feed url.
        /// </summary>
        public ReadOnlyProperty<string> FeedUrl { get; }

        /// <summary>
        /// Full website url.
        /// </summary>
        public ReadOnlyProperty<string> Url { get; }

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
