using System;
using System.Reactive;
using Windows.ApplicationModel.Resources;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using ReactiveUI;

namespace MyFeed.Uwp.Actions
{
    public sealed class ModalConfirmAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty InteractionProperty = DependencyProperty.Register(
            nameof(Interaction), typeof(Interaction<Unit, bool>), typeof(ModalConfirmAction), null);

        public Interaction<Unit, bool> Interaction
        {
            get => (Interaction<Unit, bool>)GetValue(InteractionProperty);
            set => SetValue(InteractionProperty, value);
        }
        
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(ModalConfirmAction), null);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(ModalConfirmAction), null);

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public object Execute(object sender, object arg) => Interaction?.RegisterHandler(async interaction =>
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
