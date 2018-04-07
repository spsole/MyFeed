using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ReactiveUI;

namespace myFeed.Uwp.Actions
{
    public sealed class ModalSelectAction : ModalActionBase
    {
        public static readonly DependencyProperty InteractionProperty = DependencyProperty.Register(
            nameof(Interaction), typeof(Interaction<IList<string>, int>), typeof(ModalSelectAction), null);

        public Interaction<IList<string>, int> Interaction
        {
            get => (Interaction<IList<string>, int>) GetValue(InteractionProperty);
            set => SetValue(InteractionProperty, value);
        }

        public override object Execute(object sender, object arg) => Interaction?.RegisterHandler(async interaction =>
        {
            var loader = ResourceLoader.GetForViewIndependentUse();
            var selectBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 18, 0, 0),
                ItemsSource = interaction.Input
            };
            var selectionDialog = new ContentDialog
            {
                Title = Title, Content = new StackPanel { Children = { selectBox } },
                SecondaryButtonText = loader.GetString("Cancel"),
                PrimaryButtonText = loader.GetString("Ok")
            };
            selectBox.SelectedIndex = selectBox.Items?.Any() == true ? 0 : -1;
            var result = await selectionDialog.ShowAsync();
            var index = result != ContentDialogResult.Primary ? -1 : selectBox.SelectedIndex;
            interaction.SetOutput(index);
        });
    }
}
