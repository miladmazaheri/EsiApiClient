using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using DNTPersianUtils.Core;

namespace IPAClient.Tools
{
    public class RfidHelper : IDisposable
    {
        private readonly Action<uint, bool, bool, string> _dataReceivedAction;
        private readonly SerialPort _serialPort1;
        private volatile bool _isFree = true;
        public bool IsConnected => _serialPort1?.IsOpen ?? false;
        public RfidHelper(int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int baudRate = 19200, string portName = "COM4", Action<uint, bool, bool, string> dataReceivedAction = null)
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

        public void SetIsBusy(bool isBusy)
        {
            _isFree = !isBusy;
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_isFree)
            {
                var txt = string.Empty;
                do
                {
                    txt += _serialPort1.ReadLine();
                } while (_serialPort1.BytesToRead != 0);

                if (string.IsNullOrWhiteSpace(txt))
                {
                    return;
                };

                var parsedData = ParsDate(txt);
                _dataReceivedAction.Invoke(parsedData.number, parsedData.isActive, parsedData.isExp, parsedData.expDate);
            }
        }

        private (uint number, bool isActive, bool isExp, string expDate) ParsDate(string txt)
        {
            var parts = txt.Split("\r").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            uint number = 0;
            var isExp = false;
            var isActive = false;
            var expDate = string.Empty;
            foreach (var part in parts)
            {
                if (part.StartsWith("USERCODE"))
                {
                    number = uint.Parse(part.Split(":")[1]);
                }
                else if (part.StartsWith("EXPDATE") && number.ToString().Length > 6) // فقط برای پرسنل غیر فولاد که کد پرسنلی بزرگتر از 6 رقم است
                {
                    var strDate = part.Split(":")[1];
                    var dateParts = strDate.Split("/").Select(int.Parse).ToList();
                    var year = dateParts[0] < 90 ? 1400 + dateParts[0] : 1300 + dateParts[0];
                    var persianDate = new PersianDay(year, dateParts[1], dateParts[2]);
                    isExp = persianDate.PersianDayToGregorian().Date <= DateTime.Now.Date;
                    expDate = strDate;
                }
                else if (!part.StartsWith("CARDID"))
                {
                    isActive = part == "ACTIVE";
                }
            }

            return (number, isActive, isExp, expDate);
        }


        public void Dispose()
        {
            _serialPort1.Close();
        }
    }
}