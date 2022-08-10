using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using TradingHour.Helpers;
using TradingHour.Models;
using TradingHour.Services;
using TradingHour.View.Utils;

namespace TradingHour.ViewModels
{
    public enum TradingHourType
    {
        OPENING,
        CLOSING,
        DURING
    }

    public class TradingDetailsItemViewModel : PropertyChangedBase
    {
        private const string CLOSED = "CLOSED";
        private const string TIME_ERROR = "TIME ERROR";
        private const string NO_TRADING_INFO = "NO INFO";
        private TradingDetails _tradingDetails;
        private const int ONE_MILLISECOND = 1000;
        private IFetchingInstructment _fetchingInstructment;
        private INofifyTradingHoursState _nofifyTradingHoursState;
        private Timer _countdownBlinkingTimer = new Timer();

        public TradingDetailsItemViewModel(TradingDetails tradingDetails, IFetchingInstructment fetchingInstructment, INofifyTradingHoursState nofifyTradingHoursState, INavigateService navigateService)
        {
            _tradingDetails = tradingDetails;
            _fetchingInstructment = fetchingInstructment;
            _nofifyTradingHoursState = nofifyTradingHoursState;
            _countdownBlinkingTimer.Elapsed += _countdownBlinkingTimer_Elapsed;
            InitCountDownInterval();
        }

        private int _executeOnOpenTime = 0;
        private int _executeOnCloseTime = 0;

        public int ExecuteOnOpenTime
        {
            get => _executeOnOpenTime;
            set => _executeOnOpenTime = value;
        }

        private bool _canExecuteCommand = false;

        public bool CanExecuteCommand
        {
            get => _canExecuteCommand;
            set => _canExecuteCommand = value;
        }

        public int ExecuteOnCloseTime
        {
            get => _executeOnCloseTime;
            set => _executeOnCloseTime = value;
        }

        private bool _isBlinkingStart = false;

        private void _countdownBlinkingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TradingCountDown.IsBlinking = !TradingCountDown.IsBlinking;
            NotifyOfPropertyChange(() => TradingCountDown);
        }

        private void StartCountdownBlinking()
        {
            if (_isBlinkingStart)
                return;
            _isBlinkingStart = true;
            _countdownBlinkingTimer.Interval = 700;
            _countdownBlinkingTimer.Start();
        }

        private void StopCountdownBlinking()
        {
            _isBlinkingStart = false;
            TradingCountDown.IsBlinking = false;
            NotifyOfPropertyChange(() => TradingCountDown);
            _countdownBlinkingTimer.Stop();
        }

        public string ExchangeName { get => _tradingDetails.ExchangeName; }
        public string ExchangeSymbol { get => _tradingDetails.ExchangeSymbol; }
        public string InstrumentSymbol { get => _tradingDetails.InstrumentSymbol; }
        public string Country { get => _tradingDetails.Country; }

        public List<TradingHours> TradingHourNext1 => GetTradingHourClosingInWeek(0);
        public List<TradingHours> TradingHourNext2 => GetTradingHourClosingInWeek(1);
        public List<TradingHours> TradingHourNext3 => GetTradingHourClosingInWeek(2);
        public List<TradingHours> TradingHourNext4 => GetTradingHourClosingInWeek(3);
        public List<TradingHours> TradingHourNext5 => GetTradingHourClosingInWeek(4);
        public List<TradingHours> TradingHourNext6 => GetTradingHourClosingInWeek(5);
        public List<TradingHours> TradingHourNext7 => GetTradingHourClosingInWeek(6);

        private TradingHourType _tradingHourType = TradingHourType.DURING;

        private TradingCountDown _tradingCountDown = new TradingCountDown() { IsBlinking = false, CountDowntState = CountDowntState.NORMAL };

        public TradingCountDown TradingCountDown
        {
            get => _tradingCountDown;
            set
            {
                _tradingCountDown = value;
                NotifyOfPropertyChange(() => TradingCountDown);
            }
        }

        public string TimeZoneId => _tradingDetails.TimezoneId;

        private void HandleNotifyStateOpen(TimeSpan ts)
        {
            if (!CanExecuteCommand)
                return;
            if (ExecuteOnOpenTime >= 0)
            {
                if (CanNotifyStateChange(ts))
                {
                    StopCountdownBlinking();
                    Debouncer debouncer = new Debouncer();
                    debouncer.Debounce(() =>
                    {
                        _nofifyTradingHoursState.NotifyExchageStateChange(_tradingDetails, false, _tradingDetails.InstructmentSymbol2);
                    }, ExecuteOnOpenTime * 1000);
                }
            }
            else
            {
                if (CanNotifyStateChange(ts, Math.Abs(ExecuteOnOpenTime)))
                {
                    StopCountdownBlinking();
                    _nofifyTradingHoursState.NotifyExchageStateChange(_tradingDetails, true, _tradingDetails.InstructmentSymbol2);
                }
            }
        }

