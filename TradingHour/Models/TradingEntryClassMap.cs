using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Models
{
    public class TradingEntryClassMap : ClassMap<TradingEntry>
    {
        public TradingEntryClassMap()
        {
            Map(m => m.Country).Name("Country");
            Map(m => m.ExchangeName).Name("Exchange Name");
            Map(m => m.ExchangeSymbol).Name("Exchange Symbol");
            Map(m => m.InstrumentSymbol).Name("Instrument Symbol");
            Map(m => m.InstrumentName).Name("Instrument Name");
            Map(m => m.InstrumentType).Name("Instrument Type");
            Map(m => m.InstrumentSymbol2).Name("Symbol For Run Command");
        }
    }
}