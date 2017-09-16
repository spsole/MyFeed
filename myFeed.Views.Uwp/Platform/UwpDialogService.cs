using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using myFeed.Services.Abstractions;
using myFeed.Views.Uwp.Controls;

namespace myFeed.Views.Uwp.Platform
{
    public sealed class UwpDialogService : IDialogService
    {
        public async Task ShowDialog(string message, string title)
        {
            await new MessageDialog(message, title).ShowAsync();
        }

        public async Task<bool> ShowDialogForConfirmation(string message, string title)
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse();
            var messageDialog = new MessageDialog(message, title);
            var okString = resourceLoader.GetString("Ok");
            var cancelString = resourceLoader.GetString("Cancel");
            messageDialog.Commands.Add(new UICommand(okString, x => { }, true));
            messageDialog.Commands.Add(new UICommand(cancelString, x => { }, false));
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
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
    }
}
