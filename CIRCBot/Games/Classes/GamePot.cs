using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot.Games
{
    class GamePot
    {

        public bool Handled { get; set; }

        public List<PlayerPot> PlayerPots { get; set; }

        public int Total
        {
            get
            {
                return PlayerPots.Sum(x => x.Funds);
            }
        }

        public int Max
        {
            get
            {
                return PlayerPots.Max(x => x.Funds);
            }
        }

        public PlayerPot Get(int userId)
        {
            var pot = PlayerPots.FirstOrDefault(x => x.UserId == userId);
            if(pot == null)
            {
                PlayerPots.Add(new PlayerPot(userId, 0));
                return PlayerPots.First(x => x.UserId == userId);
            }
            return pot;
        }

        public bool Has(int userId)
        {
            return PlayerPots.Any(x => x.UserId == userId);
        }

        public GamePot()
        {
            Handled = false;
            PlayerPots = new List<PlayerPot>();
        }

    }
}
