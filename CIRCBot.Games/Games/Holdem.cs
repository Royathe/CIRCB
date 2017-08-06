using CIRCBot.Games.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CIRCBot.Games
{
    class Holdem : GameBase, IGame
    {

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

        #region Constants

        public const int EntryFee = 500;

        public const int MinimumBet = 1500;

        public const int MaximumBet = BaseFunds;

        public const int MinimumRaise = 1000;

        public const int BaseFunds = 100000;

        public const int RoundTimeLimit = 80;

        public const int RoundEndWarningTimeLimit = 10;

        #endregion Constants

        #region Private accessors

        private List<HoldemPlayer> Players { get; set; }

        private Deck Deck { get; }

        private List<Card> River { get; set; }

        private int CurrentBet { get; set; }

        private int Pot { get; set; }

        #endregion Private accessors

        #region Constructor

        public Holdem(BotMessager messager) : base(messager)
        {
            CurrentPhase = Phase.Join;
            Players = new List<HoldemPlayer>();
            Deck = new Deck();
            River = new List<Card>();
            CurrentBet = MinimumBet;
            Pot = 0;

            Bot.Say("Holdem peli aloitettu. Aloituspanos " + EntryFee + ". Peliin voi liittyä komennolla !holdem. Aloituksen voi perua komennolla !cancel. Peli vaatii vähintään 2 pelaajaa.");
        }

        #endregion Constructor

        #region Public methods
        
        /// <summary>
        /// Only allow public messages
        /// </summary>
        public bool CommandIsValid(Msg message)
        {
            return !message.IsPrivate;
        }

        public bool Join(User user)
        {
            if (CurrentPhase == Phase.Join)
            {
                AddPlayer(user);

                if (Players.Count > 1)
                {
                    Timer.Set(RoundTimeLimit, EndPhase);
                    Bot.Say("Pelaajilla on " + RoundTimeLimit + " sekuntia aikaa liittyä.");
                }
                return true;
            }
            else
            {
                Bot.Say(user.Username, "Et voi enää liittyä peliin.");
            }
            return false;
        }

        public bool Check(Msg message)
        {
            if (IsPlayer(message.From))
            {
                var Player = GetPlayer(message);

                if (!Player.Raising)
                {
                    Bot.Say(Player.PlayerName, "Checkkaat.");
                }

                if(CurrentBet == Player.Funds)
                {
                    Player.Allin = true;
                }
                else
                {
                    Player.Checked = true;
                }

                CheckStatus();
                return true;
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Fold(Msg message)
        {
            if (IsPlayer(message.From))
            {
                if(CurrentPhase > Phase.Join)
                {
                    var Player = GetPlayer(message);

                    if(!Player.Checked && !Player.Allin)
                    {
                        PlayerLose(Player);
                        CheckStatus();
                        return true;
                    }
                }
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Cancel(Msg message)
        {
            if (IsPlayer(message.From))
            {
                if(Players.Count == 1 && CurrentPhase == Phase.Join)
                {
                    Bot.Say("Pelin aloitus peruttu");
                    GameEnd();
                    return true;
                }
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Allin(Msg message)
        {
            if (IsPlayer(message.From))
            {
                if(CurrentPhase > Phase.Join)
                {
                    var Player = GetPlayer(message);

                    if(!Player.Allin && !Player.Checked)
                    {
                        SetNewBet(Player.Funds, Player);

                        Check(message);
                        return true;
                    }
                }
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Hit(Msg message)
        {
            if (IsPlayer(message.From))
            {

            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Bet(Msg message)
        {
            if (IsPlayer(message.From))
            {
                if(CurrentPhase > Phase.Join)
                {
                    var Player = GetPlayer(message);

                    if(!Player.Raising && !Player.Allin)
                    {
                        int raise;

                        if(int.TryParse(message.NextCommand(), out raise))
                        {
                            // If less than minimum, set to minimum
                            if(raise < MinimumRaise)
                            {
                                raise = MinimumRaise;
                            }

                            var newBet = CurrentBet + raise;
                            SetNewBet(newBet, Player);
                            Player.Raising = true;

                            Check(message);
                            return true;
                        }
                    }
                }
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        #endregion Public methods

        #region Game logic methods

        private void EndPhase()
        {
            SetBasicTimer();
            ResetPlayerRaises();

            if(Players.Count > 0)
            {
                switch (CurrentPhase)
                {
                    // Ending the joining phase, moving to Draw
                    case Phase.Join:
                        if (Players.Count > 1)
                        {
                            // Draw cards for players
                            DrawPlayerCards();
                            ResetPlayerChecks();
                            CurrentPhase = CurrentPhase + 1;

                            CurrentBet = MinimumBet;

                            Bot.Say("Vedonlyönti alkaa. Minimi betti " + MinimumBet + ". Aikaa " + (RoundTimeLimit + RoundEndWarningTimeLimit) + " sekuntia.");
                        }
                        else
                        {
                            // Not enough players to continue. Stop the game.
                            NotEnoughPlayers();
                        }
                        break;

                    // Ending the draw phase, moving to flop
                    case Phase.Draw:
                        if(Players.Count > 1)
                        {
                            ResetPlayerChecks();
                            CurrentPhase = CurrentPhase + 1;

                            River.Add(Deck.Draw());
                            Deck.Draw();
                            River.Add(Deck.Draw());
                            Deck.Draw();
                            River.Add(Deck.Draw());

                            ReportCards();

                            CheckStatus();
                        }
                        else
                        {
                            GameEnd();
                        }
                        break;

                    // Ending the flop phase, moving to turn
                    case Phase.Flop:

                        if (Players.Count > 1)
                        {
                            ResetPlayerChecks();
                            CurrentPhase = CurrentPhase + 1;
                            
                            DrawNext();

                            CheckStatus();
                        }
                        else
                        {
                            GameEnd();
                        }
                        break;

                    // Ending the turn phase, moving to river
                    case Phase.Turn:

                        if (Players.Count > 1)
                        {
                            ResetPlayerChecks();
                            CurrentPhase = CurrentPhase + 1;

                            DrawNext();

                            CheckStatus();
                        }
                        else
                        {
                            GameEnd();
                        }
                        break;

                    // Ending the river phase, moving to gameend
                    case Phase.River:
                        
                        GameEnd();
                        break;
                }
            }
            else
            {
                Timer.Stop();
                NotEnoughPlayers();
            }

        }

        private void DrawPlayerCards()
        {
            foreach(HoldemPlayer player in Players)
            {
                player.Cards[0] = Deck.Draw();
                player.Cards[1] = Deck.Draw();

                // Log games played
                var user = Users.Get(player.PlayerName);

                user.UpdateScore(
                    new Score(user.UserId, Score.Holdem).Started()
                    );

                // Report cards to the player as a private message
                var message = "------------------------------------------------";
                message += player.Cards[0].Name;
                message += " | ";
                message += player.Cards[1].Name;

                Bot.Say(player.PlayerName, message);
            }
        }

        private void ResetPlayerChecks()
        {
            foreach(HoldemPlayer player in Players)
            {
                player.Checked = false;
                player.Raising = false;
            }
        }

        private void ResetPlayerRaises()
        {
            foreach (HoldemPlayer player in Players)
            {
                player.Raising = false;
            }
        }

        private void FoldNotCheckedPlayers()
        {
            List<HoldemPlayer> playersToRemove = new List<HoldemPlayer>();

            foreach (var player in Players)
            {
                if(!player.Checked && !player.Allin)
                {
                    playersToRemove.Add(player);
                }
            }

            foreach(var player in playersToRemove)
            {
                PlayerLose(player, true);
            }

            CheckStatus();
        }

        private void CheckStatus()
        {
            // Number of players checking
            int checkingCount = 0;

            foreach(var player in Players)
            {
                if (player.Allin || player.Checked)
                {
                    checkingCount += 1;

                    if(CurrentBet > player.Funds)
                    {
                        CurrentBet = player.Funds;
                        Bot.Say("Bet allennettu => " + CurrentBet);

                        // Recall after altered bet.
                        CheckStatus();
                        return;
                    }
                    player.Bet = CurrentBet;
                }
            }

            bool allChecked = checkingCount == Players.Count || Players.Count == 1;

            if (allChecked)
            {
                switch (CurrentPhase)
                {
                    case Phase.Join:
                        if (Players.Count > 1)
                        {
                            EndPhase();
                        }
                        break;
                    default:
                        EndPhase();
                        break;
                }
            }
        }

        private void GameEnd()
        {
            Timer.Stop();

            if(CurrentPhase > Phase.Join)
            {
                // Process bets to pot.
                foreach(var player in Players)
                {
                    Pot += player.Bet;
                    player.CurrentWin = 0 - player.Bet;
                }

                // Check if game has multiple players left.
                if(Players.Count > 1)
                {
                    // Public message of each player's hands.
                    string message = "";

                    // List of players' hands.
                    List<Hand> Hands = new List<Hand>();

                    // Current hand being processed.
                    Hand myHand;

                    // Process each player's cards to their highest value combination
                    foreach (var player in Players)
                    {
                        // Get player's best hand
                        myHand = new Hand(player.Cards.Concat(River).ToArray());
                        myHand.Owner = Users.Get(player.PlayerName);


                        // Start message with current's player's name in capital letters
                        message = player.PlayerName.ToUpper();

                        message += "| " + myHand.Text;

                        // Report player's hand and all of his cards privately
                        string privMessage = myHand.Text + " | -";

                        foreach(var card in myHand.Cards)
                        {
                            privMessage += " | " + card.Name;
                        }

                        Bot.Say(player.PlayerName, privMessage);

                        Hands.Add(myHand);
                        Bot.Action(message);
                    }

                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("");

                    Console.WriteLine("HANDS: ");

                    foreach(var hand in Hands)
                    {
                        Console.WriteLine(hand.Owner.Username);
                        Console.WriteLine(hand.HandScores);
                        Console.WriteLine(hand.CardScores);
                        Console.WriteLine("");
                    }

                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("____________________________________________________");

                    //Bot.Action(message);

                    // Get the winners
                    Hand[] WinningHands = Hand.ResolveWinningHands(Hands.ToArray());

                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("");

                    Console.WriteLine("WINNERS: ");

                    foreach(var winner in WinningHands)
                    {
                        Console.WriteLine(winner.Owner.Username);
                        Console.WriteLine(winner.HandScores);
                        Console.WriteLine(winner.CardScores);
                        Console.WriteLine("");
                    }

                    Console.WriteLine("____________________________________________________");
                    Console.WriteLine("____________________________________________________");

                    message = "";

                    int WinValue = 0;

                    if(WinningHands.Length == 1)
                    {
                        Hand WinningHand = WinningHands[0];

                        WinValue = Pot;

                        GetPlayer(WinningHand.Owner).CurrentWin += WinValue;

                        Bot.Say("VOITTAJA: " + WinningHand.Owner.Username.ToUpper() + " | " + WinningHand.Text + " " + WinningHand.TextAdd);
                    }
                    else
                    {
                        message = "TASAPELI: ";

                        WinValue = Pot / WinningHands.Length;

                        foreach (var WinningHand in WinningHands)
                        {
                            GetPlayer(WinningHand.Owner).CurrentWin += WinValue;
                            message += WinningHand.Owner.Username.ToUpper() + ", ";
                        }

                        message = message.Substring(0, message.Length - 2);

                        message += " ||| " + WinningHands[0].Text;

                        Bot.Say(message);
                    }

                    Bot.Say("Voitti: " + GetPlayer(WinningHands[0].Owner).CurrentWin);
                }
                else if(Players.Count == 1)
                {
                    Players[0].CurrentWin += Pot;
                    Bot.Say("VOITTAJA: " + Players[0].PlayerName.ToUpper());
                    Bot.Say("Voitti: " + Players[0].CurrentWin);
                }

                // Käsitellään voittajat
                ProcessWins();
            }

            CurrentPhase = Phase.Over;
        }

        private void ProcessWins()
        {
            foreach (var Player in Players)
            {
                Users.Get(Player.PlayerName).UpdateScore(
                    new Score(Player.PlayerUserId, Score.Holdem).Gains(Player.CurrentWin)
                    );
            }
        }

        #endregion Game logic methods

        #region General Private methods

        private HoldemPlayer GetPlayer(Msg message)
        {
            return Players.First(x => x.PlayerName == message.From.Username);
        }

        private HoldemPlayer GetPlayer(User user)
        {
            return Players.First(x => x.PlayerName == user.Username);
        }

        private void SetBasicTimer()
        {
            Timer.Set(RoundTimeLimit, WarningTimeOut);
        }

        private void SetNewBet(int newBet, HoldemPlayer player)
        {
            ResetPlayerChecks();
            SetBasicTimer();

            // Increase bet
            CurrentBet = newBet;

            if(CurrentBet > MaximumBet)
            {
                CurrentBet = MaximumBet;
            }
            if(CurrentBet > player.Funds)
            {
                CurrentBet = player.Funds;
            }

            Bot.Say("Veto nostettu => " + CurrentBet);
        }

        private void DrawNext()
        {
            if(CurrentPhase > Phase.Flop)
            {
                Deck.Draw();
                River.Add(Deck.Draw());

                ReportCards();
            }
        }

        private void ReportCards()
        {
            string message = "";

            foreach(var card in River)
            {
                message += " | " + card.Name;
            }

            Bot.Action(message);
        }
        
        private void PlayerLose(HoldemPlayer player, bool byTimeout = false)
        {
            Bot.Say(player.PlayerName, "Hävisit: " + player.Bet);
            Pot += player.Bet;
            Players.Remove(player);

            var PlayerScore = new Score(player.PlayerUserId, Score.Holdem);
            PlayerScore.TotalGains -= player.Bet;

            if (byTimeout)
            {
                PlayerScore.GamesForfeitted = 1;
            }
            Users.Get(player.PlayerName).UpdateScore(PlayerScore);
        }

        private void WarningTimeOut()
        {
            Bot.Say(RoundEndWarningTimeLimit + " sekuntia jäljellä!");
            Timer.Set(RoundEndWarningTimeLimit, TimeOut);
        }

        private void TimeOut()
        {
            if(CurrentPhase > Phase.Join)
            {
                Bot.Say("Aika loppui. Kaikki jotka eivät checkanneet foldaavat.");
                FoldNotCheckedPlayers();
            }
            else
            {
                Bot.Say("Aika loppui.");
                foreach(var player in Players)
                {
                    player.Checked = true;
                }
                CheckStatus();
            }
        }

        private void NotAPlayer(string username)
        {
            Bot.Say(username, "Et ole pelissä.");
        }

        private void NotEnoughPlayers()
        {
            Bot.Say("Ei tarpeeksi pelaajia. Lopetetaan Holdem.");
            GameEnd();
        }

        private bool IsPlayer(User user)
        {
            return Players.Any(x => x.PlayerUserId == user.UserId);
        }

        private void AddPlayer(User user)
        {
            if (!IsPlayer(user))
            {
                Players.Add(new HoldemPlayer(user));
            }
        }

        #endregion General Private methods

    }

    class HoldemPlayer : Player
    {
        public int Bet { get; set; }

        public int Funds { get; set; }

        public int CurrentWin { get; set; }

        public Card[] Cards { get; set; }

        public bool Checked { get; set; }

        public bool Allin { get; set; }

        public bool Raising { get; set; }

        public HoldemPlayer(User user) : base(user)
        {
            Bet = Holdem.EntryFee;
            Funds = Holdem.BaseFunds;
            Cards = new Card[2];
            Checked = false;
            Allin = false;
            Raising = false;
        }
    }
}
