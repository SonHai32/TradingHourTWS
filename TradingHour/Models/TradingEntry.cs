namespace TradingHour.Models
{
    public class TradingEntry
    {
        private string _instructmentType = "STK";
        public string Country { get; set; }
        public string InstrumentSymbol { get; set; }
        public string ExchangeName { get; set; }
        public string ExchangeSymbol { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentSymbol2 { get; set; }
        public string InstrumentType { get => GetInstructmentType(); set => _instructmentType = value; }

        public string GetInstructmentType()
        {
            if ("stock".Contains(_instructmentType) || _instructmentType.Trim().ToUpper().Equals("STK"))
                return "STK";
            if ("futures".Contains(_instructmentType) || _instructmentType.Trim().ToUpper().Equals("FUT"))
                return "FUT";
            if (_instructmentType.Trim().ToUpper().Equals("CFDS"))
                return "CFD";
            if (_instructmentType.Trim().ToUpper().Equals("OPTIONS"))
                return "OPT";
            if (_instructmentType.Trim().ToUpper().Equals("BONDS"))
                return "BOND";
            return "STK";
        }
    }
}