using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters {
    public class LocaleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var key = (string) parameter;
            var loader = new ResourceLoader();
            try {
                return loader.GetString(key);
            } catch (Exception) {
                return "NOT IMPLEMENTED";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
