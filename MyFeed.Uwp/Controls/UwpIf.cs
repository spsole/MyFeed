using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyFeed.Uwp.Controls
{
    public class UwpIf : ContentControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(bool), typeof(UwpIf), new PropertyMetadata(false, OnValuePropertyChanged)
        );

        public bool Value
        {
            get => (bool)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private object DettachedContent { get; set; }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Value) return;
            DettachedContent = Content;
            Content = null;
        }

        private static void OnValuePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var control = (UwpIf)o;
            control.Content = (bool) args.NewValue ? control.DettachedContent : null;
        }
    }
}
