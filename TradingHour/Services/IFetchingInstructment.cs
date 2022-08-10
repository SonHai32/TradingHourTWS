using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingHour.Models;

namespace TradingHour.Services
{
    public delegate void HandleFetchingInstructment(TradingDetails tradingDetails, bool showDetail);

    public interface IFetchingInstructment
    {
        event HandleFetchingInstructment OnFetchingInstructment;

        void FetchInstructment(TradingDetails tradingDetails, bool showDetail = true);
    }
}