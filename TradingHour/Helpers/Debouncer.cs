using System;
using System.Timers;

namespace TradingHour.Helpers
{
    public class Debouncer : IDisposable
    {
        private Timer _timer;
        private Action _action;

        public void Debounce(Action action, int interval)
        {
            _action = action;
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = interval;
            _timer.AutoReset = false;
            _timer.Start();
        }

        public void ClearTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Dispose();
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _action?.Invoke();
        }

        public void Dispose()
        {
            ClearTimer();
            _timer = null;
        }
    }
}