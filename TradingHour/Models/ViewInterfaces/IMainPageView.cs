using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHour.Models.ViewInterfaces
{
    public interface IMainPageView
    {
        void ScrollToButtonLogMessageBox(int offset);

        void OpenContractDetailPopup();

        void CloseConfigPopup();

        void OpenConfigPopup();
    }
}