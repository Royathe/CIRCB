using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace CIRCBot.Libraries
{
    static class Cities
    {

        #region Constants

        /// <summary>
        /// Country code of the default country.
        /// </summary>
        public const string DefaultCountry = "FI";

        /// <summary>
        /// Name of the default city.
        /// </summary>
        public const string DefaultCity = "kerava";

        #endregion Constants

        #region Readonly accessors

        /// <summary>
        /// Number of loaded cities
        /// </summary>
        public static int CityCount
        {
            get
            {
                return CityRepository.CityCount;
            }
        }

        /// <summary>
        /// Number of loaded countries
        /// </summary>
        public static int CountryCount
        {
            get
            {
                return CityRepository.CountryCount;
            }
        }

        /// <summary>
        /// Default country city list
        /// </summary>
        private static Dictionary<string, City> Default
        {
            get
            {
                return CityRepository.CitiesByCountry[DefaultCountry];
            }
        }

        #endregion Readonly accessors

        #region Public methods

        /// <summary>
        /// Gets the default city.
        /// </summary>
        public static City Get()
        {
            return CityRepository.CitiesByCountry[DefaultCountry][DefaultCity];
        }

        /// <summary>
        /// Gets the city with the given name.
        /// </summary>
        public static City Get(string cityName)
        {
            cityName = cityName.ToLower();
            if (Default.Keys.Contains(cityName))
            {
                return Default[cityName];
            }

            return CityRepository.Cities.FirstOrDefault(x => x.Name == cityName);
        }

        /// <summary>
        /// Gets the city from the specified country.
        /// </summary>
        public static City Get(string cityName, string country)
        {
            country = country.ToUpper();
            cityName = cityName.ToLower();
            if (CityRepository.CitiesByCountry.Keys.Contains(country))
            {
                if (CityRepository.CitiesByCountry[country].Keys.Contains(cityName))
                {
                    return CityRepository.CitiesByCountry[country][cityName];
                }
            }
            return null;
        }

        /// <summary>
        /// Loads the City Repository.
        /// </summary>
        public static void Load()
        {
            CityRepository.Load();
        }

        #endregion Public methods

        #region Subclasses

        /// <summary>
        /// Contains the raw list of cities and the country-sorted dictionary
        /// </summary>
        private static class CityRepository
        {
            
            #region Private accessors

            /// <summary>
            /// Cities grouped by country.
            /// </summary>
            private static Dictionary<string, Dictionary<string, City>> _citiesByCountry { get; set; }

            /// <summary>
            /// Raw list of cities.
            /// </summary>
            private static List<City> _cities { get; set; }

            #endregion Private accessors

            #region Internal accessors

            /// <summary>
            /// Number of loaded cities
            /// </summary>
            internal static int CityCount
            {
                get
                {
                    if(_cities == null)
                    {
                        Load();
                    }
                    return _cities.Count;
                }
            }

            /// <summary>
            /// Number of loaded countries
            /// </summary>
            internal static int CountryCount
            {
                get
                {
                    if(_citiesByCountry == null)
                    {
                        Load();
                    }
                    return _citiesByCountry.Keys.Count;
                }
            }

            /// <summary>
            /// Getter for the country-grouped city dictionary.
            /// </summary>
            internal static Dictionary<string, Dictionary<string, City>> CitiesByCountry
            {
                get
                {
                    if (_citiesByCountry == null)
                    {
                        Load();
                    }
                    return _citiesByCountry;
                }
            }

            /// <summary>
            /// Getter for the raw city list.
            /// </summary>
            internal static List<City> Cities
            {
                get
                {
                    if (_cities == null)
                    {
                        Load();
                    }
                    return _cities;
                }
            }

            #endregion Internal accessors
            
            /// <summary>
            /// Loads the city list from the JSON file.
            /// </summary>
            internal static void Load()
            {
                if (_citiesByCountry == null)
                {
                    // Init the country-grouped city dictionary
                    _citiesByCountry = new Dictionary<string, Dictionary<string, City>>();

                    // Read the city list json file
                    _cities = JsonConvert.DeserializeObject<List<City>>(File.ReadAllText(Library.JSON_City_List));

                    // Sort cities to dictionary
                    foreach (var city in _cities)
                    {
                        // Add country to the dictionary if it doesn't exist in it yet
                        if (!_citiesByCountry.Keys.Contains(city.Country))
                        {
                            _citiesByCountry.Add(
                                city.Country.ToUpper(),
                                new Dictionary<string, City>()
                                );
                        }

                        // Add city to the country's dictionary. Only 1 of each city allowed (There are some near identical duplicates).
                        if (!_citiesByCountry[city.Country.ToUpper()].Keys.Contains(city.Name.ToLower()))
                        {
                            _citiesByCountry[city.Country].Add(city.Name.ToLower(), city);
                        }
                    }
                }
            }
        }

        #endregion Subclasses

    }
}
