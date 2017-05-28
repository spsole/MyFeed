using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace myFeed.Extensions.Controls
{
    /// <summary>
    /// Additional properties for Expander control.
    /// </summary>
    public class Expander : Microsoft.Toolkit.Uwp.UI.Controls.Expander
    {
        /// <summary>
        /// Identifies the <see cref="HeaderContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register(
                nameof(HeaderContent),
                typeof(FrameworkElement),
                typeof(Expander),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets a value indicating whether the Header of the control.
        /// </summary>
        public FrameworkElement HeaderContent
        {
            get => (FrameworkElement)GetValue(HeaderContentProperty);
            set => SetValue(HeaderContentProperty, value);
        }

        public Expander()
        {
            Expanded += OnExpanded;
            Collapsed += OnCollapsed;
        }

        ~Expander()
        {
            Expanded -= OnExpanded;
            Collapsed -= OnCollapsed;
        }

        /// <summary>
        /// Paints all buttons to default color when 
        /// expander control becomes collapsed.
        /// </summary>
        private void OnCollapsed(object sender, EventArgs e)
        {
            var root = HeaderContent;
            if (root == null) throw new ArgumentNullException();
            ProcessAllDescendants(root, o =>
            {
                if (o is Button button)
                    button.Foreground =
                        (SolidColorBrush)Application.Current
                            .Resources["ApplicationForegroundThemeBrush"];
            });
        }

        /// <summary>
        /// Paints all buttons to white color when expander becomes 
        /// expanded and painted using accent color brush.
        /// </summary>
        private void OnExpanded(object sender, EventArgs e)
        {
            var root = HeaderContent;
            if (root == null) throw new ArgumentNullException();
            ProcessAllDescendants(root, o =>
            {
                if (o is Button button)
                    button.Foreground =
                        new SolidColorBrush(Colors.White);
            });
        }

        /// <summary>
        /// Applies given function to all descendants of a certain DO in VisualTree.
        /// </summary>
        /// <param name="o">Object to analyze.</param>
        /// <param name="action">Action to apply.</param>
        private static void ProcessAllDescendants(DependencyObject o, Action<DependencyObject> action)
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(o);
            for (var x = 0; x < childrenCount; x++)
            {
                var nthChild = VisualTreeHelper.GetChild(o, x);
                ProcessAllDescendants(nthChild, action);
                action.Invoke(nthChild);
            }
        }
    }
}
