using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using DryIocAttributes;
using MyFeed.Platform;

namespace MyFeed.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IFilePickerService))]
    public sealed class UwpFilePickerService : IFilePickerService
    {
        public async Task<Stream> PickFileForReadAsync()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".opml");
            var file = await picker.PickSingleFileAsync();
            if (file == null) return null;
            var stream = await file.OpenStreamForReadAsync();
            return stream;
        }

        public async Task<Stream> PickFileForWriteAsync()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Opml", new List<string> { ".opml" });
            picker.SuggestedFileName = "Feeds";
            var file = await picker.PickSaveFileAsync();
            if (file == null) return null;
            await FileIO.WriteTextAsync(file, string.Empty);
            var stream = await file.OpenStreamForWriteAsync();
            return stream;
        }
    }
}
