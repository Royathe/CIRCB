using System.Collections.Generic;

namespace CIRCBot
{
    /// <summary>
    /// Library of commands
    /// </summary>
    public static class Cmd
    {

        #region Private readonly variables

        private static Dictionary<string, string> simpleCommands;

        #endregion Private readonly variables

        public const string Blackjack = "blackjack";
        public const string Niggagiggel = "niggagiggel";
        public const string Mustajaakko = "mustajaakko";
        public const string Scoreboard = "scoreboard";
        public const string Repost = "repost";
        public const string Overwatch = "overwatch";
        public const string Holdem = "holdem";
        public const string Tournament = "tournament";
        public const string Turnaus = "turnaus";
        public const string Check = "check";

        /// <summary>
        /// Array of commands the comparators check for
        /// </summary>
        public static string[] Comparables = new string[]
        {
            Repost,
            Scoreboard,
            Blackjack,
            Niggagiggel,
            Mustajaakko,
            Overwatch,
            Holdem,
            Tournament,
            Turnaus
        };

        /// <summary>
        /// Database loaded and default hardcoded Simple Commands.
        /// </summary>
        public static Dictionary<string, string> Simple
        {
            get
            {
                if (simpleCommands == null)
                {
                    simpleCommands = new Dictionary<string, string>();

                    simpleCommands.Add("hye", @">.<");
                    simpleCommands.Add("repost", @"HA GAYYYYYYYYYYY");
                }
                return simpleCommands;
            }
            set
            {
                simpleCommands = value;
            }
        }

        /// <summary>
        /// Commands that are not valid executables.
        /// </summary>
        public static List<string> Invalid = new List<string>()
        {
            "nigga",
            "troll",
            "noppa",
            "mkk",
            "sää",
            "il",
            "kk",
            "kkstart"
        };
    }
}


