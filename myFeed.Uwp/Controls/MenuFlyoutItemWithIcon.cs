using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Uwp.Controls
{
    public class MenuFlyoutItemWithIcon : MenuFlyoutItem
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(Symbol),
                typeof(MenuFlyoutItemWithIcon), new PropertyMetadata(null));

        public Symbol Icon
        {
            get => (Symbol)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
    }
}
