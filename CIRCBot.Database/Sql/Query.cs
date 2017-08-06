using System;
using System.Data;
using System.Data.SqlClient;

namespace CIRCBot.Sql
{
    public static class Query
    {

        #region Private variables

        private static DBHandler dbhandler;

        private static Mapper mapper;

        #endregion Private variables

        #region Private accessors

        private static string connectionString { get; set; }

        /// <summary>
        /// SqlCommand container.
        /// </summary>
        private static DBHandler DB {
            get
            {
                if(dbhandler == null)
                {
                    dbhandler = new DBHandler(connectionString);
                }
                return dbhandler;
            }
        }

        /// <summary>
        /// Maps data from database to objects.
        /// </summary>
        private static Mapper Map
        {
            get
            {
                if (mapper == null)
                {
                    mapper = new Mapper();
                }
                return mapper;
            }
        }

        #endregion Private accessors

        #region Public accessors

        public static string ConnectionString
        {
            get
            {
                if(connectionString == null)
                {
                    throw new Exception("No connection string assigned");
                }
                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }
        
        #endregion Public accessors

        #region Public methods

        /// <summary>
        /// Get user information from the database.
        /// </summary>
        public static void LoadDatabase()
        {
            Map.Database(Run(DB.LoadDatabase));
        }

        public static void LoadCommands()
        {
            Map.Commands(Run(DB.LoadCommands));
        }

        public static void AddSimpleCommand(params string[] parameters)
        {
            Run(DB.EXEC(DBHandler.Procedures.AddSimpleCommand, parameters));
        }

        public static void ReplaceSimpleCommand(params string[] parameters)
        {
            Run(DB.EXEC(DBHandler.Procedures.ReplaceSimpleCommand, parameters));
        }

        public static void UpdateUser(string UserId, string Authorized, string Admin, string PartOfSeason)
        {
            Run(DB.UpdateUser(UserId, Authorized, Admin, PartOfSeason));
        }

        public static void AddUserScore(Score score, bool update)
        {
            if (update)
            {
                Run(DB.UpdateUserScore(score));
            }
            else
            {
                Run(DB.InsertUserScore(score));
            }
        }

        public static void CreateUser(string username)
        {
            Run(DB.CreateUser(username));
            LoadDatabase();
        }

        public static void DeleteUser(string username)
        {
            Run(DB.DeleteUser(username));
            LoadDatabase();
        }

        public static void UploadAdmin(string username, string address)
        {
            Run(DB.EXEC(DBHandler.Procedures.AddAdmin, username, address));
        }

        public static void EndSeason(int winner, string losers)
        {
            //Games.TournmentLoader.Reset();
            Run(DB.EndSeason(winner, losers));
        }

        public static void UpdateParamInt(ParamInt paramInt)
        {
            Run(DB.UpdateParamInt(paramInt));
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Runs an SqlCommand, and returns the results.
        /// </summary>
        /// <param name="command">SqlCommand to execute.</param>
        /// <returns>results of query.</returns>
        private static DataSet Run(SqlCommand command)
        {
            DataSet results = new DataSet();
            using (command.Connection)
            {
                command.Connection.Open();
                using (command)
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(results);
                }
            }
            return results;
        }

        #endregion Private methods

    }
}
