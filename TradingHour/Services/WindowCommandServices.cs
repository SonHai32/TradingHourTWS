using System;
using System.Diagnostics;

namespace TradingHour.Services
{
    public class WindowCommandServices : IWindowCommandService
    {
        private ProcessStartInfo _processStartInfo;

        public event HandleCommandRun OnCommandExecuted;

        public event HandleCommandRun OnCommandError;

        public WindowCommandServices()
        {
            _processStartInfo = new ProcessStartInfo();
            _processStartInfo.WorkingDirectory = @"C:\Windows\System32";
            _processStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
        }

        public void ExecuteCommand(string command)
        {
            try
            {
                Process process = new Process();
                _processStartInfo.Arguments = "/k " + command;
                process.StartInfo = _processStartInfo;
                process.Start();
                OnCommandExecuted?.Invoke(command);
            }
            catch (Exception ex)
            {
                OnCommandError?.Invoke(ex.Message);
            }
        }
    }
}