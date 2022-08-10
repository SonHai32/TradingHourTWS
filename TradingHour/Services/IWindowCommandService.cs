using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Services
{
    public delegate void HandleCommandRun(string command);

    public delegate void HandleCommandRunError(string command);

    public interface IWindowCommandService
    {
        event HandleCommandRun OnCommandExecuted;

        event HandleCommandRun OnCommandError;

        void ExecuteCommand(string command);
    }
}