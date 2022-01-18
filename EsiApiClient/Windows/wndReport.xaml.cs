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
using DataLayer.Dtos;
using DataLayer.Services;

namespace IPAClient.Windows
{
    /// <summary>
    /// Interaction logic for wndReport.xaml
    /// </summary>
    public partial class wndReport : Window
    {
        private readonly ReservationService _reservationService;
        public wndReport()
        {
            InitializeComponent();
            _reservationService = new ReservationService();
        }

        private async void WndReport_OnLoaded(object sender, RoutedEventArgs e)
        {
            grdReport.ItemsSource = await _reservationService.GetReportAsync();
        }
    }
}
