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

        private SerialPort _monitorSerialPort;
        public MonitorHelper(int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int baudRate = 115200, string portName = "COM4")
        {
            _dataBits = dataBits;
            _parity = parity;
            _stopBits = stopBits;
            _baudRate = baudRate;
            _portName = portName;
            InitPort();
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
        }

        public void SendDate(string dataStr)
        {
            OpenPort();
            var messageBytes = Encoding.UTF8.GetBytes(dataStr);
            _monitorSerialPort.Write(messageBytes, 0, messageBytes.Length);
            ClosePort();
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
            _monitorSerialPort.Dispose();
            _monitorSerialPort = null;
        }
    }
}