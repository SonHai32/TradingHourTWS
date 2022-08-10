using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TradingHour.Views
{
    /// <summary>
    /// Interaction logic for ContractDetailsView.xaml
    /// </summary>
    public partial class ContractDetailsView : AdonisWindow
    {
        public ContractDetailsView()
        {
            InitializeComponent();
        }

        private void AdonisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = 0;
        }
    }
}