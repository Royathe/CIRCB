using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CIRCBot
{
    public class User
    {
        public int UserId { get; }

        public string Username { get; }

        public bool IsAuthorized { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsPartOfSeason { get; set; }

        public List<Score> Scores { get; set; }

        public Score CurrentSeasonScoreFor(int gameId)
        {
            return Scores.
                FirstOrDefault(x => x.SeasonId == Seasons.Current.SeasonId && x.GameId == gameId) 
                ?? 
                new Score(UserId, gameId);
        }

        /// <summary>
        /// Returns false if user has no username.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.Username != String.Empty ? true : false;
            }
        }

        /// <summary>
        /// Create empty user.
        /// </summary>
        public User()
        {
            UserId = -1;
            Username = String.Empty;
            IsAuthorized = false;
            IsAdmin = false;
            IsPartOfSeason = false;
        }

        /// <summary>
        /// Create user from DataRow.
        /// </summary>
        /// <param name="row"></param>
        public User(DataRow row)
        {
            UserId = int.Parse(row["UserId"].ToString());
            Username = row["Username"].ToString();
            IsAuthorized = (bool)row["Authorized"];
            IsAdmin = (bool)row["Admin"];
            IsPartOfSeason = (bool)row["PartOfSeason"];
            Scores = new List<Score>();
        }

        public void Update()
        {
            Sql.Query.UpdateUser(
                UserId.ToSQLString(),
                IsAuthorized.ToSQLString(),
                IsAdmin.ToSQLString(),
                IsPartOfSeason.ToSQLString()
                );
        }

        public void UpdateScore(Score score)
        {
            var oldScore = Scores.FirstOrDefault(x => x.Equals(score));

            if(oldScore != null)
            {
                // Update pre-existing score.
                oldScore.GamesPlayed += score.GamesPlayed;
                oldScore.GamesWon += score.GamesWon;
                oldScore.GamesForfeitted += score.GamesForfeitted;
                oldScore.TotalGains += score.TotalGains;
                Sql.Query.AddUserScore(oldScore, true);
            }
            else
            {
                // Add new score.
                Scores.Add(score);
                Sql.Query.AddUserScore(score, false);
            }
        }
    }
}
