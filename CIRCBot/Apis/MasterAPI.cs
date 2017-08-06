using System;

namespace CIRCBot
{
    public static class MasterAPI
    {

        /// <summary>
        /// Gets the current time from Time.Is
        /// </summary>
        /// <returns>Current time at Time.Is</returns>
        public static DateTime CurrentTime()
        {
            var TIAPI = new Apis.TimeIsAPI();

            return TIAPI.CurrentTime();
        }

        /// <summary>
        /// Gets either the current weather, or the next forecasted weather after the given date
        /// </summary>
        public static Weather WeatherOrForecast(string city, string country, DateTime? date)
        {
            var OWAPI = new Apis.OpenWeatherAPI(System.Configuration.ConfigurationManager.AppSettings["OpenWeatherAPI"]);

            if (date.HasValue)
            {
                return OWAPI.NextForecast((DateTime)date, city, country);
            }
            else
            {
                return OWAPI.CurrentWeather(city, country);
            }
        }
    }
}
