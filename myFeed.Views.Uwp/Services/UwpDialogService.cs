using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Services
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
            var resourceLoader = ResourceLoader.GetForViewIndependentUse();
            var inputDialog = new ContentDialog {Title = title};
            var contentBox = new TextBox {PlaceholderText = message};
            inputDialog.PrimaryButtonText = resourceLoader.GetString("Ok");
            inputDialog.SecondaryButtonText = resourceLoader.GetString("Cancel");
            inputDialog.Content = new StackPanel {Children = {contentBox}};
            var result = await inputDialog.ShowAsync();
            return result == ContentDialogResult.Primary ? contentBox.Text : string.Empty;
        }

        public async Task<object> ShowDialogForSelection(IEnumerable<object> items)
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse();
            var selectionDialog = new ContentDialog {Title = resourceLoader.GetString("AddIntoCategory")};
            var selectBox = new ComboBox {DisplayMemberPath = "Title", ItemsSource = items};
            selectionDialog.PrimaryButtonText = resourceLoader.GetString("Ok");
            selectionDialog.SecondaryButtonText = resourceLoader.GetString("Cancel");
            selectionDialog.Content = selectBox;
            var result = await selectionDialog.ShowAsync();
            return result != ContentDialogResult.Primary ? null : selectBox.SelectedItem;
        }
    }
}
