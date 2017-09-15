using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using myFeed.Services.Abstractions;
using myFeed.Views.Uwp.Controls;

namespace myFeed.Views.Uwp.Platform
{
    public sealed class UwpProvider : IPlatformService
    {
        public async Task LaunchUri(Uri uri) => await Launcher.LaunchUriAsync(uri);

        public async Task ShowDialog(string msg, string title) => await new MessageDialog(msg, title).ShowAsync();

        public Task Share(string content)
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (sender, args) =>
            {
                var request = args.Request;
                request.Data.SetText(content);
                request.Data.Properties.Title = "myFeed";
            };
            DataTransferManager.ShowShareUI();
            return Task.CompletedTask;
        }

        public Task CopyTextToClipboard(string text)
        {
            var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            return Task.CompletedTask;
        }

        public async Task<Stream> PickFileForReadAsync()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".opml");
            var file = await picker.PickSingleFileAsync();
            var stream = await file.OpenStreamForReadAsync();
            return stream;
        }

        public async Task<Stream> PickFileForWriteAsync()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Opml", new List<string> {".opml"});
            picker.SuggestedFileName = "Feeds";
            var file = await picker.PickSaveFileAsync();
            var stream = await file.OpenStreamForWriteAsync();
            return stream;
        }

        public async Task<bool> ShowDialogForConfirmation(string message, string title)
        {
            var resourceLoader = new ResourceLoader();
            var messageDialog = new MessageDialog(message, title);
            var okString = resourceLoader.GetString("Ok");
            var cancelString = resourceLoader.GetString("Cancel");
            messageDialog.Commands.Add(new UICommand(okString, x => { }, true));
            messageDialog.Commands.Add(new UICommand(cancelString, x => { }, false));
            var result = await messageDialog.ShowAsync();
            return (bool) result.Id;
        }

        public async Task<string> ShowDialogForResults(string message, string title)
        {
            var inputDialog = new InputDialog(message, title);
            var result = await inputDialog.ShowAsync();
            return result == ContentDialogResult.Primary ? inputDialog.Value : string.Empty;
        }

        public async Task<object> ShowDialogForSelection(IEnumerable<object> items)
        {
            var selectionDialog = new SelectCategoryDialog(items, "YOBA");
            var result = await selectionDialog.ShowAsync();
            return result != ContentDialogResult.Primary ? null : selectionDialog.Value;
        }

        public Task RegisterBackgroundTask(int freq) => Task.Delay(1);
        public Task RegisterBanners(bool needBanners) => Task.Delay(1);
        public Task RegisterTheme(string theme) => Task.Delay(1);
    }
}