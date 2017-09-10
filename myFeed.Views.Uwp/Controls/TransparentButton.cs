using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Views.Uwp.Controls
{
    public sealed class TransparentButton : Button
    {
        public TransparentButton() => DefaultStyleKey = typeof(TransparentButton);

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(nameof(Symbol), typeof(Symbol),
                typeof(TransparentButton), new PropertyMetadata(Symbol.Emoji2));

        public Symbol Symbol
        {
            get => (Symbol) GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }
    }
}