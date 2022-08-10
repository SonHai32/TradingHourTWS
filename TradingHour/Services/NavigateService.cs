using System.Windows;
using TradingHour.Helpers;
using TradingHour.ViewModels;
using TradingHour.Views;

namespace TradingHour.Services
{
    public class NavigateService : INavigateService
    {
        private Debouncer _navigateDebouncer = new Debouncer();
        private ContractDetailsView _contractDetailsView;
        private ContractDetailsView ContractDetailsView => _contractDetailsView ?? (_contractDetailsView = new ContractDetailsView());

        public NavigateService()
        {
        }

        public void NavigateWindow(object parameter)
        {
            if (parameter is string contractDetails)
            {
                ContractDetailsViewModel contractDetailsViewModel = new ContractDetailsViewModel(contractDetails);
                ContractDetailsView.DataContext = contractDetailsViewModel;
                if (!ContractDetailsView.IsActive)
                {
                    ContractDetailsView.Show();
                }
                else
                    ContractDetailsView.UpdateLayout();
            }
        }
    }
}