using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using ReactiveUI;

namespace myFeed.Uwp.Actions
{
    public sealed class ModalErrorAction : ModalActionBase
    {
        public static readonly DependencyProperty InteractionProperty = DependencyProperty.Register(
            nameof(Interaction), typeof(Interaction<Exception, bool>), typeof(ModalErrorAction), null);

        public Interaction<Exception, bool> Interaction
        {
            get => (Interaction<Exception, bool>)GetValue(InteractionProperty);
            set => SetValue(InteractionProperty, value);
        }

        public override object Execute(object sender, object parameter) => Interaction?.RegisterHandler(async i =>
        {
            var message = $"{Message} {i.Input.Message}";
            var loader = ResourceLoader.GetForViewIndependentUse();
            var dialog = new MessageDialog(message, Title);
            dialog.Commands.Add(new UICommand(loader.GetString("Ok"), x => { }, true));
            dialog.Commands.Add(new UICommand(loader.GetString("Cancel"), x => { }, false));
            var result = await dialog.ShowAsync();
            i.SetOutput((bool) result.Id);
        });
    }
}