        private void HandleNotifyStateClose(TimeSpan ts)
        {
            if (!CanExecuteCommand)
                return;

            if (ExecuteOnCloseTime >= 0)
            {
                if (CanNotifyStateChange(ts))
                {
                    StopCountdownBlinking();
                    Debouncer debouncer = new Debouncer();
                    debouncer.Debounce(() =>
                    {
                        _nofifyTradingHoursState.NotifyExchageStateChange(_tradingDetails, false, _tradingDetails.InstructmentSymbol2);
                    }, ExecuteOnCloseTime * 1000);
                }
            }
            else
            {
                if (CanNotifyStateChange(ts, Math.Abs(ExecuteOnCloseTime)))
                {
                    StopCountdownBlinking();
                    _nofifyTradingHoursState.NotifyExchageStateChange(_tradingDetails, false, _tradingDetails.InstructmentSymbol2);
                }
            }
        }

        private bool CanNotifyStateChange(TimeSpan ts, double timeInvoke = 0)
        {
            var timeout = Math.Floor(ts.TotalSeconds);
            return timeout == timeInvoke;
        }

        public TradingHourType TradingHourType
        {
            get => _tradingHourType;
            set => _tradingHourType = value;
        }

        private void InitCountDownInterval()
        {
            Timer time = new Timer();
            time.Interval = ONE_MILLISECOND;
            time.Elapsed += Time_Elapsed;
            time.Start();
        }

        public void UpdateTradingHour(List<TradingHours> tradingHours)
        {
            _tradingDetails.TradingHours = tradingHours;
            NotifyOfPropertyChange(string.Empty);
        }

        private string GetTimeSpanDisplay(TimeSpan ts)
        {
            return ts.ToString("d'd 'h'h 'm'm 's's'");
        }

        private void HandleDisplayWarningTime(CountDowntState countDowntState, TimeSpan ts)
        {
            TradingCountDown.CountDowntState = CountDowntState.NORMAL;
            NotifyOfPropertyChange(() => TradingCountDown);
            StopCountdownBlinking();

            if (_warningTime == 0)
            {
                StopCountdownBlinking();
                TradingCountDown.CountDowntState = CountDowntState.NORMAL;
                NotifyOfPropertyChange(() => TradingCountDown);
                return;
            }
            var timeout = Math.Floor(ts.TotalSeconds);
            if (timeout <= _warningTime)
            {
                StartCountdownBlinking();
                TradingCountDown.CountDowntState = countDowntState;
                NotifyOfPropertyChange(() => TradingCountDown);
                return;
            }
        }

