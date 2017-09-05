using System;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters {
    public class NegateBoolConverter : IValueConverter {
        public object Convert(object value, Type target, object o, string lang) => !(bool)value;
        public object ConvertBack(object value, Type target, object o, string lang) => !(bool)value;
    }
}
