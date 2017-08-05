using System;
using System.Collections.Generic;

using CIRCBot.Sql;
using System.Linq;

namespace CIRCBot.Execution.Executors
{
    /// <summary>
    /// Admin commands.
    /// </summary>
    [ClassName("Admin", "Botin hallintakomennot")]
    class CmdAdmin : BaseExecutor, IExecutor
    {

        #region Readonly accessors

        /// <summary>
        /// Command that has its parameters passed to this executor.
        /// </summary>
        //public string Admin_Command_Identifier { get { return "admin"; } }

        #endregion Readonly accessors

        #region Private accessors

        /// <summary>
        /// Message to send to the admin.
        /// </summary>
        private string adminMessage { get; set; }

        #endregion Private accessors

        #region Constructor

        public CmdAdmin(BotMessager messager) : base(messager)
        {
            // Get the properties of a StatisticRow for ORDER BY logging.
            //string orderPropertyNameList = StatisticsRow.GetOrderProperties().ToConcatString();

            //// Get the properties of a StatisticRow for EXTRAS logging.
            //string extraPropertyNameList = StatisticsRow.GetExtraProperties().ToConcatString();

            Admin("Add new entry to the SimpleCommands dictionary.", cmd_AddSimpleCommand,
                "addcommand");

            Admin("Replace existing SimpleCommands entry.", cmd_ReplaceSimpleCommand,
                "replacecommand");

            Admin("Checks if the given key exists in SimpleCommands.", cmd_SimpleCommandExists,
                "cmdexists");

            //Add("Tulostaa pelaajan holdem voitot.", cmd_Saldo, 
            //    "saldo", "score", "pisteet");

            //Add("Tulostaa kuittaamattomat kausivoitot.", cmd_Unsettled,
            //    "loans", "velat", "unsettled", "joni", "vittujoni");

            //Add("Statistiikka. FILTERÖINTI: Kauden ID, Pelin nimi, Käyttäjän nimi. ORDER BY: " + orderPropertyNameList + " | EXTRA: " + extraPropertyNameList, cmd_Stats,
            //    "stats");

            //Add("Tulostaa kuluvan kauden ID:n.", cmd_Season,
            //    "season", "kausi");

            //Add("Tulostaa kaudet ja niiden ajanjaksot.", cmd_Seasons,
            //    "seasons", "kaudet");

            Admin("Asettaa statistiikan värit. 'TextColor.BackgroundColor'.", cmd_ColorizeStats,
                "statscolor");

            Admin("Asettaa turnauksen textin värit. 'TextColor.BackgroundColor'.", cmd_ColorizeTournament,
                "tournamentcolor");

            //Add("Tulostaa pelin voitot", cmd_Scoreboard, 
            //    "scoreboard", "scorebard", "scöörebörd", "scoorebord", "sköörebörd", "skoorebord", "skuurbuurd", "skoorboord", "sköörböörd");

            //CmdActions.Add("addfunds", cmd_AddFunds, "Give player a certain amount of funds. Increases loan.");

            //CmdActions.Add("addfundsnoloan", cmd_AddFundsNoLoan, "Give player a certain amount of funds. Does not increase loan.");

            //CmdActions.Add("adduser", cmd_addUser, "Add new user or authorize existing.");

            Admin("Add new user or authorize existing.", cmd_addUser,
                "adduser");

            Admin("Unauthorize existing user.", cmd_BanUser, 
                "banuser");

            //CmdActions.Add("deleteuser", cmd_DeleteUser, "Delete user from database entirely.");

            //CmdActions.Add("loans", cmd_Loans, "Report everyone's loans.");

            //Admin("Reset SimpleCommands to those in the Bulk Inserter.", cmd_ResetSimpleCommands,
            //    "resetcmd");
            Admin("Ends the current season and creates a new one.", cmd_EndSeason, 
                "endseason");

            Add("Log in as an admin from a new address. New address updated to database.", cmd_AdminLogin,
                "login");

            Admin("Floods channel with messages.", cmd_Flood,
                "flood");

            Admin(cmd_Transmit,
                "t");

            Admin(cmd_currentTimeDiff,
                "aikaero", "timesync");

            Admin(cmd_currentTime,
                "aika", "timeis");

            Add("Sääennuste", cmd_weather, 
                "sääennuste").TimerLocked(5);
        }

        #endregion Constructor
        
        public new void Execute(Msg message)
        {
            base.Execute(message);

            LogResults(RunCommand());
        }

        #region Commands

