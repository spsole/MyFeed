using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using myFeed.Extensions;
using myFeed.FeedModels.Models.Feedly;
using myFeed.Search.Controls;

namespace myFeed.Search
{
    /// <summary>
    /// Represents search item view model.
    /// </summary>
    public class SearchItemViewModel : ViewModelBase
    {
        private readonly SearchItemModel _model;
        public SearchItemViewModel(SearchItemModel model) => _model = model;

        #region Properties

        /// <summary>
        /// Favicon.
        /// </summary>
        public BitmapImage Image => _model.IconUrl == null ? new BitmapImage() :
            new BitmapImage(new Uri(_model.IconUrl)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };

        /// <summary>
        /// Search result title.
        /// </summary>
        public string Title => _model.Title;

        /// <summary>
        /// Search result description.
        /// </summary>
        public string Description => _model.Description;

        /// <summary>
        /// Full website url.
        /// </summary>
        public string Url => _model.Website;

        #endregion

        #region Methods

        /// <summary>
        /// Returns feed url.
        /// </summary>
        public string GetFeedUrl() => _model.FeedId.Substring(5);

        /// <summary>
        /// Adds model to sources.
        /// </summary>
        public async void AddToSources() => 
            await new SearchAddDialog(
                new SearchAddDialogViewModel(
                    _model)).ShowAsync();

        /// <summary>
        /// Copies website link to clipboard.
        /// </summary>
        public void CopyLink()
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_model.Website);
            Clipboard.SetContent(dataPackage);
        }

        /// <summary>
        /// Opens link to website in default browser.
        /// </summary>
        public async void OpenInEdge()
        {
            if (Uri.IsWellFormedUriString(_model.Website, UriKind.Absolute))
                await Launcher.LaunchUriAsync(new Uri(_model.Website));
        }

        #endregion
    }
}
