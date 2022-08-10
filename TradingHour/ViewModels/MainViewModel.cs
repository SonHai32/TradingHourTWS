using Caliburn.Micro;
using CsvHelper;
using CsvHelper.Configuration;
using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using TradingHour.Helpers;
using TradingHour.Models;
using TradingHour.Models.ViewInterfaces;
using TradingHour.Services;
using TradingHour.View.Utils;
using Timer = System.Timers.Timer;

namespace TradingHour.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private EWrapperImpl ibClient = new EWrapperImpl();
        private ObservableCollection<TradingDetailsItemViewModel> _tradingDetailList = new ObservableCollection<TradingDetailsItemViewModel>();
        private List<TradingEntry> _tradingEntries;
        private FetchingInstructmentService _fetchingInstructmentService = new FetchingInstructmentService();
        private TradingHourStateChangeSevice _tradingHourStateChangeSevice = new TradingHourStateChangeSevice();
        private NavigateService _navigateService;

        private bool _isConnected = false;
        private const string ERROR = "[ERROR]";
        private const string INFO = "[INFO]";
        private const string SUCCESS = "[SUCCESS]";
        private const string CLOSE_MESSAGE_LOG = "Close log";
        private const string OPEN_MESSAGE_LOG = "Open log";
        private const string CONNECTED_MESSAGE = "Server connect successfully";
        private Timer _updateTimer;
        public IMainPageView _mainPageView;
        private Debouncer _autoUpdateDebouncer = new Debouncer();
        private Debouncer debouncer = new Debouncer();
        private WindowCommandServices _windowCommandServices;
        private string _host = "127.0.0.1";
        private int _port = 7496;
        private int _clientId = 1;

        public string Host
        {
            get => _host;
            set => _host = value;
        }

        public int Port
        {
            get => _port;
            set => _port = value;
        }

        public int ClientId
        {
            get => _clientId;
            set => _clientId = value;
        }

        public MainViewModel(IMainPageView mainPageView)
        {
            _mainPageView = mainPageView;
            _logMessages.CollectionChanged += _logMessages_CollectionChanged;

            _updateTimer = new Timer();
            _updateTimer.Elapsed += _updateTimer_Elapsed;
            _fetchingInstructmentService.OnFetchingInstructment += _fetchingInstructmentService_OnFetchingInstructment;
            _tradingHourStateChangeSevice.OnTradingHourStateChanged += _tradingHourStateChangeSevice_OnTradingHourStateChanged;
            _windowCommandServices = new WindowCommandServices();
            _navigateService = new NavigateService();
        }

        private void _tradingHourStateChangeSevice_OnTradingHourStateChanged(TradingDetails exchange, bool isOpen, string arg)
        {
            if (isOpen)
            {
                if (string.IsNullOrEmpty(CommandForOpening))
                    return;
                _windowCommandServices.ExecuteCommand(string.Format("{0} {1}", CommandForOpening, arg));
                return;
            }
            if (string.IsNullOrEmpty(CommandForClosing))
                return;
            _windowCommandServices.ExecuteCommand(string.Format("{0} {1}", CommandForClosing, arg));
        }

        private void _logMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher?.Invoke(() =>
            {
                _mainPageView.ScrollToButtonLogMessageBox(LogMessages.Count - 1);
            });
        }

        private ObservableCollection<LogMessage> _logMessages = new ObservableCollection<LogMessage>();

        public ObservableCollection<LogMessage> LogMessages
        {
            get => _logMessages;
        }

        private int _reqFetchId = -1000;
        private int _reqUpdateId = 50000;
        private int _updateCount = 0;
        public int UpdateCount { get => _updateCount; set => _updateCount = value; }

        private void _fetchingInstructmentService_OnFetchingInstructment(TradingDetails tradingDetails, bool showDetail)
        {
            if (!_isConnected)
                return;
            var contract = new Contract();
            contract.Symbol = tradingDetails.InstrumentSymbol;
            contract.Exchange = tradingDetails.ExchangeSymbol;
            contract.SecType = tradingDetails.InstructmentType;
            ibClient.ClientSocket.reqContractDetails(showDetail ? _reqFetchId : _reqUpdateId, contract);
        }

        private void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FetchData();
            UpdateCount++;
            NotifyOfPropertyChange(() => UpdateCount);
        }

        private void ConnectToServer()
        {
            try
            {
                if (!IsConfigValid())
                    throw new Exception("Missing Config");
                ibClient.ClientSocket.eConnect(Host, Port, ClientId);
                var reader = new EReader(ibClient.ClientSocket, ibClient.Signal);
                reader.Start();
                new Thread(() =>
                {
                    while (ibClient.ClientSocket.IsConnected())
                    {
                        ibClient.Signal.waitForSignal();
                        reader.processMsgs();
                    }
                })
                { IsBackground = true }.Start();
                while (ibClient.NextOrderId <= 0)
                {
                    _isConnected = ibClient.ClientSocket.IsConnected();
                }
                ibClient.OnMessageResponse += IbClient_OnMessageResponse;
                ibClient.OnError += IbClient_OnError;
                LogMessages.Add(new LogMessage() { Message = GetMessage(CONNECTED_MESSAGE, LogMessageType.SUCCESS), MessageType = LogMessageType.SUCCESS });
            }
            catch (Exception ex)
            {
                LogMessages.Add(new LogMessage() { Message = GetMessage(ex.Message, LogMessageType.ERROR), MessageType = LogMessageType.ERROR });
            }
        }

        private bool IsConfigValid()
        {
            return !string.IsNullOrEmpty(Host);
        }

        private void IbClient_OnError(string errorMessage)
        {
            Application.Current.Dispatcher?.Invoke(() =>
            {
                LogMessages.Add(new LogMessage() { Message = GetMessage(errorMessage, LogMessageType.ERROR), MessageType = LogMessageType.ERROR });
            });
        }

        private void LoadDataConfiguration()
        {
            var currentDirectory = AppContext.BaseDirectory;
            //var a = Directory.EnumerateFiles(currentDirectory, "*.csv", SearchOption.AllDirectories);
            var dataFiles = Directory.GetFiles(currentDirectory, "Data.csv");
            if (!dataFiles.Any())
            {
                LogMessages.Add(new LogMessage() { Message = GetMessage("No data file Data.csv was found in current directory", LogMessageType.ERROR), MessageType = LogMessageType.ERROR });
                return;
            }
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true };
            using (StreamReader streamReader = new StreamReader(dataFiles.FirstOrDefault()))
            {
                try
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, csvConfiguration))
                    {
                        csvReader.Context.RegisterClassMap<TradingEntryClassMap>();
                        var entryData = csvReader.GetRecords<TradingEntry>().ToList();
                        _tradingEntries = entryData;
                        FetchData();
                    }
                }
                catch (Exception e)
                {
                    LogMessages.Add(new LogMessage() { Message = GetMessage(e.Message, LogMessageType.ERROR), MessageType = LogMessageType.ERROR });
                }
            }
        }

        private void FetchData()
        {
            if (!_isConnected)
                return;
            if (!_tradingEntries.Any())
                return;
            var contractList = PrepareContractList(_tradingEntries);
            if (!contractList.Any())
                return;

            // Pause here until the connection is complete
            contractList.AsParallel().ForAll(x =>
           {
               if (x != null)
               {
                   ibClient.ClientSocket.reqContractDetails(contractList.IndexOf(x), x);
                   Thread.Sleep(100);
               }
           });
        }

        private List<Contract> PrepareContractList(List<TradingEntry> tradingEntries)
        {
            var contractList = new List<Contract>();
            foreach (var item in tradingEntries)
            {
                var contract = new Contract();
                contract.Symbol = item.InstrumentSymbol;
                contract.SecType = item.InstrumentType;
                contract.Exchange = item.ExchangeSymbol;
                contractList.Add(contract);
            }
            return contractList;
        }

        private void ResolveConfict(TradingDetails tradingDetail)
        {
            var contractEntry = _tradingEntries.Where(entry => entry.InstrumentSymbol.Equals(tradingDetail.InstrumentSymbol)).FirstOrDefault();
            if (contractEntry != null)
            {
                tradingDetail.ExchangeName = contractEntry.ExchangeName;
                tradingDetail.ExchangeSymbol = contractEntry.ExchangeSymbol;
                tradingDetail.InstrumentSymbol = contractEntry.InstrumentSymbol;
                tradingDetail.InstructmentType = contractEntry.InstrumentType;
                tradingDetail.InstructmentSymbol2 = contractEntry.InstrumentSymbol2;
                tradingDetail.Country = contractEntry.Country;
            }
        }

        private void IbClient_OnMessageResponse(ContractDetails contractDetails, int reqId)
        {
            if (contractDetails == null)
                return;
            TradingDetails tradingDetails = new TradingDetails() { ExchangeName = contractDetails.Contract.Exchange, ExchangeSymbol = contractDetails.Contract.Exchange, InstrumentSymbol = contractDetails.Contract.Symbol, TimezoneId = contractDetails.TimeZoneId };
            tradingDetails.TradingHours = contractDetails.TradingHours.StringToTradingHours(contractDetails.TimeZoneId);
            ResolveConfict(tradingDetails);
            TradingDetailsItemViewModel tradingDetailsItem = new TradingDetailsItemViewModel(tradingDetails, _fetchingInstructmentService, _tradingHourStateChangeSevice, _navigateService);
            Application.Current.Dispatcher?.Invoke(() =>
            {
                HandleTradingDetailUpdate(tradingDetailsItem, tradingDetails);
                LogMessages.Add(new LogMessage() { Message = GetMessage($"{tradingDetails.ExchangeName} Trading hour has been fetched", LogMessageType.INFO), MessageType = LogMessageType.INFO });
            });
            if (reqId == _reqFetchId)
            {
                HandleShowContractDetailPopup(contractDetails);
            }
            _reqUpdateId++;
        }

        private void HandleShowContractDetailPopup(ContractDetails contractDetails)
        {
            List<string> contractDetailMetadata = new List<string>();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(contractDetails))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(contractDetails);
                contractDetailMetadata.Add(string.Format("{0}={1}", name, value == null ? " N/A" : value));
            }
            ContractDetailPopupText = string.Join("\n", contractDetailMetadata);
            Application.Current.Dispatcher?.Invoke(() =>
            {
                _navigateService.NavigateWindow(ContractDetailPopupText);
            });

            _reqFetchId = _reqFetchId - 1;
        }

        private void HandleTradingDetailUpdate(TradingDetailsItemViewModel item, TradingDetails trading)
        {
            var existedTradingDetail = _tradingDetailList.Where(tradingDetail => tradingDetail.InstrumentSymbol.Equals(item.InstrumentSymbol)).FirstOrDefault();
            if (existedTradingDetail != null)
            {
                existedTradingDetail.UpdateTradingHour(trading.TradingHours);
                return;
            }

            _tradingDetailList.Add(item);
        }

        public ObservableCollection<TradingDetailsItemViewModel> TradingDetailsList
        {
            get => _tradingDetailList;
        }

        private int _timeToUpdate;

        public int TimeToUpdate
        {
            get => _timeToUpdate;
            set
            {
                _timeToUpdate = value;
            }
        }

        private int _timeToWaring = 0;

        public int TimeToWarning
        {
            get => _timeToWaring;
            set
            {
                _timeToWaring = value;

                foreach (var item in _tradingDetailList)
                {
                    item.UpdateWarningTime(_timeToWaring);
                }
                if (value > 0)
                {
                    LogMessages.Add(new LogMessage() { Message = GetMessage($"Countdown warning time has been set to {value} seconds", LogMessageType.SUCCESS), MessageType = LogMessageType.SUCCESS });
                }
                else
                {
                    LogMessages.Add(new LogMessage() { Message = GetMessage($"Countdown warning has been stop", LogMessageType.SUCCESS), MessageType = LogMessageType.SUCCESS });
                }
            }
        }

        private void HandleAutoUpdate()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
            }
            _updateTimer.Interval = Math.Abs(_timeToUpdate) * 1000;
            _updateTimer.Start();
        }

        private string GetMessage(string message, LogMessageType messageType)
        {
            string messageTypeTag = INFO;
            switch (messageType)
            {
                case LogMessageType.ERROR:
                    messageTypeTag = ERROR;
                    break;

                case LogMessageType.SUCCESS:
                    messageTypeTag = SUCCESS;
                    break;

                default:
                    messageTypeTag = INFO;
                    break;
            }
            return string.Format("{0} - {1}: {2}", messageTypeTag, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), message);
        }

        private bool _isContractDetailPopupOpen = false;

        public bool IsContractDetailPopupText
        {
            get => _isContractDetailPopupOpen;
            set => _isContractDetailPopupOpen = value;
        }

        private string _contractDetailPopupText = string.Empty;

        public string ContractDetailPopupText
        {
            get => _contractDetailPopupText;
            set => _contractDetailPopupText = value;
        }

        private ICommand _updateDataCommand;
        public ICommand UpdateDateCommand => _updateDataCommand ?? (_updateDataCommand = new RelayCommand(x => { HandleUpdateData(); }));

        private void HandleUpdateData()
        {
            FetchData();
            UpdateCount++;
            NotifyOfPropertyChange(() => UpdateCount);
        }

        private ICommand _connectToServerCommand;
        public ICommand ConnectToServerCommand => _connectToServerCommand ?? (_connectToServerCommand = new RelayCommand(x => { HandleConnectToServer(); }));

        private void HandleConnectToServer()
        {
            ConnectToServer();
            while (_isConnected)
            {
                LoadDataConfiguration();
                _mainPageView.CloseConfigPopup();
                break;
            }
        }

        private bool _isShowMessageLog = true;

        public bool IsShowMessageLog
        {
            get => _isShowMessageLog;
            set
            {
                _isShowMessageLog = value;
                NotifyOfPropertyChange(() => ToggleMessageLogButtonName);
                NotifyOfPropertyChange(() => IsShowMessageLog);
            }
        }

        private void ToggleMessageBox()
        {
            IsShowMessageLog = !_isShowMessageLog;
        }

        public string ToggleMessageLogButtonName => IsShowMessageLog ? CLOSE_MESSAGE_LOG : OPEN_MESSAGE_LOG;

        private ICommand _toggleShowMessageBox;

        public ICommand ToggleShowMessageBox => _toggleShowMessageBox ?? (_toggleShowMessageBox = new RelayCommand(x =>
                         {
                             ToggleMessageBox();
                         }));

        private bool _isAutoUpdate = false;

        public string IsAutoUpdateName
        {
            get
            {
                if (_isAutoUpdate)
                    return "Stop auto update";
                return "Start auto update";
            }
        }

        private ICommand _toggleAutoUpdateCmd;

        public ICommand ToggleAutoUpdateCmd => _toggleAutoUpdateCmd ?? (_toggleAutoUpdateCmd = new RelayCommand(x =>
                       {
                           ToggleAutoUpdate();
                       }));

        private void ToggleAutoUpdate()
        {
            if (_isAutoUpdate)
            {
                _isAutoUpdate = false;
                NotifyOfPropertyChange(() => IsAutoUpdateName);
                _updateTimer.Stop();
                LogMessages.Add(new LogMessage() { Message = GetMessage($"Auto update is stopped", LogMessageType.SUCCESS), MessageType = LogMessageType.SUCCESS });
                return;
            }
            if (TimeToUpdate > 0)
            {
                _isAutoUpdate = true;
                LogMessages.Add(new LogMessage() { Message = GetMessage($"Auto update is running per {TimeToUpdate} seconds", LogMessageType.SUCCESS), MessageType = LogMessageType.SUCCESS });
                HandleAutoUpdate();
                NotifyOfPropertyChange(() => IsAutoUpdateName);
            }
            if (TimeToUpdate == 0)
            {
                LogMessages.Add(new LogMessage() { Message = GetMessage($"Auto update cannot be run with 0 second", LogMessageType.ERROR), MessageType = LogMessageType.ERROR });
            }
        }

        public string TradingDate1 => GetTradingDate(0);
        public string TradingDate2 => GetTradingDate(1);
        public string TradingDate3 => GetTradingDate(2);
        public string TradingDate4 => GetTradingDate(3);
        public string TradingDate5 => GetTradingDate(4);
        public string TradingDate6 => GetTradingDate(5);
        public string TradingDate7 => GetTradingDate(6);

        private string GetTradingDate(int dayIndex)
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("he-IL");
            var currentDate = DateTime.Now;
            currentDate = currentDate.AddDays(dayIndex);
            return currentDate.ToString("dd.MM.yyyy", ci);
        }

        private string _commandForOpening = string.Empty;
        private string _commandForClosing = string.Empty;

        public string CommandForOpening
        {
            get => _commandForOpening;
            set => _commandForOpening = value;
        }

        public string CommandForClosing
        {
            get => _commandForClosing;
            set => _commandForClosing = value;
        }

        private double _windowWidth = 450;

        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                NotifyOfPropertyChange(() => WindowWidth);
            }
        }

        private string _executeOnCloseTime = "0";

        public string ExecuteOnCloseTime
        {
            get => _executeOnCloseTime;
            set
            {
                _executeOnCloseTime = value;
                SetExecuteCloseTimeForItemVM(GetIntergerByString(value));
            }
        }

        private string _executeOnOpenTime = "0";

        public string ExecuteOnOpenTime
        {
            get => _executeOnOpenTime;
            set
            {
                _executeOnOpenTime = value;
                SetExecuteOpenTimeForItemVM(GetIntergerByString(value));
            }
        }

        private void SetExecuteOpenTimeForItemVM(int time)
        {
            foreach (var vm in _tradingDetailList)
            {
                vm.ExecuteOnOpenTime = time;
            }
        }

        private void SetExecuteCloseTimeForItemVM(int time)
        {
            foreach (var vm in _tradingDetailList)
            {
                vm.ExecuteOnCloseTime = time;
            }
        }

        private int GetIntergerByString(string value)
        {
            int number;
            if (!int.TryParse(value, out number))
                return 0;
            return number;
        }
    }
}