using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;

namespace myFeed.Extensions.Behaviors
{
    /// <summary>
    /// Open flyout action.
    /// </summary>
    public class OpenFlyoutAction : DependencyObject, IAction
    {
        /// <summary>
        /// Shows flyout.
        /// </summary>
        public object Execute(object sender, object parameter)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            return null;
        }
    }
}
