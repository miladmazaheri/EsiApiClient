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

namespace IPAClient.Windows
{
    /// <summary>
    /// Interaction logic for wndKeyPad.xaml
    /// </summary>
    public partial class wndKeyPad : Window
    {
        public wndKeyPad()
        {
            InitializeComponent();
        }
        public string PersonnelNumber { get; private set; }
        private string _currentNum;
        private void WndKeyPad_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnNumberClick(object sender, MouseButtonEventArgs e)
        {
            var currentTxt = lblInput.Content.ToString();
            currentTxt += ((Label)sender).Tag.ToString();
            lblInput.Content = currentTxt;
        }

        private void BtnReset_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearInput();
        }

        private void ClearInput()
        {
            lblInput.Content = string.Empty;
        }
        private void BtnOk_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentNum))
            {
                var txt = lblInput.Content.ToString();
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    _currentNum = txt;
                    //TODO Check If Exist With This Num
                    ClearInput();
                    grdMain.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/IPAClient;component/Images/23.png")));
                }
            }
            else
            {
                //TODO Check PassWord
                PersonnelNumber = _currentNum;
                Close();
            }
        }


        private void BtnBack1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PersonnelNumber = null;
            Close();
        }
    }
}
