using System;
using System.Collections.Generic;
using System.Linq;

namespace CIRCBot.Execution.Executors
{
    /// <summary>
    /// Admin commands.
    /// </summary>
    [ClassName("Statistics", "Botin statistiikat")]
    class CmdStatistics : BaseExecutor, IExecutor
    {

        #region Constructor

        public CmdStatistics(BotMessager messager) : base(messager)
        {
            // Get the properties of a StatisticRow for ORDER BY logging.
            string orderPropertyNameList = StatisticsRow.GetOrderProperties().ToConcatString();

            // Get the properties of a StatisticRow for EXTRAS logging.
            string extraPropertyNameList = StatisticsRow.GetExtraProperties().ToConcatString();

            Add("Tulostaa pelaajan tournament voitot.", 
                cmd_Saldo,
                "saldo", "score", "pisteet");

            Add("Tulostaa kuittaamattomat kausivoitot.", 
                cmd_Unsettled,
                "loans", "velat", "unsettled", "joni", "vittujoni");

            Add("Statistiikka. FILTERÖINTI: Kauden ID, Pelin nimi, Käyttäjän nimi. ORDER BY: " + orderPropertyNameList + " | EXTRA: " + extraPropertyNameList, 
                cmd_Stats, 
                "stats").TimerLocked(2);

            Add("Tulostaa kuluvan kauden ID:n.", 
                cmd_Season,
                "season", "kausi").TimerLocked(2);

            Add("Tulostaa kaudet ja niiden ajanjaksot.", 
                cmd_Seasons,
                "seasons", "kaudet").TimerLocked(2);

            Add("Tulostaa pelin voitot", 
                cmd_Scoreboard,
                "scoreboard", "scorebard", "scöörebörd", "scoorebord", "sköörebörd", "skoorebord", "skuurbuurd", "skoorboord", "sköörböörd").TimerLocked(2);
        }

        #endregion Constructor

        public new void Execute(Msg message)
        {
            base.Execute(message);

            LogResults(RunCommand());
        }

        #region Commands

        /// <summary>
        /// Logs the senders current Tournament score.
        /// </summary>
        private void cmd_Saldo()
        {
            var nextCommand = Message.NextCommand();
            if(nextCommand != String.Empty)
            {
                var target = Users.Get(nextCommand.Trim());

                if(target != null)
                {
                    Bot.Say(target.Username.AddI() + "n saldo: " + target.CurrentSeasonScoreFor(Score.Tournament).TotalGains);
                    return;
                }
            }
            Bot.Say(Message.From.Username.AddI() + "n saldo: " + Message.From.CurrentSeasonScoreFor(Score.Tournament).TotalGains);
        }

        /// <summary>
        /// Logs current season statistics
        /// </summary>
        private void cmd_Scoreboard()
        {
            string message = Message.Text;

            int splitIndex = message.IndexOf(' ');

            if (splitIndex < 0)
            {
                message += " " + Seasons.Current.SeasonId + " ";
            }
            else
            {
                message = message.Insert(message.IndexOf(' '), Seasons.Current.SeasonId + " ");
            }

            PrintStats(message);
        }

        /// <summary>
        /// Logs dynamic statistics
        /// </summary>
        private void cmd_Stats()
        {
            PrintStats(Message.Text + " ");
        }

        /// <summary>
        /// Logs the current season
        /// </summary>
        private void cmd_Season()
        {
            Bot.Say("Kuluva kausi: " + Seasons.Current.SeasonId);
        }

        /// <summary>
        /// Logs the seasons, their timespans and their winners/losers
        /// </summary>
        private void cmd_Seasons()
        {
            foreach (var season in Seasons.All.OrderBy(x => x.SeasonId))
            {
                PositionString message = "";
                message.Position(3, " " + season.SeasonId);
                message.Section(25,
                    season.FromDate.ToShortDateString() + " - "
                    +
                    (season.ToDate.HasValue ? season.ToDate.Value.ToShortDateString() : "")
                    );

                if (season.Winner.HasValue)
                {

                    string losers = "Häviäjät: ";
                    foreach (var user in season.LosersList)
                    {
                        if (!season.SettledUsersList.Contains(user))
                        {
                            losers += user.Username.ToUpper() + "[ ], ";
                        }
                        else
                        {
                            losers += user.Username.ToUpper() + "[X], ";
                        }
                    }
                    losers = losers.Substring(0, losers.Length - 2);

                    message.Section(25, "Voittaja: " + Users.Get(season.Winner ?? -1).Username.ToUpper());
                    message.Section(150, losers);
                }
                message.SectionInsert(150);

                Action(message);
            }
        }