        private void cmd_weather()
        {
            Weather weather = null;
            string city = Message.NextCommand();
            string country = Message.NextCommand();
            string time = Message.NextCommand();

            bool getForecast = false;
            int hour = 0;

            if(int.TryParse(city, out hour))
            {
                city = "";
                country = "";
                getForecast = true;
            }
            else if(int.TryParse(country, out hour))
            {
                country = "";
                getForecast = true;
            }
            else if(int.TryParse(time, out hour))
            {
                getForecast = true;
            }

            if (getForecast)
            {
                DateTime now = DateTime.Now;
                DateTime date = new DateTime(now.Year, now.Month, now.Day);

                hour = hour > 24 ? 24 : hour;
                hour = hour < 0 ? 0 : hour;

                if(hour < now.Hour)
                {
                    date = date.AddDays(1);
                }

                date = date.AddHours(hour);

                weather = GM.WeatherOrForecast(city, country, date);
            }
            else
            {
                weather = GM.WeatherOrForecast(city, country, null);
            }

            if(weather != null)
            {
                string message = 
                    weather.City.Name + " " + weather.Date.ToShortDateTimeString() + 
                    " | Lämpötila: " + weather.Temperature + Library.CENTIGRADE + 
                    ", tuntuu kuin: " + weather.FeelsLike +
                    " | Pilvisyys: " + weather.Cloudiness + "%" + 
                    " | Ilmankosteus: " + weather.Humidity + "%" + 
                    " | Tuulennopeus: " + weather.WindSpeed + "m/s" + 
                    weather.WeatherDescription;

                Bot.Action(message);
            }
        }

        private void cmd_currentTimeDiff()
        {
            DateTime now = GM.CurrentTime();

            TimeSpan diff = now - DateTime.Now;

            Bot.Say("-" + diff.ToString());
        }

        private void cmd_currentTime()
        {
            DateTime now = GM.CurrentTime();

            Bot.Say(now.ToString());
        }

        /// <summary>
        /// Have bot say the given text.
        /// </summary>
        private void cmd_Transmit()
        {
            string message = "";

            while(Message.Command != String.Empty)
            {
                message += Message.NextCommand() + " ";
            }

            Bot.Say(message);
        }

        /// <summary>
        /// Flood channel to test flood protection.
        /// </summary>
        private void cmd_Flood()
        {
            int i = 0;
            while(i < 20)
            {
                Bot.Say("FLOODING!");
                i++;
            }
        }
        
        /// <summary>
        /// Check given password and if matched, add new admin address.
        /// </summary>
        private void cmd_AdminLogin()
        {
            adminMessage = "Login failed. Incorrect password.";
            if(Message.NextCommand() == System.Configuration.ConfigurationManager.AppSettings["AdminPassword"])
            {
                adminMessage = "Admin addition to dictionary failed.";
                Users.Add(Message.From.Username, Message.Address);

                adminMessage = "Admin addition to database failed.";
                Query.UploadAdmin(Message.From.Username, Message.Address);
            }
        }

        /// <summary>
        /// Add a command to the simple commands dictionary.
        /// </summary>
        private void cmd_AddSimpleCommand()
        {
            addSimpleCommand(Message.NextCommand());
        }

        /// <summary>
        /// Replaces existing command in the simple commands dictionary.
        /// </summary>
        private void cmd_ReplaceSimpleCommand()
        {
            addSimpleCommand(Message.NextCommand(), true);
        }

        /// <summary>
        /// Checks if the given key exists in SimpleCommands.
        /// </summary>
        private void cmd_SimpleCommandExists()
        {
            if (Cmd.Simple.ContainsKey(Message.NextCommand()))
            {
                Bot.Say(Message.From.Username, "Command " + Message.Command + " found in dictionary. Value | " + Cmd.Simple[Message.Command]);
            }
            else
            {
                Bot.Say(Message.From.Username, "Command " + Message.Command + " does not exist in the dictionary.");
            }
        }

        ///// <summary>
        ///// Add funds for a player.
        ///// </summary>
        //private void cmd_AddFunds()
        //{
        //    addFunds(Message.NextCommand(), Message.NextCommand());
        //}

        ///// <summary>
        ///// Add funds for a player without increasing their loans.
        ///// </summary>
        //private void cmd_AddFundsNoLoan()
        //{
        //    addFunds(Message.NextCommand(), Message.NextCommand(), false);
        //}

        /// <summary>
        /// Either adds a new user entirely, or if exists, authorizes them.
        /// </summary>
        private void cmd_addUser()
        {
            setUserAuthorization(Message.NextCommand(), true);
        }

        /// <summary>
        /// Sets user's authorization off.
        /// </summary>
        private void cmd_BanUser()
        {
            setUserAuthorization(Message.NextCommand(), false);
        }

        /// <summary>
        /// Deletes a user entirely from the database.
        /// </summary>
        private void cmd_DeleteUser()
        {
            deleteUser(Message.NextCommand());
        }

