using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingHour.Models;

namespace TradingHour.Services
{
    internal class FetchingInstructmentService : IFetchingInstructment
    {
        public event HandleFetchingInstructment OnFetchingInstructment;

        public void FetchInstructment(TradingDetails tradingDetails, bool showDetail = true)
        {
            OnFetchingInstructment?.Invoke(tradingDetails, showDetail);
        }
    }
}