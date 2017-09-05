using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using myFeed.Repositories.Abstractions;
using myFeed.Repositories.Entities.Local;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Platform {
    public sealed class UwpProvider : IPlatformProvider {
        public async Task LaunchUri(Uri uri) => await Launcher.LaunchUriAsync(uri);
        public Task Share(string content) => Task.Delay(1);
        public Task CopyTextToClipboard(string text) => Task.Delay(1);
        public Task RegisterBackgroundTask(int freq) => Task.Delay(1);
        public Task RegisterBanners(bool needBanners) => Task.Delay(1);
        public Task RegisterTheme(string theme) => Task.Delay(1);

        public async Task<Stream> PickFileForReadAsync() {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".opml");
            var file = await picker.PickSingleFileAsync();
            var stream = await file.OpenStreamForReadAsync();
            return stream;
        }

        public async Task<Stream> PickFileForWriteAsync() {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Opml", new List<string> { ".opml" });
            picker.SuggestedFileName = "Feeds";
            var file = await picker.PickSaveFileAsync();
            var stream = await file.OpenStreamForWriteAsync();
            return stream;
        }

        public async Task ShowDialog(string message, string title) {
            await new MessageDialog(message, title).ShowAsync();
        }

        public Task<bool> ShowDialogForConfirmation(string message, string title) {
            return Task.FromResult(true);
        }

        public Task<string> ShowDialogForResults(string message, string title) {
            return Task.FromResult("hey");
        }

        public Task<SourceCategoryEntity> ShowDialogForCategorySelection(ISourcesRepository sourcesRepository) {
            var e = new SourceCategoryEntity();
            return Task.FromResult(e);
        }
    }
}
