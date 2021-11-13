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
    }
}
