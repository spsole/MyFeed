using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace myFeed.Extensions.UserControls
{
    public sealed partial class EllipticImageUserControl
    {
        /// <summary>
        /// Image source. May be represented as a BitmapImage.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(ImageBrush),
                new PropertyMetadata(null, ImageSourcePropertyChanged)
            );

        /// <summary>
        /// Image source brush.
        /// </summary>
        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o">Sender</param>
        /// <param name="args">Args</param>
        private static void ImageSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args) => 
            (o as EllipticImageUserControl).AvatarBrush.ImageSource = args.NewValue as ImageSource;

        /// <summary>
        /// Animate the image.
        /// </summary>
        private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (AvatarEllipse.Opacity == 0) AvatarEllipse.FadeIn();
        }

        public EllipticImageUserControl() => InitializeComponent();
    }
}
