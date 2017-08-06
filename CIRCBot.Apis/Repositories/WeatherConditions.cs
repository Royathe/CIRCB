using System;
using System.Collections.Generic;

namespace CIRCBot.Apis.Repositories
{
    static class WeatherConditions
    {

        public static string Get(string id)
        {
            if (WeatherConditionRepository.Codes.ContainsKey(id))
            {
                return WeatherConditionRepository.Codes[id];
            }
            return String.Empty;
        }

        public static void Load()
        {
            WeatherConditionRepository.Load();
        }

        private class WeatherConditionRepository
        {
            private static Dictionary<string, string> _codes { get; set; }

            internal static Dictionary<string, string> Codes
            {
                get
                {
                    if(_codes == null)
                    {
                        Load();
                    }
                    return _codes;
                }
            }

            /// <summary>
            /// Adds the keys to the dictionary with the given description
            /// </summary>
            private static void Add(string desc, params int[] ids)
            {
                foreach(int id in ids)
                {
                    if (_codes.ContainsKey(id.ToString()))
                    {
                        _codes[id.ToString()] = desc;
                    }
                    else
                    {
                        _codes.Add(id.ToString(), desc);
                    }
                }
            }

            private static void AddGroup(string desc, int centuryGroup)
            {
                for(int id = centuryGroup; id < centuryGroup + 99; id++)
                {
                    _codes.Add(id.ToString(), desc);
                }
            }

            internal static void Load()
            {
                _codes = new Dictionary<string, string>();

                AddGroup("Ukkosmyrsky", 200);

                AddGroup("Tihkusadetta", 300);

                AddGroup("Sadetta", 500);
                Add("Raskasta sadetta", 502, 503, 504, 522, 531);

                AddGroup("Lumisadetta", 600);
                Add("Räntäsadetta", 611, 612);
                Add("Raskasta lumisadetta", 602, 622);

                Add("Sumuista", 701, 741);

                Add("Selkeä taivas", 800);
                Add("Vähän pilvistä", 801);
                Add("Hajanaisia pilviä", 802, 803);
                Add("Pilvistä", 804);

                Add("Tuulista", 905);
                Add("Rakeita", 906);

                Add("Todella tuulista", 957, 958, 959);
                Add("Myrskyistä", 960, 961);
            }
        }
    }
}
