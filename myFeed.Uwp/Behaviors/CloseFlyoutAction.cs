using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace myFeed.Uwp.Behaviors
{
    public class CloseFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var opened = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in opened) popup.IsOpen = false;
            return null;
        }
    }
}
