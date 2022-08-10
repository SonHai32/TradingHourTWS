using System;
using System.Globalization;
using System.Windows.Data;

namespace TradingHour.Helpers.Converters
{
    public class DisplayUpdateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return $"Auto update time: {count}";
            }
            return "Auto update time: 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}