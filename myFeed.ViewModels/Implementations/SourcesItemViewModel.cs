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
            Url = new ObservableProperty<string>(entity.Uri);
            Name = new ObservableProperty<string>(new Uri(entity.Uri).Host);
            Notify = new ObservableProperty<bool>(entity.Notify);
            CopyLink = new ObservableCommand(() => platformService.CopyTextToClipboard(entity.Uri));
            OpenInBrowser = new ObservableCommand(async () =>
            {
                if (!Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute)) return;
                var uri = new Uri(entity.Uri);
                var plainUri = new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host));
                await platformService.LaunchUri(plainUri);
            });
            DeleteSource = new ObservableCommand(async () =>
            {
                parentViewModel.Items.Remove(this);
                await sourcesRepository.RemoveSourceAsync(entity.Category, entity);
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
        public ObservableCommand DeleteSource { get; }

        /// <summary>
        /// Opens the website in edge.
        /// </summary>
        public ObservableCommand OpenInBrowser { get; }

        /// <summary>
        /// Copies link location.
        /// </summary>
        public ObservableCommand CopyLink { get; }
    }
}