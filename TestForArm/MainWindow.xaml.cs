using System;
using System.Collections.Generic;
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
        private SerialPort serialPort1;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void SerialPort1OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var txt = string.Empty;
                do
                {
                    txt += serialPort1.ReadLine();
                } while (serialPort1.BytesToRead !=0);

                Dispatcher.Invoke((Action)(() => TxtReadLine.Text = txt));

                //var txt = serialPort1.ReadLine();
                //var a = serialPort1.BytesToRead;
                //var txt1 = serialPort1.ReadLine();
                //var a1 = serialPort1.BytesToRead;
                //var txt2 = serialPort1.ReadLine();
                //var a2 = serialPort1.BytesToRead;
                //var txt3 = serialPort1.ReadLine();
                //var a3 = serialPort1.BytesToRead;
                //var txt4 = serialPort1.ReadLine();
                //var a4 = serialPort1.BytesToRead;
                //var txt5 = serialPort1.ReadLine();


                //var in_data_cnt = serialPort1.BytesToRead;
                //var rx_byte = new byte[in_data_cnt];
                //serialPort1.Read(rx_byte, 0, in_data_cnt);
                //Dispatcher.Invoke((Action)(() =>
                //{
                //    TxtBytes.Text = rx_byte.ToString();
                //    TxtDataHex.Text = ByteToHexString(rx_byte);
                //    TxtDataAnsi.Text = ByteToAnsiString(rx_byte);
                //    TxtDataUtf8.Text = ByteToUtf8String(rx_byte);
                //    TxtDataAscci.Text = ByteToASCIIString(rx_byte);
                //}));
            }
            catch (Exception exception)
            {
                Dispatcher.Invoke((Action)(() => TxtDataUtf8.Text = exception.Message));
            }
        }

        //convert byte to utf8 string
        private string ByteToUtf8String(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
        //convert byte to ansi string
        private string ByteToAnsiString(byte[] data)
        {
            return Encoding.Default.GetString(data);
        }

        //convert byte to hex string
        private string ByteToHexString(byte[] data)
        {
            return BitConverter.ToString(data);
        }

        private string ByteToASCIIString(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            InitPort();
        }

        private void InitPort()
        {
            try
            {

                serialPort1?.DiscardInBuffer();
                serialPort1?.Close();
                serialPort1?.Dispose();
                if (serialPort1 != null)
                {
                    serialPort1.DataReceived -= SerialPort1OnDataReceived;
                }
                serialPort1 = null;
                serialPort1 = new SerialPort
                {
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    BaudRate = 19200,
                    PortName = "COM3"
                };
                serialPort1.DataReceived += SerialPort1OnDataReceived;
                serialPort1.Open();
                button.Content = "Connected To COM4";
            }
            catch (Exception ex)
            {
                TxtBytes.Text = ex.Message;
            }
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            TxtReadLine.Text = string.Empty;
        }
    }
}
