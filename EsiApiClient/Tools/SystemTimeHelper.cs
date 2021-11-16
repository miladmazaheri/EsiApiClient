using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DNTPersianUtils.Core;

namespace EsiApiClient.Tools
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



    }
}
