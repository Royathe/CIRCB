using System;
using System.Data;

namespace CIRCBot.Sql
{
    class Mapper
    {
        /// <summary>
        /// Map dataset results to repositories.
        /// </summary>
        /// <param name="users">Dataset with database data</param>
        public void Database(DataSet users)
        {
            CIRCBot.Users.Reset();
            mapUsers(users.Tables[0]);
            mapAdmins(users.Tables[1]);
            mapScores(users.Tables[2]);
            mapSeasons(users.Tables[3]);
            mapParams(users.Tables[4]);
        }

        /// <summary>
        /// Map dataset results to Simple Commands dictionary.
        /// </summary>
        /// <param name="commands">Dataset with the database commands</param>
        public void Commands(DataSet commands)
        {
            Cmd.Simple = null;
            foreach (DataRow row in commands.Tables[0].Rows)
            {
                try
                {
                    Cmd.Simple.Add(row[0].ToString(), row[1].ToString());
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void mapUsers(DataTable users)
        {
            foreach (DataRow row in users.Rows)
            {
                try
                {
                    CIRCBot.Users.Add(new User(row));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void mapAdmins(DataTable admins)
        {
            foreach(DataRow row in admins.Rows)
            {
                try
                {
                    CIRCBot.Users.Add(
                        row["Name"].ToString(),
                        row["Address"].ToString()
                        );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void mapScores(DataTable scores)
        {
            foreach(DataRow row in scores.Rows)
            {
                try
                {
                    var score = ClassMapper.Map<Score>(row);
                    CIRCBot.Users.Add(score);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void mapSeasons(DataTable seasons)
        {
            Seasons.Reset();
            foreach (DataRow row in seasons.Rows)
            {
                try
                {
                    var season = ClassMapper.Map<Season>(row);
                    season.ParseData();
                    Seasons.Add(season);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void mapParams(DataTable paramInts)
        {
            Params.Reset();
            foreach (DataRow row in paramInts.Rows)
            {
                try
                {
                    var paramInt = ClassMapper.Map<ParamInt>(row);
                    Params.Add(paramInt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
