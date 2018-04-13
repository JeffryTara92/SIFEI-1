﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace SIF.Visualization.Excel.ViewModel
{
    internal class BooleanToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool) value)
                return 2;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}