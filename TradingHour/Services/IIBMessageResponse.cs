using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Services
{
    public delegate void HandleMessageResponse(ContractDetails contractDetails, int reqId);

    internal interface IIBMessageResponse
    {
        event HandleMessageResponse OnMessageResponse;
    }
}