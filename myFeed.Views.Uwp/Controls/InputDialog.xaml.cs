using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace myFeed.Views.Uwp.Controls
{
    public sealed partial class InputDialog : ContentDialog
    {
        public InputDialog()
        {
            InitializeComponent();
            var resourceLoader = new ResourceLoader();
            PrimaryButtonText = resourceLoader.GetString("Ok");
            SecondaryButtonText = resourceLoader.GetString("Cancel");
        }

        public InputDialog(string message, string title) : this()
        {
            InputBox.PlaceholderText = message;
            Title = title;
        }

        public string Value => InputBox.Text;
    }
}