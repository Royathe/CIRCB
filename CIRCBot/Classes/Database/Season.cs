using System;
using System.Collections.Generic;

namespace CIRCBot
{
    class Season : IComparable<Season>
    {
        public int SeasonId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? Winner { get; set; }

        public string Losers { get; set; }

        public List<User> LosersList { get; set; }

        public string SettledUsers { get; set; }

        public List<User> SettledUsersList { get; set; }

        public void ParseData()
        {
            LosersList = new List<User>();
            SettledUsersList = new List<User>();

            if(Losers != null)
            {
                var loserIds = Losers.Split(' ');
                parseToList(loserIds, LosersList);
            }

            if(SettledUsers != null)
            {
                var settledIds = SettledUsers.Split(' ');
                parseToList(settledIds, SettledUsersList);
            }
        }

        private void parseToList(string[] ids, List<User> users)
        {
            foreach (var s in ids)
            {
                int id;
                if (int.TryParse(s, out id))
                {
                    users.Add(Users.Get(id));
                }
            }
        }

        public int CompareTo(Season other)
        {
            if(other.SeasonId > SeasonId)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
