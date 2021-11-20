using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IPAClient.Models;

namespace IPAClient.Windows
{
    /// <summary>
    /// Interaction logic for wndConfirmConfig.xaml
    /// </summary>
    public partial class wndConfirmConfig : Window
    {
        private readonly ConfigModel _configModel;
        public wndConfirmConfig(ConfigModel configModel)
        {
            InitializeComponent();
            _configModel = configModel;
            lblError.Visibility = Visibility.Hidden;
        }


        private void BtnConfirm_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            lblError.Visibility = Visibility.Hidden;
            _configModel.Restaurant_Cod = txtRestaurant_Cod.Text;
            _configModel.Device_Category = txtDevice_Category.Text;
            _configModel.Device_Type = txtDevice_Type.Text;
            _configModel.Device_Name = txtDevice_Name.Text;
            _configModel.Device_Cod = txtDevice_Cod.Text;
            _configModel.IP = txtIP.Text;
            _configModel.Num_Queue = txtNum_Queue.Text;
            _configModel.WebServiceUrl = txtWebServiceUrl.Text;

            if (_configModel.IsValid())
            {
                var res = new wndCompleteConfig(_configModel).ShowDialog();
                if (res == true)
                {
                    DialogResult = true;
                }
            }
            else
            {
                lblError.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancel_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void WndConfirmConfig_OnLoaded(object sender, RoutedEventArgs e)
        {
            txtRestaurant_Cod.Text = _configModel.Restaurant_Cod;
            txtDevice_Category.Text = _configModel.Device_Category;
            txtDevice_Type.Text = _configModel.Device_Type;
            txtDevice_Name.Text = _configModel.Device_Name;
            txtDevice_Cod.Text = _configModel.Device_Cod;
            txtIP.Text = _configModel.IP;
            txtNum_Queue.Text = _configModel.Num_Queue;
            txtWebServiceUrl.Text = _configModel.WebServiceUrl;
            SetBackGroundImage(_configModel.CheckOnline);
        }

        private void SetBackGroundImage(bool checkOnline)
        {
            grdMail.Background = new ImageBrush(new BitmapImage(new Uri(checkOnline ? @"pack://application:,,,/EsiApiClient;component/Images/3.png" : @"pack://application:,,,/EsiApiClient;component/Images/2.png")));
        }

        private void BtnCheckOnline_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _configModel.CheckOnline = !_configModel.CheckOnline;
            SetBackGroundImage(_configModel.CheckOnline);
        }
    }
}
