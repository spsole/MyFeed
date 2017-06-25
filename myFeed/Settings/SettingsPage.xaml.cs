namespace myFeed.Settings
{
    public sealed partial class SettingsPage
    {
        public SettingsPageViewModel ViewModel => DataContext as SettingsPageViewModel;
        
        public SettingsPage() => InitializeComponent();
    }
}
