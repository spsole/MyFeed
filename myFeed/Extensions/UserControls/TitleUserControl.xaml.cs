using Windows.UI.Xaml;

namespace myFeed.Extensions.UserControls
{
    public sealed partial class TitleUserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TitleUserControl),
                new PropertyMetadata(null)
            );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(
                nameof(Count),
                typeof(string),
                typeof(TitleUserControl),
                new PropertyMetadata(null, OnCountPropertyChanged)
            );

        public string Count
        {
            get => (string)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        private static void OnCountPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) =>
            ((TitleUserControl)dependencyObject).Count =
            (string)dependencyPropertyChangedEventArgs.NewValue;

        public TitleUserControl() => InitializeComponent();
    }
}