        /// <summary>
        /// Ends the current season and start a new one.
        /// </summary>
        private void cmd_EndSeason()
        {
            List<Score> topTournamentScores = new List<Score>();
            User winner;

            var tournamentPlayers = Games.TournmentLoader.UsersInTournament();

            foreach (var user in tournamentPlayers)
            {
                topTournamentScores.Add(user.CurrentSeasonScoreFor(Score.Tournament));
            }

            winner = Users.Get(topTournamentScores.Max().UserId);

            string losers = "";

            foreach(var user in tournamentPlayers)
            {
                if(user.UserId != winner.UserId)
                {
                    losers += user.UserId + " ";
                }
            }

            Query.EndSeason(winner.UserId, losers);
            Query.LoadDatabase();
        }

        /// <summary>
        /// Sets the colors used for statistics table texts.
        /// </summary>
        private void cmd_ColorizeStats()
        {
            Options.ColorOptions.StatisticsTableColor = parseTextColorFromCommand();
        }

        /// <summary>
        /// Sets the colors used for tournament texts.
        /// </summary>
        private void cmd_ColorizeTournament()
        {
            Options.ColorOptions.TournamentTextColor = parseTextColorFromCommand();
        }

        /// <summary>
        /// Parse a new color definition from the text commands
        /// </summary>
        /// <returns></returns>
        private MircColor parseTextColorFromCommand()
        {
            string colorFullName = Message.NextCommand();

            int splitIndex = colorFullName.IndexOf('.');

            string textColor = "";
            string bgColor = String.Empty;

            if (splitIndex != -1)
            {
                textColor = colorFullName.Substring(0, splitIndex).Trim();
                bgColor = colorFullName.Substring(splitIndex + 1).Trim();
            }
            else
            {
                textColor = colorFullName.Trim();
            }

            MircColor color = new MircColor().Colorize(textColor);

            if (bgColor != string.Empty)
            {
                color = color.Colorize(bgColor);
            }

            return color;
        }

        #endregion Commands

        #region Private utility methods

        /// <summary>
        /// Send a private message to all admins.
        /// </summary>
        /// <param name="message">Message to send.</param>
        private void logToAdmin(string message)
        {
            if(message != String.Empty)
            {
                message = " -> Admin log: " + message;
                Console.WriteLine(message);
                foreach (User admin in Users.Admins)
                {
                    Bot.Notice(admin.Username, message);
                }
            }
        }

        private void addSimpleCommand(string command, bool replace = false)
        {
            // Prefix for the admin message indicating an error.
            string error = "Command addition failed. ";
            adminMessage = error + "Dictionary failure.";

            string message = "";

            foreach(string part in Message.CommandParts.Subarray(Message.CurrentCommandIndex + 1))
            {
                message += part + " ";
            }

            if (command != String.Empty && message != String.Empty)
            {
                adminMessage = error + "Procedure failure.";

                if (!replace)
                {
                    Query.AddSimpleCommand(command, message);
                    Query.LoadCommands();
                    adminMessage = "Command addition succesfull.";
                }
                else
                {
                    Query.ReplaceSimpleCommand(command, message);
                    Query.LoadCommands();
                    adminMessage = "Command replacement succesfull.";
                }
            }
            else
            {
                adminMessage = error + "Empty parameter failure.";
                throw new NullReferenceException();
            }
        }

        private void setUserAuthorization(string username, bool authorized)
        {
            // Prefix for the admin message indicating an error.
            string error = "Authorization alteration for " + username + " failed. ";
            adminMessage = error + "No username.";

            if (username != String.Empty)
            {
                adminMessage = error + "Get User failure.";
                User user = Users.Get(username);
                if (user.IsValid)
                {
                    adminMessage = error + "Procedure failure when updating.";
                    user.IsAuthorized = authorized;
                    user.Update();
                    adminMessage = "Existing user's authorization set to: " + (user.IsAuthorized ? "Authorized" : "Unauthorized");
                }
                else if(authorized)
                {
                    adminMessage = error + "Procedure failure when creating new user.";
                    Query.CreateUser(username);
                    adminMessage = "Succesfully created new user: " + username;
                }
            }
        }

        private void deleteUser(string username)
        {
            // Prefix for the admin message indicating an error.
            string error = "Deletion of user " + username + " failed. ";
            adminMessage = error + "No username.";

            if (username != String.Empty)
            {
                adminMessage = error + "Get User failure.";
                User user = Users.Get(username);
                if(user.IsValid)
                {
                    adminMessage = error + "User is an admin.";
                    if (!user.IsAdmin)
                    {
                        adminMessage = error + "Procedure failure.";
                        Query.DeleteUser(username);
                        adminMessage = username + " deleted from database.";
                    }
                }
            }
        }

        #endregion Private utility methods

    }
}
