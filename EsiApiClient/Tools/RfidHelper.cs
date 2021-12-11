using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNTPersianUtils.Core;

namespace IPAClient.Tools
{
    public class RfidHelper : IDisposable
    {
        private readonly Func<uint, bool, bool, Task<bool>> _dataReceivedAction;
        private readonly SerialPort _serialPort1;
        private volatile bool _isFree = true;
        public bool IsConnected => _serialPort1?.IsOpen ?? false;
        public RfidHelper(int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int baudRate = 19200, string portName = "COM4", Func<uint, bool, bool, Task<bool>> dataReceivedAction = null)
        {
            _dataReceivedAction = dataReceivedAction;
            _serialPort1 = new SerialPort
            {
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                BaudRate = baudRate,
                PortName = portName
            };
            _serialPort1.DataReceived += SerialPort1_DataReceived;
            _serialPort1.Open();
        }

        private async void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_isFree)
                {
                    _isFree = false;
                    var txt = string.Empty;
                    do
                    {
                        txt += _serialPort1.ReadLine();
                    } while (_serialPort1.BytesToRead != 0);

                    if (string.IsNullOrWhiteSpace(txt))
                    {
                        _isFree = true;
                        return;
                    };

                    var parsedData = ParsDate(txt);
                    _isFree = (_dataReceivedAction == null || await _dataReceivedAction.Invoke(parsedData.number, parsedData.isActive, parsedData.isExp));
                }
            }
            catch (Exception)
            {
                _isFree = true;
            }
        }

        private (uint number, bool isActive, bool isExp) ParsDate(string txt)
        {
            var parts = txt.Split("\r").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            uint number = 0;
            var isExp = false;
            var isActive = false;
            foreach (var part in parts)
            {
                if (part.StartsWith("USERCODE"))
                {
                    number = uint.Parse(part.Split(":")[1]);
                }
                else if (part.StartsWith("EXPDATE"))
                {
                    var strDate = part.Split(":")[1];
                    var dateParts = strDate.Split("/").Select(int.Parse).ToList();
                    var year = dateParts[0] < 90 ? 1400 + dateParts[0] : 1300 + dateParts[0];
                    var persianDate = new PersianDay(year, dateParts[1], dateParts[2]);
                    isExp = persianDate.PersianDayToGregorian().Date <= DateTime.Now.Date;
                }
                else if (!part.StartsWith("CARDID"))
                {
                    isActive = part == "ACTIVE";
                }
            }

            return (number, isActive, isExp);
        }


        public void Dispose()
        {
            _serialPort1.Close();
        }
    }
}