        public TradingCountDown CountDownTime
        {
            get
            {
                if (!_tradingDetails.TradingHours.Any())
                    return null;
                var currentTime = DateTime.Now;
                var todayTradingHourList = _tradingDetails.TradingHours.Where(time => currentTime.Date.CompareTo(time.OpeningTime.Date) == 0 && !time.IsTrandingClosed);
                if (todayTradingHourList.Any())
                {
                    if (todayTradingHourList.Count() >= 2)
                    {
                        var soonSeasion = todayTradingHourList.Where(time => time.OpeningTime.CompareTo(currentTime) > 0 && !time.IsTrandingClosed).FirstOrDefault();
                        if (soonSeasion != null)
                        {
                            TimeSpan tsSoon = soonSeasion.OpeningTime.Subtract(currentTime);
                            HandleDisplayWarningTime(CountDowntState.OPENING, tsSoon);
                            HandleNotifyStateOpen(tsSoon);
                            return new TradingCountDown() { Countdown = tsSoon, CountdownDisplay = GetTimeSpanDisplay(tsSoon), CountDowntState = CountDowntState.OPENING };
                            //return "OPENING IN \n" + GetTimeSpanDisplay(tsSoon);
                        }
                        var timeInSeasion = todayTradingHourList.Where(time => currentTime.CompareTo(time.OpeningTime) > 0 && currentTime.CompareTo(time.ClosingTime) < 0 && !time.IsTrandingClosed).FirstOrDefault();
                        if (timeInSeasion != null)
                        {
                            TimeSpan tsInSeasion = timeInSeasion.ClosingTime.Subtract(currentTime);
                            HandleDisplayWarningTime(CountDowntState.CLOSING, tsInSeasion);
                            HandleNotifyStateClose(tsInSeasion);
                            return new TradingCountDown() { Countdown = tsInSeasion, CountdownDisplay = GetTimeSpanDisplay(tsInSeasion), CountDowntState = CountDowntState.CLOSING };
                            //return "CLOSING IN \n" + GetTimeSpanDisplay(tsInSeasion);
                        }
                        var inCloseTimeSeasion = todayTradingHourList.Where(time => time.OpeningTime.CompareTo(currentTime) > 0 && !time.IsTrandingClosed).FirstOrDefault();
                        if (inCloseTimeSeasion != null)
                        {
                            TimeSpan tsOpenNextSeasion = inCloseTimeSeasion.OpeningTime.Subtract(currentTime);
                            HandleDisplayWarningTime(CountDowntState.OPENING, tsOpenNextSeasion);
                            HandleNotifyStateOpen(tsOpenNextSeasion);
                            //return "OPENING IN \n" + GetTimeSpanDisplay(tsOpenNextSeasion);
                            return new TradingCountDown() { Countdown = tsOpenNextSeasion, CountdownDisplay = GetTimeSpanDisplay(tsOpenNextSeasion), CountDowntState = CountDowntState.OPENING };
                        }
                        var nextDayTrading = _tradingDetails.TradingHours.Where(time => time.OpeningTime.Date.CompareTo(currentTime.Date) > 0 && !time.IsTrandingClosed).FirstOrDefault();
                        if (nextDayTrading != null)
                        {
                            TimeSpan tsNextDay = nextDayTrading.OpeningTime.Subtract(currentTime);
                            HandleDisplayWarningTime(CountDowntState.OPENING, tsNextDay);
                            HandleNotifyStateOpen(tsNextDay);
                            return new TradingCountDown() { Countdown = tsNextDay, CountdownDisplay = GetTimeSpanDisplay(tsNextDay), CountDowntState = CountDowntState.OPENING };
                            //return "OPENING IN \n" + GetTimeSpanDisplay(tsNextDay);
                        }
                        TradingHourType = TradingHourType.DURING;
                        return new TradingCountDown() { IsNoInfo = true };
                        //return NO_TRADING_INFO;
                    }
                    else
                    {
                        var todayTradingHour = todayTradingHourList.FirstOrDefault();
                        var isSoon = todayTradingHour.OpeningTime.CompareTo(currentTime) > 0;
                        if (isSoon)
                        {
                            TimeSpan tsSoon = todayTradingHour.OpeningTime.Subtract(currentTime);
                            HandleDisplayWarningTime(CountDowntState.OPENING, tsSoon);
                            HandleNotifyStateOpen(tsSoon);
                            //return "OPENING IN \n" + GetTimeSpanDisplay(tsSoon);
                            return new TradingCountDown() { Countdown = tsSoon, CountdownDisplay = GetTimeSpanDisplay(tsSoon), CountDowntState = CountDowntState.OPENING };
                        }
                        if (currentTime.CompareTo(todayTradingHour.OpeningTime) > 0 && currentTime.CompareTo(todayTradingHour.ClosingTime) < 0)
                        {
                            TimeSpan tsClose = todayTradingHour.ClosingTime.Subtract(currentTime);
                            HandleDisplayWarningTime(CountDowntState.CLOSING, tsClose);
                            HandleNotifyStateClose(tsClose);
                            //return "CLOSING IN \n" + GetTimeSpanDisplay(tsClose);
                            return new TradingCountDown() { Countdown = tsClose, CountdownDisplay = GetTimeSpanDisplay(tsClose), CountDowntState = CountDowntState.CLOSING };
                        }
                        var nextDate = _tradingDetails.TradingHours
                            .Where(time => time.OpeningTime.CompareTo(todayTradingHour.ClosingTime) > 0 && !time.IsTrandingClosed).FirstOrDefault();
                        if (nextDate == null)
                            //return NO_TRADING_INFO;
                            return new TradingCountDown() { IsNoInfo = true };
                        TimeSpan tsOpen = nextDate.OpeningTime.Subtract(currentTime);
                        HandleDisplayWarningTime(CountDowntState.OPENING, tsOpen);
                        HandleNotifyStateOpen(tsOpen);
                        //return "OPENING IN \n" + GetTimeSpanDisplay(tsOpen);
                        return new TradingCountDown() { Countdown = tsOpen, CountdownDisplay = GetTimeSpanDisplay(tsOpen), CountDowntState = CountDowntState.OPENING };
                    }
                }
                var nextTradingDate = _tradingDetails.TradingHours.Where(time => time.OpeningTime.Date.CompareTo(currentTime.Date) > 0 && !time.IsTrandingClosed).FirstOrDefault();
                if (nextTradingDate != null)
                {
                    TimeSpan tsNextDay = nextTradingDate.OpeningTime.Subtract(currentTime);
                    HandleDisplayWarningTime(CountDowntState.OPENING, tsNextDay);
                    HandleNotifyStateOpen(tsNextDay);
                    //return "OPENING IN \n" + GetTimeSpanDisplay(tsNextDay);
                    return new TradingCountDown() { Countdown = tsNextDay, CountdownDisplay = GetTimeSpanDisplay(tsNextDay), CountDowntState = CountDowntState.OPENING };
                }
                //return NO_TRADING_INFO;
                return new TradingCountDown() { IsNoInfo = true };
            }
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            NotifyOfPropertyChange(() => CountDownTime);
        }

