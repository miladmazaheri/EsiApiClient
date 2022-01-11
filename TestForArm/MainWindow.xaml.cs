using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestForArm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        public MainWindow()
        {
            InitializeComponent();

            var path = Directory.GetCurrentDirectory() + @"\MonitorDtoSample.json";
            if (File.Exists(path))
            {
                txtJson.Text = File.ReadAllText(path);
            }

        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialPort = new SerialPort(txtPortName.Text, int.Parse(txtBaudRate.Text));
                _serialPort.Open();
                btnConnect.Content = "Connected";
            }
            catch (Exception)
            {
                btnConnect.Content = "Unable To Connect";
            }

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen && !string.IsNullOrWhiteSpace(txtJson.Text))
            {
                try
                {
                    if (rdbW.IsChecked ?? true)
                    {
                        var messageBytes = Encoding.UTF8.GetBytes(txtJson.Text.Replace("\r\n", " ") + "\n");
                        _serialPort.Write(messageBytes, 0, messageBytes.Length);
                    }
                    else
                    {
                        _serialPort.WriteLine(txtJson.Text.Replace("\r\n", " "));
                    }
                    btnSend.Content = "Data Sent";
                }
                catch (Exception)
                {

                    btnSend.Content = "Error On Sending Data";
                }
            }
        }
    }
}
