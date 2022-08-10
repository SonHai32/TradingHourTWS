using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TradingHour.Helpers.Converters
{
    public class WeeklyTradingHourColorConverter : IValueConverter
    {
        private const string CLOSED = "CLOSED";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string state)
            {
                if (state.Equals(CLOSED))
                {
                    return "#FE0000";
                }
                return "White";
            }

            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}