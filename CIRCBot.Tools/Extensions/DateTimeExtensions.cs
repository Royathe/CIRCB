using System;

namespace CIRCBot
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns a date with a time, without the seconds
        /// </summary>
        public static string ToShortDateTimeString(this DateTime dt)
        {
            string str = dt.ToShortDateString() + " " + dt.ToShortTimeString();
            return str;
        }
    }
}
