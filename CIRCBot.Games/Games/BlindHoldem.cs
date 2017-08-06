using CIRCBot.Games.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot.Games
{
    class BlindHoldem : GameBase, IGame
    {

        #region Constants
        
        public const int RoundTimeLimit = 80;

        #endregion Constants

        #region Phases

        private enum Phase
        {
            Join,
            Draw,
            Flop,
            Turn,
            River,
            Over
        }

        private Phase CurrentPhase { get; set; }

        public bool GameOver
        {
            get
            {
                return CurrentPhase == Phase.Over;
            }
        }

        #endregion Phases

        #region Private accessors

        private int CurrentBet { get; }

        private List<HoldemPlayer> Players { get; set; }

        private Deck Deck { get; }

        #endregion Private accessors

        #region Constructor

        public BlindHoldem(BotMessager messager) : base(messager)
        {
            CurrentPhase = Phase.Join;
            CurrentBet = Holdem.MaximumBet;
            Players = new List<HoldemPlayer>();
            Deck = new Deck();

            Bot.Say("Sokko allin holdemi aloitettu.");
        }

        #endregion Constructor

        public bool Allin(Msg message)
        {
            return false;
        }

        public bool Bet(Msg message)
        {
            return false;
        }

        public bool Cancel(Msg message)
        {
            if(Players.Count < 2 && Players.Any(x => x.UserId == message.From.UserId))
            {
                Bot.Say("Holdem peruttu.");
                CurrentPhase = Phase.Over;
                return true;
            }
            return false;
        }

        public bool Check(Msg message)
        {
            var player = Players.FirstOrDefault(x => x.UserId == message.From.UserId);

            if (player != null)
            {
                player.Checked = true;
                CheckStatuses();
                return true;
            }
            return false;
        }

        public bool Fold(Msg message)
        {
            return false;
        }

        public bool Hit(Msg message)
        {
            return false;
        }

        public bool Join(User user)
        {
            if(CurrentPhase == Phase.Join)
            {
                if(!Players.Any(x => x.UserId == user.UserId))
                {
                    var player = new HoldemPlayer(user);
                    player.Cards[0] = Deck.Draw();
                    player.Cards[1] = Deck.Draw();
                    Players.Add(player);

                    if(Players.Count == 2)
                    {
                        Timer.Set(RoundTimeLimit, TimeLimit);
                    }
                    return true;
                }
            }
            return false;
        }

        private void TimeLimit()
        {
            if(Players.Count < 2)
            {
                Bot.Say("Aika loppui ja ei tarpeeksi pelaajia.");
                CurrentPhase = Phase.Over;
            }
            else
            {
                foreach(var player in Players)
                {
                    player.Checked = true;
                }
                CheckStatuses();
            }
        }

        private void CheckStatuses()
        {
            if(Players.Count(x => x.Checked) == Players.Count && Players.Count > 1)
            {
                End();
            }
        }

        private void End()
        {
            Timer.Stop();
            var river = new Card[]
            {
                Deck.Draw(),
                Deck.Draw(),
                Deck.Draw(),
                Deck.Draw(),
                Deck.Draw()
            };

            string cardReport = "";

            foreach (var card in river)
            {
                cardReport += " | " + card.Name;
            }

            Bot.Action(cardReport);

            var hands = new List<Hand>();

            foreach(var player in Players)
            {
                var hand = new Hand(river.Concat(player.Cards).ToArray());
                hand.Owner = player.User;

                hands.Add(hand);

                string message = player.Username.ToUpper();
                message = message.InsertAbsolute(13, "| " + player.Cards[0].Name + " ");
                message = message.InsertAbsolute(22, player.Cards[1].Name);
                message = message.InsertAbsolute(29, "| " + hand.Text);

                Bot.Say(message);
            }

            var winningHands = Hand.ResolveWinningHands(hands.ToArray());

            int pot = (CurrentBet * Players.Count);

            if (winningHands.Length == 1)
            {
                var player = Players.First(x => x.UserId == winningHands[0].Owner.UserId);

                player.Gains += pot;

                Bot.Say(winningHands[0].Owner.Username + " Voitti " + player.Gains);
            }
            else
            {
                string message = "TASAPELI: ";

                int potSplit = pot / winningHands.Length;

                foreach (var winningHand in winningHands)
                {
                    var player = Players.First(x => x.UserId == winningHands[0].Owner.UserId);

                    player.Gains += potSplit;

                    message += player.Username + " ";
                }

                Bot.Say(message);
                Bot.Say("Voittivat: " + (potSplit - CurrentBet));
            }

            // Update the scores
            foreach(var player in Players)
            {
                var newScore = new Score(player.UserId, Score.Holdem).Started();
                newScore.Gains(player.Gains);

                player.User.UpdateScore(newScore);
                Bot.Say(player.Username, "Voitit/Hävisit: " + player.Gains);
            }

            CurrentPhase = Phase.Over;
        }

        private class HoldemPlayer
        {

            public int UserId
            {
                get
                {
                    return User.UserId;
                }
            }

            public string Username
            {
                get
                {
                    return User.Username;
                }
            }

            public User User { get; }

            public bool Checked { get; set; }

            public int Gains { get; set; }

            public Card[] Cards { get; }

            public HoldemPlayer(User user)
            {
                User = user;
                Checked = false;
                Cards = new Card[2];
                Gains = Holdem.MaximumBet * -1;
            }
        }

    }
}
