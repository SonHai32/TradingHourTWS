using System;
using System.Globalization;
using System.Windows.Data;
using TradingHour.Models;

namespace TradingHour.Helpers.Converters
{
    public class LogMessageColorConverter : IValueConverter
    {
        private const string WHITE_COLOR = "#FFFFFF";
        private const string ERROR_COLOR = "#FE0000";
        private const string SUCCESS_COLOR = "#6ED323";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogMessageType logMessageType)
            {
                switch (logMessageType)
                {
                    case LogMessageType.ERROR:
                        return ERROR_COLOR;

                    case LogMessageType.SUCCESS:
                        return SUCCESS_COLOR;

                    default:
                        return WHITE_COLOR;
                }
            }
            return WHITE_COLOR;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}