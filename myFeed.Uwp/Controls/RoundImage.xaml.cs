using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls; 

namespace myFeed.Uwp.Controls
{
    public sealed partial class RoundImage : UserControl
    {
        private static readonly Uri FallbackUri = new Uri("ms-appx:///Assets/StoreLogo.png");

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(Uri), typeof(RoundImage), new PropertyMetadata(FallbackUri,
                (o, args) => ((RoundImage)o).BitmapImage.UriSource = (Uri)args.NewValue));

        public Uri Source
        {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public RoundImage()
        {
            InitializeComponent();
            Source = FallbackUri;
        }
    }
}
