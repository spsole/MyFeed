using System;
using System.Threading.Tasks;
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
            Url = new Property<string>(entity.Uri);
            Name = new Property<string>(new Uri(entity.Uri).Host);
            Notify = new Property<bool>(entity.Notify);
            CopyLink = new Command(() => platformService.CopyTextToClipboard(entity.Uri));
            OpenInBrowser = new Command(async () =>
            {
                if (!Uri.IsWellFormedUriString(entity.Uri, UriKind.Absolute)) return;
                var uri = new Uri(entity.Uri);
                var plainUri = new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host));
                await platformService.LaunchUri(plainUri);
            });
            DeleteSource = new Command(async () =>
            {
                parentViewModel.Items.Remove(this);
                await Task.Run(() => sourcesRepository.RemoveSourceAsync(entity.Category, entity));
            });
            Notify.PropertyChanged += async (sender, args) =>
            {
                entity.Notify = Notify.Value;
                await Task.Run(() => sourcesRepository.UpdateAsync(entity.Category));
            };
        }

        /// <summary>
        /// Are notifications enabled or not?
        /// </summary>
        public Property<bool> Notify { get; }

        /// <summary>
        /// Model url.
        /// </summary>
        public Property<string> Url { get; }

        /// <summary>
        /// Website name.
        /// </summary>
        public Property<string> Name { get; }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        public Command DeleteSource { get; }

        /// <summary>
        /// Opens the website in edge.
        /// </summary>
        public Command OpenInBrowser { get; }

        /// <summary>
        /// Copies link location.
        /// </summary>
        public Command CopyLink { get; }
    }
}