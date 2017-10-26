using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Services.Platform;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpDialogService : IDialogService
    {
        private readonly ITranslationsService _translationsService;

        public UwpDialogService(ITranslationsService translationsService) => _translationsService = translationsService;

        public async Task ShowDialog(string message, string title) => await new MessageDialog(message, title).ShowAsync();

        public async Task<bool> ShowDialogForConfirmation(string message, string title)
        {
            var messageDialog = new MessageDialog(message, title);
            var okString = _translationsService.Resolve("Ok");
            var cancelString = _translationsService.Resolve("Cancel");
            messageDialog.Commands.Add(new UICommand(okString, x => { }, true));
            messageDialog.Commands.Add(new UICommand(cancelString, x => { }, false));
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public async Task<string> ShowDialogForResults(string message, string title)
        {
            var contentBox = new TextBox { PlaceholderText = message, Margin = new Thickness(0, 18, 0, 0) };
            var inputDialog = new ContentDialog
            {
                Title = title,
                PrimaryButtonText = _translationsService.Resolve("Ok"),
                SecondaryButtonText = _translationsService.Resolve("Cancel"),
                Content = new StackPanel {Children = {contentBox}}
            };
            var result = await inputDialog.ShowAsync();
            return result == ContentDialogResult.Primary ? contentBox.Text : string.Empty;
        }

        public async Task<object> ShowDialogForSelection(IEnumerable<object> items)
        {
            var selectBox = new ComboBox
            {
                DisplayMemberPath = "Title",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 18, 0, 0),
                ItemsSource = items
            };
            var selectionDialog = new ContentDialog
            {
                Title = _translationsService.Resolve("AddIntoCategory"),
                PrimaryButtonText = _translationsService.Resolve("Ok"),
                SecondaryButtonText = _translationsService.Resolve("Cancel"),
                Content = new StackPanel {Children = {selectBox}}
            };
            selectBox.SelectedIndex = selectBox.Items?.Any() == true ? 0 : -1;
            var result = await selectionDialog.ShowAsync();
            return result != ContentDialogResult.Primary ? null : selectBox.SelectedItem;
        }
    }
}