        public string CurrentOpeningTime
        {
            get
            {
                var currentTime = DateTime.Now;
                var todayTimeTrading = _tradingDetails.TradingHours.Where(tradingHour => currentTime.Date.CompareTo(tradingHour.TradingDate.Date) == 0);
                if (!todayTimeTrading.Any())
                    return NO_TRADING_INFO;
                if (todayTimeTrading.Count() >= 2)
                {
                    var timeRanges = new List<string>();
                    foreach (var time in todayTimeTrading)
                    {
                        timeRanges.Add(GetHourRange(time));
                    }
                    return string.Join("\n", timeRanges);
                }
                if (todayTimeTrading.FirstOrDefault().IsTrandingClosed)
                    return CLOSED;
                return todayTimeTrading.FirstOrDefault().OpeningTime.ToString("HH:mm");
            }
        }

        private string GetHourRange(TradingHours tradingHours)
        {
            return string.Format("{0}-{1}", tradingHours.OpeningTime.ToString("HH:mm"), tradingHours.ClosingTime.ToString("HH:mm"));
        }

        public string CurrentClosingTime
        {
            get
            {
                var currentTime = DateTime.Now;
                var todayTimeTrading = _tradingDetails.TradingHours.Where(tradingHour => tradingHour.TradingDate.Date.CompareTo(currentTime.Date) == 0).ToList();
                if (!todayTimeTrading.Any())
                    return NO_TRADING_INFO;
                if (todayTimeTrading.Count() >= 2)
                {
                    return GetClosingTimeInSeasion(todayTimeTrading);
                }
                if (todayTimeTrading.FirstOrDefault().IsTrandingClosed)
                    return CLOSED;
                return todayTimeTrading.FirstOrDefault().ClosingTime.ToString("HH:mm");
            }
        }

        private string GetClosingTimeInSeasion(List<TradingHours> tradingHours)
        {
            List<string> closingRange = new List<string>();

            for (int i = 0; i < tradingHours.Count(); i += 2)
            {
                closingRange.Add(string.Format("{0}-{1}", tradingHours[i].ClosingTime.ToString("HH:mm"), tradingHours[i + 1].OpeningTime.ToString("HH:mm")));
            }
            return string.Join("\n", closingRange) + "\n" + string.Format("{0}-{1}", tradingHours.Last().ClosingTime.ToString("HH:mm"), tradingHours.FirstOrDefault().OpeningTime.ToString("HH:mm"));
        }

        private string _countDown = string.Empty;
        public string CountDown { get => _countDown; set => _countDown = value; }

        private List<TradingHours> GetTradingHourClosingInWeek(int dayIndex)
        {
            var currentDate = DateTime.Now;
            currentDate = currentDate.AddDays(dayIndex);
            return _tradingDetails.TradingHours.Where(tradingHour => currentDate.Date.CompareTo(tradingHour.TradingDate.Date) == 0).ToList();
        }

        private int _warningTime = 0;

        public void UpdateWarningTime(int time)
        {
            _warningTime = time;
        }

        private ICommand _fetchInstructmentCommand;

        public ICommand FetchInstructmentCommand => _fetchInstructmentCommand ?? (_fetchInstructmentCommand = new RelayCommand(x => { FetchInstructment(); }));

        public void FetchInstructment()
        {
            _fetchingInstructment.FetchInstructment(_tradingDetails);
        }

        private ICommand _fetchInstructmentWithoutShowDetailCommand;

        public ICommand FetchInstructmentWithoutShowDetailCommand => _fetchInstructmentWithoutShowDetailCommand ?? (_fetchInstructmentWithoutShowDetailCommand = new RelayCommand(x => { FetchInstructmentWithoutDetail(); }));

        public void FetchInstructmentWithoutDetail()
        {
            _fetchingInstructment.FetchInstructment(_tradingDetails, false);
        }
    }
}