﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace myFeed.Views.Uwp.Converters {
    public class TruthyToVisibleConverter : IValueConverter {
        public object ConvertBack(object value, Type target, object o, string lang) => null;
        public object Convert(object value, Type target, object o, string lang) => 
            value.IsDefault() ? Visibility.Collapsed : Visibility.Visible;
    }
}
