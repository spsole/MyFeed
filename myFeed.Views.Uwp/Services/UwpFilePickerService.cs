using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpFilePickerService : IFilePickerService
    {
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
            picker.FileTypeChoices.Add("Opml", new List<string> { ".opml" });
            picker.SuggestedFileName = "Feeds";
            var file = await picker.PickSaveFileAsync();
            var stream = await file.OpenStreamForWriteAsync();
            return stream;
        }
    }
}
