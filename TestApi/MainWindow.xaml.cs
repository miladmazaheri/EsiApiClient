using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestApi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        HttpClient httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var key = "Authorization";
                if (httpClient.DefaultRequestHeaders.Any(x => x.Key == key))
                {
                    httpClient.DefaultRequestHeaders.Remove(key);
                }
                httpClient.DefaultRequestHeaders.Add(key, txtToken.Text);


                if (!txtBaseAddress.Text.EndsWith("/")) txtBaseAddress.Text += "/";
                var response = await httpClient.PostAsync($"{txtBaseAddress.Text}osb/namfood/restservices/MainInfo_Send_Lookup_Data_Fun", null);
                var resAsString = await response.Content.ReadAsStringAsync();
                txtRes.Text = resAsString;
            }
            catch (Exception ex)
            {
                txtRes.Text = ex.Message;
            }
        }
    }
}
