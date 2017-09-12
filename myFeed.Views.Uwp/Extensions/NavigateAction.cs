using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace myFeed.Views.Uwp.Extensions
{
    public class NavigateAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty TargetPageProperty =
            DependencyProperty.Register(nameof(TargetPage), typeof(Type),
                typeof(NavigateAction), new PropertyMetadata(null));

        public Type TargetPage
        {
            get => (Type) GetValue(TargetPageProperty);
            set => SetValue(TargetPageProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            var root = Window.Current.Content as Frame;
            GetNavigationFrame(root).Navigate(TargetPage);
            return null;
        }

        private static Frame GetNavigationFrame(DependencyObject root)
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var x = 0; x < childrenCount; x++)
            {
                var child = VisualTreeHelper.GetChild(root, x);
                if (child is Frame thisFrame) return thisFrame;
                var frame = GetNavigationFrame(child);
                if (frame != null) return frame;
            }
            return null;
        }
    }
}