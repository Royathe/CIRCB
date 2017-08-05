using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot
{
    public class Statistics
    {

        public List<StatisticsRow> StatRows { get; set; }

        public Statistics(int[] seasonIds, int[] gameIds, int[] userIds, string[] extraColumns, string orderByColumn)
        {
            List<Score> scores = new List<Score>();

            // If no users given, use all users with scores
            if(userIds.Length < 1)
            {
                userIds = Users.All.
                    Where(x => x.Scores.Count > 0).
                    Select(x => x.UserId).
                    ToArray();
            }

            // Fetch all defined users scores
            foreach (var userId in userIds)
            {
                scores.AddRange(
                    Users.Get(userId).Scores
                    );
            }

            // Remove all scores where the season id is not contained in the filter
            if (seasonIds.Length > 0)
            {
                scores.RemoveAll(x => !seasonIds.Contains(x.SeasonId));
            }

            // Remove all scores where the game id is not contained in the filter
            if (gameIds.Length > 0)
            {
                scores.RemoveAll(x => !gameIds.Contains(x.GameId));
            }

            var scoresByUser = scores.GroupBy(x => x.UserId);
            var stats = new List<StatisticsRow>();

            foreach (var group in scoresByUser)
            {
                var userId = group.Key;
                Score sumScore = new Score();

                sumScore.UserId = userId;

                sumScore.GamesPlayed = group.Select(x => x.GamesPlayed).Sum();
                sumScore.GamesWon = group.Select(x => x.GamesWon).Sum();
                sumScore.GamesForfeitted = group.Select(x => x.GamesForfeitted).Sum();
                sumScore.TotalGains = group.Select(x => x.TotalGains).Sum();

                var statsRow = new StatisticsRow(sumScore);
                if (extraColumns.Contains("seasons"))
                {
                    statsRow.SeasonsWon = Seasons.All.Where(x => x.Winner == userId).Count();
                    statsRow.SeasonsLost = Seasons.All.Where(x => x.LosersList.Any(x2 => x2.UserId == userId)).Count();
                }
                stats.Add(statsRow);
            }

            if(orderByColumn != String.Empty && typeof(StatisticsRow).GetProperties().Any(x => x.Name.ToLower() == orderByColumn.ToLower()))
            {
                StatRows = stats.OrderByDescending(x => x.GetType().GetProperties().First(x2 => x2.Name.ToLower() == orderByColumn.ToLower()).GetValue(x, null)).ToList();
            }
            else
            {
                StatRows = stats.OrderByDescending(x => x.Total).ToList();
            }
        }
    }
}
