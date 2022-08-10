using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Services
{
    public delegate void HandleError(string errorMessage);

    internal interface IIBErrorHandler
    {
        event HandleError OnError;
    }
}