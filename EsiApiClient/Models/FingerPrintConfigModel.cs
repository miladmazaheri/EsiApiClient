using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPAClient.Models
{
    public class SerialPortConfigModel
    {
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int BaudRate { get; set; }
        public string PortName { get; set; }

        public SerialPortConfigModel()
        {
            
        }

        public SerialPortConfigModel(int dataBits, Parity parity, StopBits stopBits, int baudRate, string portName)
        {
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            BaudRate = baudRate;
            PortName = portName;
        }
    }
}
