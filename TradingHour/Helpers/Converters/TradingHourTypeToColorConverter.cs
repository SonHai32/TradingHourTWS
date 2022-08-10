using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TradingHour.Models;
using TradingHour.ViewModels;

namespace TradingHour.Helpers.Converters
{
    internal class TradingHourTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TradingCountDown countDown)
            {
                if (countDown.IsBlinking)
                    return "#000000";
                switch (countDown.CountDowntState)
                {
                    case CountDowntState.OPENING:
                        return StaticConstants.GREEN_COLOR;

                    case CountDowntState.CLOSING:
                        return StaticConstants.RED_COLOR;

                    default:
                        return StaticConstants.WHITE_COLOR;
                }
            }
            return StaticConstants.WHITE_COLOR;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}