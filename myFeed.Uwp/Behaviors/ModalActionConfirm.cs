using System;
using System.Reactive;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using ReactiveUI;

namespace myFeed.Uwp.Behaviors
{
    public sealed class ModalActionConfirm : ModalActionBase
    {
        public static readonly DependencyProperty InteractionProperty = DependencyProperty.Register(
            nameof(Interaction), typeof(Interaction<Unit, bool>), typeof(ModalActionConfirm), null);

        public Interaction<Unit, bool> Interaction
        {
            get => (Interaction<Unit, bool>)GetValue(InteractionProperty);
            set => SetValue(InteractionProperty, value);
        }

        public override object Execute(object sender, object arg) => Interaction?.RegisterHandler(async interaction =>
        {
            var loader = ResourceLoader.GetForViewIndependentUse();
            var dialog = new MessageDialog(Message, Title);
            dialog.Commands.Add(new UICommand(loader.GetString("Ok"), x => { }, true));
            dialog.Commands.Add(new UICommand(loader.GetString("Cancel"), x => { }, false));
            var result = await dialog.ShowAsync();
            interaction.SetOutput((bool)result.Id);
        });
    }
}
