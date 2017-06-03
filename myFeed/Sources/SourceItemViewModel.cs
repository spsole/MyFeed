using System;
using Windows.ApplicationModel.DataTransfer;
using myFeed.Extensions;
using myFeed.Extensions.ViewModels;
using myFeed.FeedModels.Models;

namespace myFeed.Sources
{
    /// <summary>
    /// Single source view model.
    /// </summary>
    public class SourceItemViewModel : ViewModelBase
    {
        private readonly SourceItemModel _model;
        private readonly SourceCategoryViewModel _parent;
        public SourceItemViewModel(SourceItemModel model, SourceCategoryViewModel parent)
        {
            (_model, _parent) = (model, parent);
            NotificationsEnabled.Value = _model.Notify;
            NotificationsEnabled.SelectedValueChanged += (s, a) => ToggleNotificationsAsync(a);
        }

        #region Properties

        /// <summary>
        /// Are notifications enabled or not?
        /// </summary>
        public SelectableProperty<bool> NotificationsEnabled { get; } = 
            new SelectableProperty<bool>();

        /// <summary>
        /// Model url.
        /// </summary>
        public string Url => _model.Uri;

        /// <summary>
        /// Website name.
        /// </summary>
        public string Name => new Uri(_model.Uri).Host.Capitalize();

        #endregion

        #region Methods 

        /// <summary>
        /// Responsible for saving notifications.
        /// </summary>
        private async void ToggleNotificationsAsync(bool value)
        {
            // To nothing if current value and value are the same.
            if (value == _model.Notify) return;

            // Update settings.
            var success = await SourcesManager
                .GetInstance()
                .ToggleNotifications(value, _parent.Title.Value, _model.Uri);

            // Update UI if success.
            if (success) _model.Notify = value;
        }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        public async void DeleteSource()
        {
            // Delete and update UI.
            var success = await SourcesManager
                .GetInstance()
                .DeleteSource(_model, _parent.Title.Value);
            if (success) _parent.Items.Remove(this);
        }

        /// <summary>
        /// Opens the website in edge.
        /// </summary>
        public async void OpenInEdge()
        {
            if (!Uri.IsWellFormedUriString(_model.Uri, UriKind.Absolute)) return;
            var uri = new Uri(_model.Uri);
            var plainFormat = string.Format("{0}://{1}", uri.Scheme, uri.Host);
            var plainUri = new Uri(plainFormat);
            await Windows.System.Launcher.LaunchUriAsync(plainUri);
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

        #endregion
    }
}
