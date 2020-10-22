﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace tweetz.core.Converters
{
    public class ViewModelLocatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = parameter as Type ?? throw new ArgumentNullException(nameof(parameter));
            return BootStrapper.GetService(type);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("General", "RCS1079:Throwing of new NotImplementedException.")]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}