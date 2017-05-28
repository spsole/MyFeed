using Windows.UI.Xaml;

namespace myFeed.Settings
{
    public sealed partial class SettingsPage
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SettingsPageViewModel),
                typeof(SettingsPage),
                new PropertyMetadata(null)
            );

        public SettingsPageViewModel ViewModel
        {
            get => (SettingsPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public SettingsPage()
        {
            ViewModel = new SettingsPageViewModel();
            InitializeComponent();
        }
    }
}
