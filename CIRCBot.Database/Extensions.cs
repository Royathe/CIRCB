using System;

namespace CIRCBot.Sql
{
    public static class Extensions
    {
        /// <summary>
        /// Converts an object to an SQL parameter string. Booleans converted to "1" and "0".
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="obj">Object to convert</param>
        /// <returns>Object as an SQL string</returns>
        public static string ToSQLString<T>(this T obj)
        {
            if (obj is bool)
            {
                return (bool)(object)obj ? "1" : "0";
            }
            else if (obj is DateTime)
            {
                DateTime time = (DateTime)(object)obj;
                return "'" + time.Year + "-" + time.Month + "-" + time.Day + "'";
            }
            return obj.ToString();
        }
    }
}
