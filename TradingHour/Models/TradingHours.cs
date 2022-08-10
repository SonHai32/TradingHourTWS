using System;
using TradingHour.Helpers;

namespace TradingHour.Models
{
    public class TradingHours
    {
        public TradingHours()
        {
        }

        private DateTime _openingTime;
        private DateTime _closingTime;

        public DateTime OpeningTime { get => _openingTime.ConvertToTimeZone(SourceTimeZoneId); set => _openingTime = value; }
        public DateTime ClosingTime { get => _closingTime.ConvertToTimeZone(SourceTimeZoneId); set => _closingTime = value; }
        public bool IsTrandingClosed { get; set; } = false;
        public String SourceTimeZoneId { get; set; }
        public DateTime TradingDate => _openingTime;
    }
}