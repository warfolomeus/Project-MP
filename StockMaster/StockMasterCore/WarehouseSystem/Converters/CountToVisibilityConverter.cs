using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WarehouseApp.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                bool shouldShow = count > 0;

                if (Inverse)
                {
                    shouldShow = !shouldShow;
                }

                return shouldShow ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}