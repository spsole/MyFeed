using Windows.UI.Xaml;

namespace myFeed.Views.Uwp.Controls {
    public sealed partial class TitleBlock {
        public TitleBlock() => InitializeComponent();

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string),
                typeof(TitleBlock), new PropertyMetadata(null));

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value.ToUpperInvariant());
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(nameof(Count), typeof(int),
                typeof(TitleBlock), new PropertyMetadata(null, (o, args) => 
                    ((TitleBlock)o).CountBlock.Text = args.NewValue.ToString()));

        public int Count {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }
    }
}
