using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingHour.Models;

namespace TradingHour.Helpers
{
    public static class TradingHoursHelper
    {
        private const char DELIMITER = ';';
        private const char TIME_RANGE_DELIMITER = '-';

        public static List<TradingHours> StringToTradingHours(this string tradingHoursString, string timeZoneId)
        {
            var tradingHoursSplit = tradingHoursString.Split(DELIMITER);
            List<TradingHours> tradingHoursList = new List<TradingHours>();
            foreach (var timeRange in tradingHoursSplit)
            {
                var timeRangeSplit = GetOpeningAndClosingTimeString(timeRange);
                var tradingHours = new TradingHours();
                tradingHours.SourceTimeZoneId = timeZoneId;
                bool isTradingClosed = false;
                if (timeRangeSplit.Count >= 2)
                {
                    var openingTime = DateTimeHeplers.StringToDateTime(timeRangeSplit[0], ref isTradingClosed);
                    var closingTime = DateTimeHeplers.StringToDateTime(timeRangeSplit[1], ref isTradingClosed);
                    tradingHours.OpeningTime = openingTime;
                    tradingHours.ClosingTime = closingTime;
                    if (openingTime == new DateTime() || closingTime == new DateTime())
                        tradingHours.IsTrandingClosed = true;
                    tradingHours.IsTrandingClosed = isTradingClosed;
                }
                else if (timeRangeSplit.Count == 1)
                {
                    var date = DateTimeHeplers.StringToDateTime(timeRangeSplit[0], ref isTradingClosed);
                    tradingHours.IsTrandingClosed = true;
                    tradingHours.ClosingTime = date;
                    tradingHours.OpeningTime = date;
                }
                tradingHoursList.Add(tradingHours);
            }
            return tradingHoursList;
        }

        private static List<string> GetOpeningAndClosingTimeString(string timeRange)
        {
            return timeRange.Split(TIME_RANGE_DELIMITER).ToList();
        }
    }
}