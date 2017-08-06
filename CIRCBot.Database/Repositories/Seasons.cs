using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot
{
    public static class Seasons
    {
        private static List<Season> seasons = new List<Season>();

        private static Season current;

        public static Season[] All
        {
            get
            {
                return seasons.OrderByDescending(x => x.SeasonId).ToArray();
            }
        }

        public static Season Current
        {
            get
            {
                return current;
            }
        }

        public static void Add(Season season)
        {
            if(current == null || season.SeasonId > current.SeasonId)
            {
                current = season;
            }
            seasons.Add(season);
        }

        /// <summary>
        /// Empty the repository.
        /// </summary>
        public static void Reset()
        {
            seasons = new List<Season>();
        }
    }
}
