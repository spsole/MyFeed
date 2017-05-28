using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Extensions.Controls
{
    /// <summary>
    /// Button that is transparent and contains large SymbolIcon inside.
    /// </summary>
    public sealed class TransparentButton : Button
    {
        /// <summary>
        /// Instantiates new TransparentButton control.
        /// </summary>
        public TransparentButton() => DefaultStyleKey = typeof(TransparentButton);

        /// <summary>
        /// Identifies the <see cref="Symbol"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(
                nameof(Symbol),
                typeof(Symbol),
                typeof(TransparentButton),
                new PropertyMetadata(Symbol.Emoji2)
            );

        /// <summary>
        /// Gets or sets a value indicating whether the Header of the control.
        /// </summary>
        public Symbol Symbol
        {
            get => (Symbol)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }
    }
}
