using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace myFeed.Views.Uwp.Controls {
    public class Expander : Microsoft.Toolkit.Uwp.UI.Controls.Expander {
        public Expander() {
            Expanded += OnExpanded;
            Collapsed += OnCollapsed;
        }

        public static readonly DependencyProperty HeaderContentProperty = DependencyProperty
            .Register(nameof(HeaderContent), typeof(FrameworkElement),
                typeof(Expander), new PropertyMetadata(null));

        public FrameworkElement HeaderContent {
            get => (FrameworkElement)GetValue(HeaderContentProperty);
            set => SetValue(HeaderContentProperty, value);
        }

        private void OnCollapsed(object sender, EventArgs e) {
            var root = HeaderContent;
            if (root == null) throw new ArgumentNullException();
            ProcessAllDescendants(root, o => {
                if (o is Button button) {
                    var brush = Application.Current.Resources["ApplicationForegroundThemeBrush"];
                    button.Foreground = (SolidColorBrush)brush;
                }
            });
        }

        private void OnExpanded(object sender, EventArgs e) {
            var root = HeaderContent;
            if (root == null) throw new ArgumentNullException();
            ProcessAllDescendants(root, o => {
                if (o is Button button) {
                    button.Foreground = new SolidColorBrush(Colors.White);
                }
            });
        }

        private static void ProcessAllDescendants(DependencyObject o, Action<DependencyObject> action) {
            var childrenCount = VisualTreeHelper.GetChildrenCount(o);
            for (var x = 0; x < childrenCount; x++) {
                var nthChild = VisualTreeHelper.GetChild(o, x);
                ProcessAllDescendants(nthChild, action);
                action.Invoke(nthChild);
            }
        }
    }
}
