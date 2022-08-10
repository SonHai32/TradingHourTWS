using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Models
{
    public enum CountDowntState
    {
        OPENING,
        CLOSING,
        NORMAL
    }

    public class TradingCountDown : IComparable
    {
        private bool _isNoInfo = false;
        public CountDowntState CountDowntState { get; set; }
        public bool IsBlinking { get; set; }
        public string CountdownDisplay { get; set; }
        public TimeSpan Countdown { get; set; }

        public bool IsNoInfo
        {
            get => _isNoInfo;
            set => _isNoInfo = value;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 0;
            if (obj is TradingCountDown countDown)
            {
                return Countdown.CompareTo(countDown.Countdown);
            }
            return 0;
        }
    }
}