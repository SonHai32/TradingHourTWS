using System.Collections.Generic;

namespace TradingHour.Models
{
    public class TradingDetails
    {
        private string _exChangeName;
        private string _exchangeSymbol;
        private string _instrumentSymbol;
        private string _instructmentType = "STK";
        private string _timeZoneId;
        private string _instructmentSymbol2;
        private string _country;

        public TradingDetails()
        {
        }

        public string ExchangeName { get => _exChangeName; set => _exChangeName = value; }
        public string ExchangeSymbol { get => _exchangeSymbol; set => _exchangeSymbol = value; }
        public string InstrumentSymbol { get => _instrumentSymbol; set => _instrumentSymbol = value; }
        public string InstructmentType { get => _instructmentType; set => _instructmentType = value; }
        public string TimezoneId { get => _timeZoneId; set => _timeZoneId = value; }
        public string InstructmentSymbol2 { get => _instructmentSymbol2; set => _instructmentSymbol2 = value; }
        public string Country { get => _country; set => _country = value; }
        public List<TradingHours> TradingHours { get; set; }
    }
}