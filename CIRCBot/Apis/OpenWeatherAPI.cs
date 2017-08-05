using System;
using System.Collections.Generic;
using System.Linq;
using CIRCBot.Libraries;
using Newtonsoft.Json;

namespace CIRCBot.Apis
{
    /// <summary>
    /// Retrieves weather data through the OpenWeather api: 
    /// https://openweathermap.org/api
    /// </summary>
    class OpenWeatherAPI
    {

        #region Private constants/readonly accessors

        private const string BaseAddress = "https://api.openweathermap.org/data/2.5/{0}?id={1}&units=metric&APPID={2}";

        private class Types
        {
            internal const string Weather = "weather";
            internal const string Forecast = "forecast";
        }

        private string Key { get; }

        #endregion Private constants/readonly accessors

        #region Constructors

        /// <summary>
        /// Constructor.
        /// 
        /// </summary>
        public OpenWeatherAPI()
        {
            Key = System.Configuration.ConfigurationManager.AppSettings["OpenWeatherAPI"];
        }

        #endregion Constructors

        #region Private methods

        /// <summary>
        /// Create target url
        /// </summary>
        /// <param name="type">Weather/Forecast</param>
        /// <param name="targetId">Target city ID</param>
        private string Address(string type, int targetId)
        {
            return String.Format(BaseAddress, type, targetId, Key);
        }

        /// <summary>
        /// Get the City corresponding to the given cityname and country. 
        /// If city is an empty string, returns the default city.
        /// </summary>
        private City GetCity(string cityname, string country)
        {
            City target = null;
            if (cityname == "" && country == "")
            {
                target = Cities.Get();
            }
            else if (country == "")
            {
                target = Cities.Get(cityname);
            }
            else
            {
                target = Cities.Get(cityname, country);
            }
            return target;
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// Get the next forecast after the given date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="cityname"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public Weather NextForecast(DateTime date, string cityname = "", string country = "")
        {
            City target = GetCity(cityname, country);
            if (target == null)
            {
                return null;
            }

            string forecastJSON = GM.GetJSONResponse(
                Address(Types.Forecast, target.Id)
                );

            List<Weather> forecasts = Weather.MapForecast(
                JsonConvert.DeserializeObject(forecastJSON)
                );

            Weather weather = forecasts.FirstOrDefault(x => x.Date >= date);

            weather.City = target;

            return weather;
        }

        /// <summary>
        /// Get the current weather
        /// </summary>
        /// <param name="cityname"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public Weather CurrentWeather(string cityname = "", string country = "")
        {
            City target = GetCity(cityname, country);
            if(target == null)
            {
                return null;
            }

            string weatherJSON = GM.GetJSONResponse(
                Address(Types.Weather, target.Id)
                );

            Weather weather = Weather.Map(
                JsonConvert.DeserializeObject(weatherJSON)
                );
            
            weather.City = target;
            
            return weather;
        }

        #endregion Public methods

    }
}
