using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using myFeed.Extensions;
using myFeed.Extensions.ViewModels;

namespace myFeed.Settings
{
    /// <summary>
    /// Represents Settings page view model.
    /// </summary>
    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPageViewModel()
        {
            // Initialize ViewModels.
            var resourceLoader = new ResourceLoader();
            FontBox = new ComboBoxViewModel<string, int>(
                new Dictionary<string, int> {
                    { resourceLoader.GetString("FontSizeSmall"), 15 },
                    { resourceLoader.GetString("FontSizeMiddle"), 17 },
                    { resourceLoader.GetString("FontSizeLarge"), 19 }
                }, 0);
            NotificationsBox = new ComboBoxViewModel<string, uint>(
                new Dictionary<string, uint> {
                    { resourceLoader.GetString("Notify30min"), 30 },
                    { resourceLoader.GetString("Notify1hour"), 60 },
                    { resourceLoader.GetString("Notify3hours"), 180 },
                    { resourceLoader.GetString("NotifyDisabled"), 0 }
                }, 0);
            ThemeGroup = new RadioGroupViewModel<int>(new[] { 0, 1, 2 }, 0);
            ImagesSwitch = new SelectableProperty<bool>();

            // Load settings async.
            LoadSettingsAsync();
        }

        /// <summary>
        /// Loads settings async.
        /// </summary>
        public void LoadSettingsAsync()
        {
            // Read settings async.
            var manager = SettingsManager.GetInstance();
            var settings = manager.GetSettings();

            // Set actual values got from deserialized settings.
            NotificationsBox.SetSelectedItem( settings.NotificationServiceCheckTime );
            ThemeGroup.SetSelectedItem( settings.ApplicationTheme );
            FontBox.SetSelectedItem( settings.ArticleFontSize );
            ImagesSwitch.SetSelectedItem( settings.DownloadImages );

            // Subscribe to items changes and save settings when user makes decision.
            ImagesSwitch.SelectedValueChanged += (s, a) => manager.UpdateSettings(() => settings.DownloadImages = a);
            FontBox.SelectedValueChanged += (s, a) => manager.UpdateSettings(() => settings.ArticleFontSize = a);
            NotificationsBox.SelectedValueChanged += (s, a) =>
            {
                // Upd stngs nd rgstr ntfer.
                manager.UpdateSettings(() => settings.NotificationServiceCheckTime = a);
                BackgroundTasksManager.RegisterNotifier(a);
            };

            // Listen to event and save, propose restart.
            var resourceLoader = new ResourceLoader();
            ThemeGroup.SelectedValueChanged += async (s, a) =>
            {
                manager.UpdateSettings(() => settings.ApplicationTheme = a);
                var messageDialog = new MessageDialog(
                    resourceLoader.GetString("SettingsNeedRestartMessage"),
                    resourceLoader.GetString("SettingsNeedRestart")
                );
                messageDialog.Commands.Add(new UICommand(
                    resourceLoader.GetString("SettingsNeedRestartButton"),
                    command => Application.Current.Exit()
                ));
                messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Cancel")));
                await messageDialog.ShowAsync();
            };
        }

        #region Properties

        /// <summary>
        /// ComboBox with check time variants.
        /// </summary>
        public ComboBoxViewModel<string, uint> NotificationsBox { get; }

        /// <summary>
        /// Font size ComboBox view model.
        /// </summary>
        public ComboBoxViewModel<string, int> FontBox { get; }

        /// <summary>
        /// Represents two-way binding value of ToggleSwitch control.
        /// </summary>
        public SelectableProperty<bool> ImagesSwitch { get; }

        /// <summary>
        /// Radio group for requested theme view model.
        /// </summary>
        public RadioGroupViewModel<int> ThemeGroup { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Resets app's settings to defaults.
        /// </summary>
        public async void ResetSettings()
        {
            var resourceLoader = new ResourceLoader();
            var messageDialog = new MessageDialog(
                resourceLoader.GetString("ShowMessageAlert"),
                resourceLoader.GetString("ShowMessageTitle")
            );

            // Add commands.
            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Cancel")));
            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Delete"), async args =>
            {
                // Delete settings and sites.
                var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
                foreach (var file in files) await file.DeleteAsync(StorageDeleteOption.Default);

                // Delete favorites.
                var favorites = await ApplicationData.Current.LocalFolder.GetFolderAsync("favorites");
                await favorites.DeleteAsync(StorageDeleteOption.Default);

                // Exit.
                Application.Current.Exit();
            }));

            // Show dialog.
            await messageDialog.ShowAsync();
        }

        /// <summary>
        /// Opens credits page.
        /// </summary>
        public async void OpenCreditPage() => 
            await Launcher.LaunchUriAsync(
                new Uri("https://worldbeater.github.io"));

        /// <summary>
        /// Sends mail via built-in mail client.
        /// </summary>
        public async void SendMail()
        {
            var message = new EmailMessage();
            message.To.Add(new EmailRecipient("worldbeater-dev@yandex.ru"));
            message.Subject = "myFeed Feedback";
            await EmailManager.ShowComposeNewEmailAsync(message);
        }

        /// <summary>
        /// Sends WinStore feedback.
        /// </summary>
        public async void SendFeedback() => 
            await Launcher.LaunchUriAsync(
                new Uri("ms-windows-store://review/?ProductId=9nblggh4nw02"));

        #endregion
    }
}
