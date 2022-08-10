using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Models
{
    public enum LogMessageType
    {
        ERROR,
        INFO,
        SUCCESS,
    }

    public class LogMessage
    {
        public string Message { get; set; }

        public LogMessageType MessageType
        {
            get; set;
        }
    }
}