using System;
using System.Collections.Generic;

namespace TradingHour.Helpers
{
    public static class DateTimeHeplers
    {
        private const char TIME_DELIMETER = ':';

        private static readonly Dictionary<string, string> TimezoneIds = new Dictionary<string, string>()
        {
            {"MET", "Central European Standard Time" },
            {"Hongkong", "China Standard Time" },
            {"Japan", "Tokyo Standard Time" },
            {"US/Eastern", "US Eastern Standard Time" },
            {"GB-Eire", "Central European Standard Time" },
            {"US/Central", "Central Standard Time" },
            {"Europe/Riga", "FLE Standard Time" },
            {"Europe/Tallinn", "FLE Standard Time" },
            {"Europe/Moscow", "Russian Standard Time" },
            {"Asia/Calcutta", "India Standard Time" },
            {"Europe/Vilnius", "FLE Standard Time" },
            {"Israel", "Israel Standard Time" },
            {"Australia/NSW", "E. Australia Standard Time" },
            {"Europe/Warsaw", "Central European Standard Time" },
            {"US/Pacific", "Pacific Standard Time" },
            {"Europe/Helsinki", "FLE Standard Time" },
            {"Europe/Budapest", "Central Europe Standard Time" },
        };

        public static DateTime StringToDateTime(this string dateTimeString, ref bool isClose)
        {
            dateTimeString = dateTimeString?.Trim();
            var timeSplit = dateTimeString.Split(TIME_DELIMETER);
            if (timeSplit.Length < 2)
                return new DateTime();
            if (Int32.TryParse(timeSplit[0]?.Trim().Substring(0, 4), out int year)
               && Int32.TryParse(timeSplit[0]?.Trim().Substring(4, 2), out int month)
               && Int32.TryParse(timeSplit[0]?.Trim().Substring(6, 2), out int day))
            {
                var isHourValid = int.TryParse(timeSplit[1]?.Trim().Substring(0, 2), out int hour);
                var isMinuteValid = int.TryParse(timeSplit[1]?.Trim().Substring(2, 2), out int minute);
                if (!isHourValid && !isMinuteValid)
                {
                    isClose = true;
                }
                return new DateTime(year, month, day, isHourValid ? hour : 0, isMinuteValid ? minute : 0, 0, DateTimeKind.Unspecified);
            }
            isClose = true;
            return new DateTime();
        }

        public static DateTime ConvertToTimeZone(this DateTime sourceTime, string timeZoneId)
        {
            var timeZoneByTimeZoneId = TimezoneIds[timeZoneId];
            if (string.IsNullOrEmpty(timeZoneByTimeZoneId))
                return sourceTime.ToUniversalTime();
            var sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneByTimeZoneId);
            return TimeZoneInfo.ConvertTime(sourceTime, sourceTimeZone, TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time"));
        }

        public static DateTime ConvertToSpecifyTimeZone(this DateTime sourceTime, string timeZoneId)
        {
            var timeZoneByTimeZoneId = TimezoneIds[timeZoneId];
            if (string.IsNullOrEmpty(timeZoneByTimeZoneId))
                return sourceTime.ToUniversalTime();
            var sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneByTimeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(sourceTime, sourceTimeZone);
        }
    }
}