using System;
using System.Globalization;
using System.Windows.Data;

namespace WarehouseApp.Converters
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price)
            {
                return price.ToString("N2") + " руб.";
            }
            return value?.ToString() ?? "0 руб.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}