using System;
using System.IO.Ports;

namespace IPAClient.Tools
{
    public class FingerPrintHelper : IDisposable
    {
        private readonly Action<uint> _dataReceivedAction;
        private int in_data_cnt;
        private byte[] rx_byte;
        private readonly byte[] rx_bytes = new byte[50];
        private int rx_cnt;
        private short rx_stat;
        private readonly SerialPort serialPort1;
        private readonly byte[] tmp_in_ch = new byte[2];
        private uint USR_ID;
        public bool IsConnected => serialPort1?.IsOpen ?? false;
        public FingerPrintHelper(int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int baudRate = 115200, string portName = "COM3", Action<uint> dataReceivedAction = null)
        {
            _dataReceivedAction = dataReceivedAction;
            serialPort1 = new SerialPort
            {
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                BaudRate = baudRate,
                PortName = portName
            };
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            in_data_cnt = serialPort1.BytesToRead;
            rx_byte = new byte[in_data_cnt];

            serialPort1.Read(rx_byte, 0, in_data_cnt);

            for (var i = 0; i < in_data_cnt; i++)
            {
                var res = my_parser(rx_byte[i]);
                if (res != 0)
                    _dataReceivedAction?.Invoke(res);
            }
                
        }

        private uint my_parser(byte in_ch)
        {
            tmp_in_ch[0] = tmp_in_ch[1];
            tmp_in_ch[1] = in_ch;
            if (rx_stat == 1)
            {
                rx_bytes[rx_cnt++] = in_ch;
                if (rx_cnt == 18)
                {
                    USR_ID = rx_bytes[5];
                    USR_ID <<= 8;
                    USR_ID += rx_bytes[4];
                    USR_ID <<= 8;
                    USR_ID += rx_bytes[3];
                    USR_ID <<= 8;
                    USR_ID += rx_bytes[2];

                    if (USR_ID == 0) return uint.MaxValue;
                    return USR_ID;
                }
            }

            if (tmp_in_ch[0] == 0x02 && tmp_in_ch[1] == 0x56)
            {
                rx_cnt = 0;
                rx_stat = 1;
            }

            return 0;
        }

        public void Dispose()
        {
            serialPort1.Close();
        }
    }
}