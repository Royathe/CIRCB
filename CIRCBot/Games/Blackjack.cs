using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot.Games
{
    class Blackjack : GameBase, IGame
    {

        #region Constants

        private const int MaximumBet = 50000;

        public const int MinimumBet = 5000;

        #endregion Constants

        #region Phases

        private enum Phase
        {
            Join,
            Players,
            House,
            Over
        }

        private Phase currentPhase { get; set; }

        public bool GameOver
        {
            get
            {
                return currentPhase == Phase.Over;
            }
        }

        #endregion Phases

        #region Private accessors

        private List<BlackjackPlayer> Players { get; set; }

        private Deck Decks { get; }

        #endregion Private accessors

        #region Constructor

        public Blackjack(BotMessager messager) : base(messager)
        {
            currentPhase = Phase.Join;
            Players = new List<BlackjackPlayer>();
            Decks = new Deck(3);
        }

        #endregion Constructor

        #region Public methods

        public bool Join(User user)
        {
            if(currentPhase == Phase.Join)
            {
                if(Players.Any(x => x.UserId == user.UserId))
                {
                    Bot.Say(user.Username, "Olet jo blackjack pelissä.");
                }
                else
                {
                    var player = new BlackjackPlayer(user);
                    HitPlayer(player, 2);
                    Players.Add(player);
                    Bot.Say(user.Username, "Liityit blackjack peliin.");
                    return true;
                }
            }
            else
            {
                Bot.Say(user.Username, "Et voi enää liittyä blackjack peliin.");
            }
            return false;
        }

        public bool Allin(Msg message)
        {
            BlackjackPlayer player;
            if (ValidPlayer(message, out player))
            {
                Raise(player, MaximumBet);
                return true;
            }
            return false;
        }

        public bool Bet(Msg message)
        {
            if (currentPhase > Phase.Join) { return false; }

            BlackjackPlayer player;
            if(ValidPlayer(message, out player))
            {
                if (!player.Checked)
                {
                    int raise;
                    if (int.TryParse(message.NextCommand(), out raise))
                    {
                        Raise(player, raise);
                    }
                    else if(message.Command == "all")
                    {
                        Raise(player, MaximumBet);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool Cancel(Msg message)
        {
            if(currentPhase == Phase.Join && Players.Any(x => x.UserId == message.From.UserId))
            {
                Bot.Say("Keskeytetään pelin aloitus");
                currentPhase = Phase.Over;
                return true;
            }
            return false;
        }

        public bool Check(Msg message)
        {
            if (currentPhase == Phase.House) { return false; }

            BlackjackPlayer player;
            if (ValidPlayer(message, out player))
            {
                if (!player.Checked)
                {
                    player.Checked = true;
                    Bot.Say(player.Username, "Checkkaat.");
                    CheckStatuses();
                    return true;
                }
            }
            return false;
        }

        public bool Fold(Msg message)
        {
            Bot.Say(message.From.Username, "gg");
            return true;
        }

        public bool Hit(Msg message)
        {
            if (currentPhase == Phase.House) { return false; }

            BlackjackPlayer player;
            if (ValidPlayer(message, out player))
            {
                if (!player.Checked)
                {
                    HitPlayer(player);
                    return true;
                }
            }
            return false;
        }

        #endregion Public methods

        #region Game logic

        private void HitPlayer(BlackjackPlayer player, int numOfCards = 1)
        {
            string message = player.Username.ToUpper() + ": ";
            for(int i = 0; i < numOfCards; i++)
            {
                var card = Decks.Draw();

                message += card.Name + " ";

                int score = CardScore(card);

                player.Score += score;
                if(score == 11)
                {
                    player.Aces++;
                }

                if(player.Score > 21 && player.Aces > 0)
                {
                    player.Score -= 10;
                    player.Aces--;
                }
            }

            Bot.Say(message + " | Pisteet: " + player.Score);

            if (player.Score > 21)
            {
                Bot.Say(player.Username.ToUpper() + " YLI!");
                PlayerLose(player);
                CheckStatuses();
            }
        }

        private void PlayerLose(BlackjackPlayer player)
        {
            var newScore = new Score(player.UserId, Score.Blackjack).Started();
            newScore.Gains(player.CurrentBet * -1);

            player.User.UpdateScore(newScore);

            Players.Remove(player);
        }

        private void CheckStatuses()
        {
            if(Players.Count(x => x.Checked) == Players.Count)
            {
                EndPhase();
            }
        }

        private void EndPhase()
        {
            ResetChecks();

            if(Players.Count > 0)
            {
                switch (currentPhase)
                {
                    case Phase.Join:
                        currentPhase++;

                        Bot.Say("Vedonlyönti päättyy. Ota lisää kortteja komennolla !hit ja jää komennolla !c");

                        break;
                    case Phase.Players:
                        currentPhase++;

                        EndGame();

                        break;
                }
            }
            else
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            if (Players.Count < 1)
            {
                Bot.Say("Talo voittaa.");
                currentPhase = Phase.Over;
                return;
            }

            int cardCount = 0;

            int best = Players.Max(x => x.Score);

            int TotalScore = 0;

            int Aces = 0;

            while ((cardCount < 2) || (TotalScore < best && TotalScore <= 21 && cardCount >= 2))
            {

                var card = Decks.Draw();

                int score = CardScore(card);

                if (score == 11)
                {
                    Aces++;
                }

                TotalScore += score;

                if (TotalScore > 21 && Aces > 0)
                {
                    TotalScore -= 10;
                    Aces--;
                }

                PositionString message = "";

                message.Section("Talo: " + card.Name);
                message.SectionInsert(16, "Pisteet: " + TotalScore);

                Bot.Action(message);

                cardCount++;
            }

            if (TotalScore > best && TotalScore <= 21)
            {
                Bot.Say("Talo voittaa.");

                foreach (var player in Players)
                {
                    var newScore = new Score(player.UserId, Score.Blackjack).Started();
                    newScore.Gains(player.CurrentBet * -1);

                    player.User.UpdateScore(newScore);
                    Bot.Say(player.Username, "Hävisit: " + player.CurrentBet);
                }
            }
            else if (TotalScore == best)
            {
                Bot.Say("Tasapeli.");

                foreach (var player in Players)
                {
                    if (player.Score < best)
                    {
                        var newScore = new Score(player.UserId, Score.Blackjack).Started();
                        newScore.Gains(player.CurrentBet * -1);

                        player.User.UpdateScore(newScore);
                        Bot.Say(player.Username, "Hävisit: " + player.CurrentBet);
                    }
                    else
                    {
                        var newScore = new Score(player.UserId, Score.Blackjack).Started();

                        player.User.UpdateScore(newScore);
                    }
                }
            }
            else
            {
                if(TotalScore > 21)
                {
                    Bot.Say("Talolla yli!");
                }
                Bot.Say("Pelaajat voittaa.");

                foreach (var player in Players)
                {
                    var newScore = new Score(player.UserId, Score.Blackjack).Started();
                    newScore.Gains(player.CurrentBet);

                    player.User.UpdateScore(newScore);
                    Bot.Say(player.Username, "Voitit: " + player.CurrentBet);
                }
            }

            currentPhase = Phase.Over;
        }

        #endregion Game logic

        #region Private utility methods

        private int CardScore(Card card)
        {
            int score = card.Value;

            if(score == 14 || score == 1)
            {
                score = 11;
            }
            else if(score > 10)
            {
                score = 10;
            }

            return score;
        }

        private void ResetChecks()
        {
            foreach(var player in Players)
            {
                player.Checked = false;
            }
        }

        private void Raise(BlackjackPlayer player, int raise)
        {
            int newBet = player.CurrentBet + raise;
            if (newBet > MaximumBet)
            {
                newBet = MaximumBet;
            }

            if (player.CurrentBet != newBet)
            {
                player.CurrentBet = newBet;
                Bot.Say(player.Username + " bettaa: " + player.CurrentBet);
            }
            player.Checked = true;
            CheckStatuses();
        }

        /// <summary>
        /// Check if valid player and output the player from the player list
        /// </summary>
        private bool ValidPlayer(Msg message, out BlackjackPlayer player)
        {
            player = Players.FirstOrDefault(x => x.UserId == message.From.UserId);
            return player != null;
        }

        #endregion Private utility methods

    }

    class BlackjackPlayer
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

        public int CurrentBet { get; set; }

        public bool Checked { get; set; }

        public int Score { get; set; }

        public int Aces { get; set; }

        public BlackjackPlayer(User user)
        {
            User = user;
            CurrentBet = Blackjack.MinimumBet;
            Checked = false;
            Score = 0;
            Aces = 0;
        }
    }
}
