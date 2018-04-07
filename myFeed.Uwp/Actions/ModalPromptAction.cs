using System;
using System.Reactive;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ReactiveUI;

namespace myFeed.Uwp.Actions
{
    public sealed class ModalPromptAction : ModalActionBase
    {
        public static readonly DependencyProperty InteractionProperty = DependencyProperty.Register(
            nameof(Interaction), typeof(Interaction<Unit, string>), typeof(ModalPromptAction), null);

        public Interaction<Unit, string> Interaction
        {
            get => (Interaction<Unit, string>)GetValue(InteractionProperty);
            set => SetValue(InteractionProperty, value);
        }

        public override object Execute(object sender, object arg) => Interaction?.RegisterHandler(async interaction =>
        {
            var loader = ResourceLoader.GetForViewIndependentUse();
            var content = new TextBox {PlaceholderText = Message, Margin = new Thickness(0, 18, 0, 0)};
            var dialog = new ContentDialog
            {
                Title = Title, Content = new StackPanel {Children = {content}},
                SecondaryButtonText = loader.GetString("Cancel"),
                PrimaryButtonText = loader.GetString("Ok")
            };
            var result = await dialog.ShowAsync();
            var confirmed = result == ContentDialogResult.Primary ? content.Text : string.Empty;
            interaction.SetOutput(confirmed);
        });
    }
}
