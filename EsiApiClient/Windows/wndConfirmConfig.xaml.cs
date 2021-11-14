﻿using System;
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
using EsiApiClient.Models;

namespace EsiApiClient.Windows
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

        }


        private void BtnConfirm_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            

        }

        private void BtnCancel_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("NOK");
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
        }
    }
}
