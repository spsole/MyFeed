using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using myFeed.Extensions;
using myFeed.Extensions.ViewModels;
using myFeed.FeedModels.Models;

namespace myFeed.Feed
{
    /// <summary>
    /// Represents feed item view model.
    /// </summary>
    public class FeedItemViewModel : ViewModelBase
    {
        private readonly FeedItemModel _model;
        public FeedItemViewModel(FeedItemModel model, bool read, bool favorite) =>
            (_model, IsRead.Value, IsFavorite.Value) = (model, read, favorite);

        #region Properties

        /// <summary>
        /// Published date.
        /// </summary>
        public DateTime PublishedDate => DateTime.Parse(_model.PublishedDate);

        /// <summary>
        /// Article title.
        /// </summary>
        public string Title => _model.Title;

        /// <summary>
        /// Source feed title.
        /// </summary>
        public string FeedTitle => _model.FeedTitle;

        /// <summary>
        /// Is this Uri well formed or not.
        /// </summary>
        public bool HasImage => 
            Uri.IsWellFormedUriString(_model.ImageUri, UriKind.Absolute) &&
            Settings.SettingsManager.GetInstance().GetSettings().DownloadImages;

        /// <summary>
        /// Preview image.
        /// </summary>
        public BitmapImage Image => 
            HasImage && 
            Settings.SettingsManager.GetInstance().GetSettings().DownloadImages
            ? new BitmapImage(new Uri(_model.ImageUri))
            {
                DecodePixelWidth = 100,
                DecodePixelType = DecodePixelType.Logical
            }
            : new BitmapImage();

        /// <summary>
        /// Human-readable date.
        /// </summary>
        public string HumanifiedDate => 
            (DateTime.Now.Date == PublishedDate.Date) ? 
            PublishedDate.ToString("HH:mm") :
            PublishedDate.ToString("MM.dd");

        /// <summary>
        /// Read state opacity.
        /// </summary>
        public float ReadStateOpacity => 
            IsRead.Value ? 0.5f : 1.0f;

        /// <summary>
        /// Is article read or not?
        /// </summary>
        public ObservableProperty<bool> IsRead { get; } = 
            new ObservableProperty<bool>();

        /// <summary>
        /// Is article added to favorites or not?
        /// </summary>
        public ObservableProperty<bool> IsFavorite { get; } = 
            new ObservableProperty<bool>();

        #endregion

        #region Methods

        /// <summary>
        /// Opens in Edge.
        /// </summary>
        public async void OpenInEdge() => await Launcher.LaunchUriAsync(new Uri(_model.Uri));

        /// <summary>
        /// Marks item as read.
        /// </summary>
        public async void MarkAsRead()
        {
            await FeedManager.GetInstance().MarkArticleAsRead(_model);
            IsRead.Value = true;
            OnPropertyChanged(nameof(ReadStateOpacity));
        }

        /// <summary>
        /// Marks item as unread if it was read.
        /// </summary>
        public async void MarkAsUnread()
        {
            await FeedManager.GetInstance().MarkArticleAsUnread(_model);
            IsRead.Value = false;
            OnPropertyChanged(nameof(ReadStateOpacity));
        }

        /// <summary>
        /// Copies link location.
        /// </summary>
        public void CopyLink()
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_model.Uri);
            Clipboard.SetContent(dataPackage);
        }

        /// <summary>
        /// Adds item to favorites.
        /// </summary>
        public async void AddToFavorites() => IsFavorite.Value =
            await FeedManager.GetInstance().AddArticleToFavorites(_model);

        /// <summary>
        /// Shows share UI.
        /// </summary>
        public void Share()
        {
            var transferManager = DataTransferManager.GetForCurrentView();
            var transferHandler = new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>((s, a) =>
            {
                var request = a.Request;
                request.Data.Properties.Title = _model.Title;
                request.Data.Properties.Description = _model.Uri;
                request.Data.SetText(
                    string.Format(
                        "{0}\r\n{1}\r\nvia myFeed for Windows 10",
                        _model.Title,
                        _model.Uri));
            });
            transferManager.DataRequested += transferHandler;
            DataTransferManager.ShowShareUI();
        }

        /// <summary>
        /// Returns inner-stored model.
        /// </summary>
        public FeedItemModel GetModel() => _model;

        #endregion
    }
}
