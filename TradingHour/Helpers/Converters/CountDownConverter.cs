using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TradingHour.Models;

namespace TradingHour.Helpers.Converters
{
    internal class CountDownConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "TIME ERROR";
            if (value is TradingCountDown countdown)
            {
                if (countdown.IsNoInfo)
                    return "NO INFO";
                string state = "";
                switch (countdown.CountDowntState)
                {
                    case CountDowntState.OPENING:
                        state = "OPENING IN";
                        break;

                    case CountDowntState.CLOSING:
                        state = "CLOSING IN";
                        break;

                    default:
                        state = "TIME ERROR";
                        break;
                }
                return $"{state}\n{countdown.CountdownDisplay}";
            }
            return "TIME ERROR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}