using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myFeed.Extensions.Controls
{
    public class MenuFlyoutItemWithIcon : MenuFlyoutItem
    {
        public MenuFlyoutItemWithIcon() => DefaultStyleKey = typeof(MenuFlyoutItemWithIcon);

        public static readonly DependencyProperty IconProperty =
           DependencyProperty.Register(
               nameof(Icon),
               typeof(Symbol),
               typeof(MenuFlyoutItemWithIcon),
               new PropertyMetadata(null)
            );

        public Symbol Icon
        {
            get => (Symbol)GetValue(IconProperty); 
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty FontIconProperty =
           DependencyProperty.Register(
               nameof(FontIcon),
               typeof(string),
               typeof(MenuFlyoutItemWithIcon),
               new PropertyMetadata(null)
            );

        public string FontIcon
        {
            get => (string)GetValue(FontIconProperty);
            set => SetValue(FontIconProperty, value);
        }
    }
}
