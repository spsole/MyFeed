using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Extensions.Converters
{
    /// <summary>
    /// Negates boolean value.
    /// </summary>
    public class NegateBoolConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType,
            object parameter, string language) => !(bool) value;

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, 
            object parameter, string language) => !(bool) value;
    }
}
