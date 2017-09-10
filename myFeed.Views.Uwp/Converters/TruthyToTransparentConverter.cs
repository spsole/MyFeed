using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters
{
    public class TruthyToTransparentConverter : IValueConverter
    {
        public object Convert(object value, Type type, object o, string lang) => value.IsDefault() ? 1 : 0.5;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => false;
    }
}