using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

namespace IPAClient.Tools
{
    public class MonitorHelper : IDisposable
    {
        private readonly int _dataBits;
        private readonly Parity _parity;
        private readonly StopBits _stopBits;
        private readonly int _baudRate;
        private readonly string _portName;
        private readonly Action _commandOne;
        private readonly Action _commandTwo;
        private readonly Action _commandThree;
        private readonly Action _commandFour;
        private SerialPort _monitorSerialPort;
        public MonitorHelper(int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int baudRate = 115200, string portName = "COM4", Action commandOne = null, Action commandTwo = null, Action commandThree = null, Action commandFour = null)
        {
            _dataBits = dataBits;
            _parity = parity;
            _stopBits = stopBits;
            _baudRate = baudRate;
            _portName = portName;
            InitPort();
            _commandOne = commandOne;
            _commandTwo = commandTwo;
            _commandThree = commandThree;
            _commandFour = commandFour;
        }


        private void InitPort()
        {
            _monitorSerialPort ??= new SerialPort
            {
                DataBits = _dataBits,
                Parity = _parity,
                StopBits = _stopBits,
                BaudRate = _baudRate,
                PortName = _portName
            };
            _monitorSerialPort.DataReceived += MonitorSerialPort_DataReceived;
            OpenPort();
        }

        private async void MonitorSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var command = _monitorSerialPort.ReadLine();
            switch (command)
            {
                case "1"://دستور اتمام غذا
                    _commandOne?.Invoke();
                    break;
                case "2"://در حال آماده سازی
                    _commandTwo?.Invoke();
                    break;
                case "3":
                    _commandThree?.Invoke();
                    break;
                case "4":
                    _commandFour?.Invoke();
                    break;
            }
        }

        public void SendDate(string dataStr)
        {
            dataStr = dataStr.Replace("\r\n", string.Empty).Replace("\n", string.Empty) + Environment.NewLine;
            var messageBytes = Encoding.UTF8.GetBytes(dataStr);
            _monitorSerialPort.Write(messageBytes, 0, messageBytes.Length);
        }

        private void OpenPort()
        {
            if (!_monitorSerialPort.IsOpen)
            {
                _monitorSerialPort.Open();
            }
        }

        private void ClosePort()
        {
            if (_monitorSerialPort.IsOpen)
            {
                _monitorSerialPort.Close();
            }
        }

        public void Dispose()
        {
            ClosePort();
            _monitorSerialPort.Dispose();
            _monitorSerialPort = null;
        }
    }
}