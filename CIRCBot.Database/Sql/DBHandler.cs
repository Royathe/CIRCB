using System.Data.SqlClient;

namespace CIRCBot.Sql
{
    class DBHandler
    {

        #region Enumerations

        public enum Procedures
        {
            AddSimpleCommand,
            ReplaceSimpleCommand,
            UpdateAllUserFields,
            AddAdmin
        }

        #endregion Enumerations

        #region Private accessors

        private string connectionString { get; }

        /// <summary>
        /// Create a connection to the database.
        /// </summary>
        private SqlConnection connection
        {
            get
            {
                return new SqlConnection(connectionString);
            }
        }

        /// <summary>
        /// A command connected to the database.
        /// </summary>
        /// <param name="queryString">The SQL of the command</param>
        /// <returns>new connected command with given SQL</returns>
        private SqlCommand command(string queryString)
        {
            return new SqlCommand(queryString, connection);
        }

        #endregion Private accessors

        public DBHandler(string cs)
        {
            connectionString = cs;
        }

        #region SqlCommand getters

        /// <summary>
        /// Load users and admin addresses.
        /// </summary>
        public SqlCommand LoadDatabase
        {
            get
            {
                return command(
                    "SELECT * FROM Users " +
                    "SELECT * FROM AdminAddresses " +
                    "SELECT * FROM Scores " +
                    "SELECT * FROM Seasons " +
                    "SELECT * FROM ParamInt "
                    );
            }
        }

        public SqlCommand LoadCommands
        {
            get
            {
                return command("SELECT * FROM SimpleCommands");
            }
        }

        public SqlCommand CreateUser(string username)
        {
            return command(string.Format("INSERT INTO Users (Username) SELECT '{0}'", username));
        }
        public SqlCommand UpdateUser(string UserId, string Authorized, string Admin, string PartOfSeason)
        {
            return command(string.Format(@"
                        UPDATE  Users
                        SET     Authorized = {1},
                                Admin = {2},
                                PartOfSeason = {3}
                        WHERE   UserId = {0}
                    ", UserId, Authorized, Admin, PartOfSeason));
        }

        public SqlCommand UpdateUserScore(Score s)
        {
            return command(string.Format(@"
                        UPDATE  Scores
                        SET     GamesPlayed = {3},
                                GamesWon = {4},
                                GamesForfeitted = {5},
                                TotalGains = {6}
                        WHERE   SeasonId = {0}
                                AND
                                UserId = {1}
                                AND
                                GameId = {2}
                ", s.SeasonId, s.UserId, s.GameId, s.GamesPlayed, s.GamesWon, s.GamesForfeitted, s.TotalGains));
        }

        public SqlCommand InsertUserScore(Score s)
        {
            return command(string.Format(@"
                        INSERT INTO Scores VALUES
                        ({0}, {1}, {2}, {3}, {4}, {5}, {6})
                ", s.SeasonId, s.UserId, s.GameId, s.GamesPlayed, s.GamesWon, s.GamesForfeitted, s.TotalGains));
        }

        public SqlCommand DeleteUser(string username)
        {
            return command(string.Format("DELETE FROM Users WHERE Username = '{0}' AND Admin = 0", username));
        }

        public SqlCommand EndSeason(int winner, string losers)
        {
            return command(string.Format(@"
                        UPDATE  Seasons
                        SET     Winner = {0},
                                Losers = '{1}',
                                ToDate = {3},
                                SettledUsers = ''
                        WHERE   SeasonId = {2}

                        INSERT INTO Seasons (FromDate)
                        SELECT {3}
                ", winner, losers, Seasons.Current.SeasonId, System.DateTime.Now.ToSQLString()));
        }

        public SqlCommand UpdateParamInt(ParamInt param)
        {
            return command(string.Format(@"
                        UPDATE  ParamInt
                        SET     ParamValue = {1}
                        WHERE   ParamId = {0}
                ", param.ParamId, param.ParamValue));
        }

        /// <summary>
        /// Return an SqlCommand that will execute a stored procedure with given parameters.
        /// </summary>
        /// <param name="procedure">Procedure to execute</param>
        /// <param name="parameters">Parameters of the procedure</param>
        /// <returns>SqlCommand that will execute the stored procedure</returns>
        public SqlCommand EXEC(Procedures procedure, params string[] parameters)
        {
            string sql = "EXEC dbo.usp_" + procedure.ToString();
            foreach(string param in parameters)
            {
                sql += " '" + param + "',";
            }
            // Remove the last comma if parameters were given.
            if(parameters.Length > 0)
            {
                sql = sql.Remove(sql.Length - 1, 1);
            }
            return command(sql);
        }

        #endregion SqlCommand getters

    }
}
