using System.Windows;
using System.Windows.Input;
using TradingHour.ViewModels;
using System.Text.RegularExpressions;
using TradingHour.Models.ViewInterfaces;
using System.Windows.Controls;
using System.Windows.Media;

namespace TradingHour.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window, IMainPageView
    {
        private readonly MainViewModel ViewModel;

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel(this);
            this.DataContext = ViewModel;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^-?\d*\.{0,1}\d+$");
            e.Handled = regex.IsMatch(e.Text) || e.Text.Equals("-");
        }

        public void ScrollToButtonLogMessageBox(int offset)
        {
            if (VisualTreeHelper.GetChildrenCount(LogMessagesBox) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(LogMessagesBox, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        public void OpenContractDetailPopup()
        {
        }

        public void CloseConfigPopup()
        {
            ConfigPopup.IsOpen = false;
            ConfigPopup.StaysOpen = false;
        }

        public void OpenConfigPopup()
        {
            ConfigPopup.IsOpen = true;
            ConfigPopup.StaysOpen = true;
        }
    }
}