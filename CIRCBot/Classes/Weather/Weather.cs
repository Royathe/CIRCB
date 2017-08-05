using System;
using System.Collections.Generic;

namespace CIRCBot
{
    /// <summary>
    /// Constructed Weather class
    /// </summary>
    public class Weather
    {

        #region Public accessors

        public City City { get; set; }

        public DateTime Date { get; set; }

        public string WeatherDescription { get; set; }

        public string FeelsLike { get; set; }

        #region JSON mapped accessors

        [JSONMap("main", "temp")]
        public string Temperature { get; set; }
        
        [JSONMap("main", "pressure")]
        public string Pressure { get; set; }
        
        [JSONMap("main", "humidity")]
        public string Humidity { get; set; }
        
        [JSONMap("wind", "speed")]
        public string WindSpeed { get; set; }
        
        [JSONMap("clouds", "all")]
        public string Cloudiness { get; set; }

        #endregion JSON mapped accessors

        #endregion Public accessors

        #region Public static mappers

        /// <summary>
        /// Maps a list of forecasts into a list of weather objects
        /// </summary>
        public static List<Weather> MapForecast(dynamic json)
        {
            List<Weather> list = new List<Weather>();

            foreach (dynamic weather in json.list)
            {
                list.Add(
                    Weather.Map(weather)
                    );
            }
            return list;
        }

        /// <summary>
        /// Maps a json to a new Weather object
        /// </summary>
        public static Weather Map(dynamic json)
        {
            Weather weather = GM.JSONMapper<Weather>(json);

            weather.WeatherDescription = " | ";
            foreach (var weatherJSON in json.weather)
            {
                string desc = Libraries.WeatherConditions.Get(((object)weatherJSON.id).ToString());

                weather.WeatherDescription += desc + ", ";
            }
            weather.WeatherDescription = weather.WeatherDescription.Substring(0, weather.WeatherDescription.Length - 2);

            if (json.dt != null)
            {
                weather.Date = GM.UnixTimeStampToDateTime((int)json.dt);
            }
            else
            {
                weather.Date = DateTime.Now;
            }

            weather.FeelsLike = CIRCBot.Weather.HeatIndex.ApparentTemperature(
                weather.Temperature,
                weather.Humidity,
                weather.WindSpeed
                );

            return weather;
        }

        #endregion Public static mappers

        #region Sub classes

        /// <summary>
        /// Subclass for calculating apparent temperature
        /// </summary>
        public static class HeatIndex
        {
            /// <summary>
            /// Calculates the Water vapour pressure (hPa) [humidity]
            /// </summary>
            /// <param name="Ta">Dry bulb temperature C</param>
            /// <param name="rh">Relative humidity</param>
            /// <returns>Water vapour pressure (e)</returns>
            private static double CalculateE(float Ta, float rh)
            {
                double s1 = rh / 100;

                double s2 = 6.105f;

                double s3 = Math.Exp(((17.27f * Ta) / (237.7f + Ta)));

                return (s1 * s2 * s3);
            }

            private static float toFloat(string value)
            {
                if (value.Contains("."))
                {
                    value = value.Replace(".", ",");
                }

                if (!value.Contains(","))
                {
                    value += ",00";
                }
                return float.Parse(value);
            }

            public static string ApparentTemperature(string temp, string humidity, string windspeed)
            {
                string apparentTemperature = "";


                float Ta = toFloat(temp);

                float rh = toFloat(humidity);

                float ws = toFloat(windspeed);

                float q = 0f;

                double e = CalculateE(Ta, rh);

                // Apparent Temperature
                double AT = 0.0f;

                AT = Ta + (0.348f * e) - (0.70f * ws) + (0.70f * (q / (ws + 10.0f))) - 4.25f;

                apparentTemperature = Math.Round(AT, 2) + Library.CENTIGRADE;

                apparentTemperature = apparentTemperature.Replace(',', '.');

                return apparentTemperature;
            }
        }

        #endregion Sub classes

    }
}
