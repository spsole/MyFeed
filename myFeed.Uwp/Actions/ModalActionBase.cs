using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace myFeed.Uwp.Actions
{
    public abstract class ModalActionBase : DependencyObject, IAction
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty
            .Register(nameof(Title), typeof(string), typeof(ModalActionBase), null);

        public static readonly DependencyProperty MessageProperty = DependencyProperty
            .Register(nameof(Message), typeof(string), typeof(ModalActionBase), null);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public abstract object Execute(object sender, object parameter);
    }
}