        /// <summary>
        /// Logs the seasons which have unsettled winner/loser debts.
        /// </summary>
        private void cmd_Unsettled()
        {
            foreach (var season in Seasons.All.Where(x => x.SettledUsersList.Count != x.LosersList.Count))
            {
                PositionString message = "";
                message.Section(12, "Kausi: " + season.SeasonId);

                message.Section(26, "Voittaja: " + Users.Get(season.Winner ?? 0).Username);

                string losers = "Kuittamatta: ";
                foreach(var user in season.LosersList)
                {
                    if (!season.SettledUsersList.Contains(user))
                    {
                        losers += user.Username + ", ";
                    }
                }
                losers = losers.Substring(0, losers.Length - 2);

                message.Section(150, losers);
                message.SectionInsert(150);
                
                Action(message);
            }
        }


        #endregion Commands

        #region Private utility methods

        /// <summary>
        /// Colorizes the text before passing it to the BotMessager.
        /// </summary>
        /// <param name="message">Message to colorize</param>
        private void Action(string message)
        {
            message = "| " + message;
            Bot.Action(message.Colorize(Options.ColorOptions.StatisticsTableColor));
        }

        /// <summary>
        /// Parses the message into filters and creates the statistics table
        /// </summary>
        /// <param name="message">Message.Text</param>
        private void PrintStats(string message)
        {
            string filters = message.ToLower().Substring(
                message.IndexOf(Message.Command)
                );

            string orderBy = String.Empty;

            if (message.Contains("orderby "))
            {
                string order = message.Substring(
                    message.IndexOf("orderby ")
                    );

                orderBy = order.Substring(order.IndexOf(' ')).Trim(' ');
            }

            // Game ID filters
            List<int> GameIdFilters = new List<int>();

            // Season ID filters
            List<int> SeasonIdFilters = new List<int>();

            // User ID filters
            List<int> UserIdFilters = new List<int>();

            // Extra Columns that will be mapped
            List<string> ExtraColumns = new List<string>();

            // Get all Games listed in the database
            var Games = Params.ParamAttName("Game");

            // Check if specific Game Names were given as filters.
            foreach (var game in Games)
            {
                if (filters.Contains(game.ParamText.ToLower()))
                {
                    GameIdFilters.Add(game.ParamCode);
                }
            }

            // Check if specific User Names were given as filters.
            foreach (var user in Users.All.Where(x => x.Scores.Count > 0))
            {
                if (filters.Contains(user.Username.ToLower()))
                {
                    UserIdFilters.Add(user.UserId);
                }
            }

            string columnOptions = filters;

            if (orderBy != String.Empty)
            {
                columnOptions = filters.Substring(0, filters.IndexOf(orderBy));
            }

            // Check if any extra columns were set to be added.
            foreach (var columnName in StatisticsRow.GetExtraProperties()/*typeof(StatisticsRow).GetProperties().Select(x => x.Name)*/)
            {
                if (columnOptions.Contains(columnName.ToLower()))
                {
                    ExtraColumns.Add(columnName.ToLower());
                }
            }

            // Check if an integer Range was given
            if (filters.Contains("-"))
            {
                int delimiterIndex = filters.IndexOf('-');

                string beforeFull = filters.Substring(0, delimiterIndex);

                int delimiterPreceedingSpaceIndex = beforeFull.LastIndexOf(' ');

                // Get the text between the last space before the delimite, and the delimite
                string before = filters.SubstringIndex(delimiterPreceedingSpaceIndex, delimiterIndex - 1);

                int delimiterFollowingSpaceIndex = filters.Substring(delimiterIndex).IndexOf(' ');

                // Get the text between the delimite and the next space. If no space, then everything after the delimiter
                string after = filters.Substring(delimiterIndex + 1);
                if (delimiterFollowingSpaceIndex != -1)
                {
                    after = after.SubstringIndex(0, delimiterFollowingSpaceIndex);
                }

                int fromValue = 0;
                int toValue = Seasons.Current.SeasonId;

                // Try parsing the texts to new values for the ranges
                int.TryParse(before, out fromValue);
                int.TryParse(after, out toValue);

                for (int i = fromValue; i <= toValue; i++)
                {
                    SeasonIdFilters.Add(i);
                }
            }
            else
            {
                // Check if specific Season ID's were given as filters
                foreach (var season in Seasons.All)
                {
                    if (filters.Contains(" " + season.SeasonId.ToString() + " ") || filters.Contains(" all"))
                    {
                        SeasonIdFilters.Add(season.SeasonId);
                    }
                }
            }

            // If no games defined, add all games to filters
            if (GameIdFilters.Count == 0)
            {
                GameIdFilters.AddRange(Games.Select(x => x.ParamCode));
            }

            // If no seasons defined, add current season to filters
            if (SeasonIdFilters.Count == 0)
            {
                SeasonIdFilters.Add(Seasons.Current.SeasonId);//AddRange(Seasons.All.Select(x => x.SeasonId));
            }

            var stats = new Statistics(SeasonIdFilters.ToArray(), GameIdFilters.ToArray(), UserIdFilters.ToArray(), ExtraColumns.ToArray(), orderBy);

            foreach (string stat in stats.StatRows)
            {
                Bot.Action(stat.Colorize(Options.ColorOptions.StatisticsTableColor));
            }
        }

        #endregion Private utility methods

    }
}
