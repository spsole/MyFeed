using System;
using myFeed.Entities.Local;
using myFeed.Repositories.Abstractions;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class SourcesItemViewModel
    {
        public SourcesItemViewModel(
            SourceEntity entity,
            SourcesCategoryViewModel parentViewModel,
            ISourcesRepository sourcesRepository,
            IPlatformService platformService)
        {
            Url = ObservableProperty.Of(entity.Uri);
            Notify = ObservableProperty.Of(entity.Notify);
            Name = ObservableProperty.Of(new Uri(entity.Uri).Host);
            CopyLink = ActionCommand.Of(() => platformService.CopyTextToClipboard(entity.Uri));
            DeleteSource = ActionCommand.Of(async () =>
            {
                await sourcesRepository.RemoveSourceAsync(entity.Category, entity);
                parentViewModel.Items.Remove(this);
            });
            OpenInBrowser = ActionCommand.Of(async () =>
            {
                if (!Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute)) return;
                var uri = new Uri(entity.Uri);
                var plainUri = new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host));
                await platformService.LaunchUri(plainUri);
            });
            Notify.PropertyChanged += async (sender, args) =>
            {
                entity.Notify = Notify.Value;
                await sourcesRepository.UpdateAsync(entity.Category);
            };
        }

        /// <summary>
        /// Are notifications enabled or not?
        /// </summary>
        public ObservableProperty<bool> Notify { get; }

        /// <summary>
        /// Model url.
        /// </summary>
        public ObservableProperty<string> Url { get; }

        /// <summary>
        /// Website name.
        /// </summary>
        public ObservableProperty<string> Name { get; }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        public ActionCommand DeleteSource { get; }

        /// <summary>
        /// Opens the website in edge.
        /// </summary>
        public ActionCommand OpenInBrowser { get; }

        /// <summary>
        /// Copies link location.
        /// </summary>
        public ActionCommand CopyLink { get; }
    }
}