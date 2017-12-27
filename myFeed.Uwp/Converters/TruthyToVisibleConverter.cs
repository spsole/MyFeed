using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace myFeed.Uwp.Converters
{
    public class TruthyToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return IsDefault(value) ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;

        public static bool IsDefault(object instance)
        {
            switch (instance)
            {
                case bool b:
                    return b == false;
                case int i:
                    return i == 0;
                default:
                    return instance == null;
            }
        }
    }
}