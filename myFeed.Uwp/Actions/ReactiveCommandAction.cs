using System.Reactive;
using System.Windows.Input;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace myFeed.Uwp.Actions
{
    public sealed class ReactiveCommandAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(ICommand), typeof(ReactiveCommandAction), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter), typeof(object), typeof(ReactiveCommandAction), new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            Command?.Execute(CommandParameter ?? Unit.Default);
            return null;
        }
    }
}
