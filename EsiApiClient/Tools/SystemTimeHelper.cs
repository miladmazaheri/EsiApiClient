using System;
using System.Linq;
using System.Runtime.InteropServices;
using DNTPersianUtils.Core;

namespace IPAClient.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;



    }


    public static class SystemTimeHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        public static bool SetSystemTime(DateTime dt)
        {
            dt = dt.ToUniversalTime();
            SYSTEMTIME st = new SYSTEMTIME
            {
                wYear = (short)dt.Year,
                wMonth = (short)dt.Month,
                wDay = (short)dt.Day,
                wHour = (short)dt.Hour,
                wMinute = (short)dt.Minute,
                wSecond = (short)dt.Second
            };
            return SetSystemTime(ref st);
        }


        public static string ToServerDateFormat(this DateTime dt)
        {
            var pDt = dt.ToPersianYearMonthDay();
            return $"{pDt.Year:0000}{pDt.Month:00}{pDt.Day:00}";
        }

        public static TimeSpan? ToTimeSpan(this string val)
        {
            if (string.IsNullOrWhiteSpace(val) || val.Length != 4 || val.Any(x => !char.IsDigit(x)))
            {
                return null;
            }
            return new TimeSpan(hours: int.Parse(val.Substring(0, 2)), minutes: int.Parse(val.Substring(2, 2)), 0);
        }

        public static bool IsNextDay(this DateTime dt)
        {
            return dt.Date.AddDays(1) == DateTime.Now.Date;
        }
        public static bool IsMinutePassed(this DateTime dt,int minute)
        {
            return dt.AddMinutes(minute) < DateTime.Now;
        }


        public static string CurrentPersinaFullDateTime()
        {
            var now = DateTime.Now;
            return now.ToString("HH:mm") + " " + now.ToPersianDateTextify();
        }

        public static string CurrentPersinaFullDate()
        {
            var now = DateTime.Now;
            return  now.ToPersianDateTextify();
        }

        public static string ToNormalJsonString(this string input)
        {
            return input?.Replace("\\n", string.Empty).Replace("\n", string.Empty).Replace("\\r\\n", string.Empty).Replace("\r\n", string.Empty);
        }
    }
}
