using Windows.UI.Xaml;
using myFeed.Services.Abstractions;
using myFeed.Views.Uwp.Services;
using Microsoft.Xaml.Interactivity;

namespace myFeed.Views.Uwp.Extensions
{
    public class NavigateAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register(nameof(Key), typeof(ViewKey),
                typeof(NavigateAction), new PropertyMetadata(null));

        public ViewKey Key
        {
            get => (ViewKey) GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            var service = UwpLocator.Current.Resolve<INavigationService>();
            service.Navigate(Key);
            return null;
        }
    }
}