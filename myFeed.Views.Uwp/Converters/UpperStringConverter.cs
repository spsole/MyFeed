using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters {
    public class UpperStringConverter : IValueConverter {
        public object Convert(object value, Type target, object o, string lang) => ((string)value).ToUpperInvariant();
        public object ConvertBack(object value, Type target, object o, string lang) => null;
    }
}
