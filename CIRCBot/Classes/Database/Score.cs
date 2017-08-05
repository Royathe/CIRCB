using System;

namespace CIRCBot
{
    public class Score : IComparable<Score>
    {

        public const int Holdem = 0;
        public const int Blackjack = 1;
        public const int Tournament = 2;

        public int SeasonId { get; set; }

        public int UserId { get; set; }

        public int GameId { get; set; }

        public int GamesPlayed { get; set; }

        public int GamesWon { get; set; }

        public int GamesForfeitted { get; set; }

        public int TotalGains { get; set; }

        /// <summary>
        /// For logging a new score change: Increase number of games played.
        /// </summary>
        public Score Started()
        {
            GamesPlayed = 1;
            return this;
        }

        /// <summary>
        /// For logging a new score change: Increase number of games won.
        /// </summary>
        public Score Victory()
        {
            GamesWon = 1;
            return this;
        }

        /// <summary>
        /// For logging a new score change: Increase number of games lost by timeout.
        /// </summary>
        public Score Forfeit()
        {
            GamesForfeitted = 1;
            return this;
        }

        /// <summary>
        /// For logging a new score change: Increase total gains. If gains increase is > 0, then also log a game victory.
        /// </summary>
        public Score Gains(int gains)
        {
            TotalGains = gains;
            if(gains > 0)
            {
                return Victory();
            }
            return this;
        }

        public Score()
        {

        }
        
        /// <summary>
        /// Constructor.
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="gameId"></param>
        public Score(int userId, int gameId)
        {
            SeasonId = Seasons.Current.SeasonId;
            UserId = userId;
            GameId = gameId;
            GamesPlayed = 0;
            GamesWon = 0;
            GamesForfeitted = 0;
            TotalGains = 0;
        }

        public bool Equals(Score other)
        {
            return 
                other.SeasonId == SeasonId
                &&
                other.UserId == UserId
                &&
                other.GameId == GameId;
        }

        public int CompareTo(Score other)
        {
            if(other.TotalGains > TotalGains)
            {
                return -1;
            }
            else if(other.TotalGains < TotalGains)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
