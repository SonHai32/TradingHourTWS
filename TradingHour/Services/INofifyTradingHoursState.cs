using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingHour.Models;

namespace TradingHour.Services
{
    public delegate void HandleTradingHoursState(TradingDetails exchange, bool isOpen, string arg);

    public interface INofifyTradingHoursState
    {
        event HandleTradingHoursState OnTradingHourStateChanged;

        void NotifyExchageStateChange(TradingDetails exchange, bool isOpen, string arg);
    }
}