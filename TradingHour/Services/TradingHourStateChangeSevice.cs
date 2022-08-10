using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingHour.Models;

namespace TradingHour.Services
{
    public class TradingHourStateChangeSevice : INofifyTradingHoursState
    {
        public event HandleTradingHoursState OnTradingHourStateChanged;

        public void NotifyExchageStateChange(TradingDetails exchange, bool isOpen, string arg)
        {
            OnTradingHourStateChanged?.Invoke(exchange, isOpen, arg);
        }
    }
}