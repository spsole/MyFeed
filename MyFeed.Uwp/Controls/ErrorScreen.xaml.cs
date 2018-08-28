using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyFeed.Uwp.Controls
{
    public sealed partial class ErrorScreen : UserControl
    {
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty
            .Register(nameof(IsVisible), typeof(bool), typeof(ErrorScreen), null);
        
        public bool IsVisible
        {
            get => (bool) GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }
        
        public static readonly DependencyProperty SymbolProperty = DependencyProperty
            .Register(nameof(Symbol), typeof(Symbol), typeof(ErrorScreen), null);

        public Symbol Symbol
        {
            get => (Symbol) GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }
        
        public static readonly DependencyProperty TitleProperty = DependencyProperty
            .Register(nameof(Title), typeof(string), typeof(ErrorScreen), null);
        
        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty
            .Register(nameof(Description), typeof(string), typeof(ErrorScreen), null);
        
        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        
        public static readonly DependencyProperty CommandProperty = DependencyProperty
            .Register(nameof(Command), typeof(ICommand), typeof(ErrorScreen), null);
        
        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        
        public static readonly DependencyProperty ActionProperty = DependencyProperty
            .Register(nameof(Action), typeof(string), typeof(ErrorScreen), null);

        public string Action
        {
            get => (string) GetValue(ActionProperty);
            set => SetValue(ActionProperty, value);
        }

        public ErrorScreen() => InitializeComponent();
    }
}
