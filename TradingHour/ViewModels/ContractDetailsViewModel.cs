namespace TradingHour.ViewModels
{
    public class ContractDetailsViewModel
    {
        public ContractDetailsViewModel(string contractDetails)
        {
            _contractDetails = contractDetails;
        }

        private string _contractDetails;

        public string ContractDetails
        {
            get => _contractDetails;
            set => _contractDetails = value;
        }
    }
}