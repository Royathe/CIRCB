using CIRCBot.Games.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CIRCBot.Games
{
    class HoldemTournament : GameBase, IGame
    {

        #region Constants

        public const int BlindIncreaseMultiplier = 2;

        public const int BlindIncreaseRoundIncrement = 10;

        #endregion Constants
        
        #region Phases

        private enum Phase
        {
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

        private List<TournamentPlayer> Players { get; set; }

        private TournamentPlayer[] ActivePlayers
        {
            get
            {
                return Players.Where(x => x.Active).ToArray();
            }
        }

        private int CurrentDealerIndex { get; set; }

        private int CurrentPlayerIndex { get; set; }

        private int CurrentPlayerId
        {
            get
            {
                return Players[CurrentPlayerIndex].UserId;
            }
        }

        private int SmallBlind { get; set; }

        private int BigBlind { get; set; }

        private Deck Deck { get; }

        private List<Card> Table { get; set; }

        private int CurrentBet { get; set; }

        /// <summary>
        /// Gets the currently active pot.
        /// </summary>
        private GamePot Pot
        {
            get
            {
                return pots[currentPot];
            }
        }

        private List<GamePot> pots { get; set; }

        private int currentPot
        {
            get
            {
                return pots.Count - 1;
            }
        }

        #endregion Private accessors

        #region Constructors

        public HoldemTournament(BotMessager messager) : base(messager)
        {

            CurrentPhase = Phase.Draw;
            Players = new List<TournamentPlayer>();
            Deck = new Deck();
            Table = new List<Card>();
            CurrentBet = 0;
            pots = new List<GamePot>();
            pots.Add(new GamePot());

            var playerUserIds = Users.Scores(Score.Tournament).Select(x => x.UserId).Distinct();

            foreach(var playerUserId in playerUserIds)
            {
                Players.Add(new TournamentPlayer(playerUserId));
            }

            var currentDealerUserId = TournmentLoader.GetDealerUserId();

            // Get the current dealers index if userId != 0
            if(currentDealerUserId != 0)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].UserId == currentDealerUserId)
                    {
                        CurrentDealerIndex = i;
                        break;
                    }
                }

                CurrentDealerIndex = NextActivePlayerIndex(CurrentDealerIndex, false);
            }
            else
            {
                // Pick a random dealer
                CurrentDealerIndex = new Random().Next(Players.Count);
            }

            // Get the current blind and the number of rounds played so far (plus the current new round).
            int blind = TournmentLoader.BaseBlind;
            int rounds = TournmentLoader.GetRoundsPlayed() + 1;
            int totalRounds = rounds;

            // Substract the BlindIncreaseRoundIncrement from the rounds
            rounds = rounds - BlindIncreaseRoundIncrement;

            // Number of times the blind should be raised.
            int increments = 0;

            while(rounds > 0)
            {
                increments++;
                rounds -= BlindIncreaseRoundIncrement;
            }

            // Multiply the blind for each passed increment.
            for(int i = 0; i < increments; i++)
            {
                blind = blind * BlindIncreaseMultiplier;
            }

            // Set the current small and big blinds.
            SmallBlind = blind;
            BigBlind = SmallBlind * 2;

            // The player after the dealer has the Small Blind.
            int smallBlindIndex = NextActivePlayerIndex(CurrentDealerIndex, false);
            if(totalRounds > 1)
            {
                CurrentPlayerIndex = smallBlindIndex;
            }
            Players[smallBlindIndex].CurrentBet = SmallBlind;

            // The player after the Small Blind has the Big Blind.
            int bigBlindIndex = NextActivePlayerIndex(smallBlindIndex, false);
            Players[bigBlindIndex].CurrentBet = BigBlind;

            // Kirjataan pelaajat.
            string message = "Holdem Turnauksen pelaajat: ";

            for (int j = 0; j < Players.Count; j++)
            {
                var player = Players[j];

                if (player.Active)
                {
                    message += player.Username;

                    if (j == bigBlindIndex)
                    {
                        message += String.Format("(Big Blind - {0}), ", BigBlind);
                    }
                    else if (j == smallBlindIndex)
                    {
                        message += String.Format("(Small Blind - {0}), ", SmallBlind);
                    }
                    else if (j == CurrentDealerIndex)
                    {
                        message += "(Dealer), ";
                    }
                    else
                    {
                        message += ", ";
                    }
                }
            }

            message = message.Substring(0, message.Length - 2);
            Say(message);

            foreach(var player in ActivePlayers)
            {
                Pot.PlayerPots.Add(new PlayerPot(player.UserId, player.CurrentBet));

                player.Cards = new Card[2];
                player.Cards[0] = Deck.Draw();
                player.Cards[1] = Deck.Draw();

                // Log games played
                var user = Users.Get(player.UserId);

                user.UpdateScore(
                    new Score(user.UserId, Score.Tournament).Started()
                    );

                // Report cards to the player as a private message
                message = "------------------------------------------------";
                message += player.Cards[0].Name;
                message += " | ";
                message += player.Cards[1].Name;

                Bot.Say(player.Username, message);
            }

            InformOfPlayerTurn();

            // Set the current bet.
            CurrentBet = BigBlind;
        }

        #endregion Constructors

        #region Public methods

        public bool Join(User user)
        {
            return false;
        }

        public bool Check(Msg message)
        {
            if (ValidPlayer(message.From))
            {
                var player = Players.FirstOrDefault(x => x.UserId == message.From.UserId);

                if (!player.Allin && !player.Checked)
                {
                    CheckPlayer(player);
                    CheckStatuses();
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
            if (ValidPlayer(message.From))
            {
                var player = Players.FirstOrDefault(x => x.UserId == message.From.UserId);

                if(!player.Allin && !player.Checked)
                {
                    SetNewBet(player.StartingFunds, player);
                    CheckPlayer(player);
                    CheckStatuses();
                    return true;
                }
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Bet(Msg message)
        {
            if (ValidPlayer(message.From))
            {
                var player = Players.FirstOrDefault(x => x.UserId == message.From.UserId);

                if (!player.Allin)
                {
                    int raise;

                    if (int.TryParse(message.NextCommand(), out raise))
                    {
                        // If less than minimum, set to minimum
                        if (raise < BigBlind)
                        {
                            raise = BigBlind;
                        }

                        var newBet = CurrentBet + raise;
                        SetNewBet(newBet, player);
                        CheckPlayer(player);
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
            return false;
        }

        public bool CommandIsValid(Msg message)
        {
            return true;
        }

        public bool Fold(Msg message)
        {
            if (ValidPlayer(message.From))
            {
                var player = Players.FirstOrDefault(x => x.UserId == message.From.UserId);

                if (player.Active && !player.Allin && !player.Checked)
                {
                    player.Active = false;
                    Bot.Say(player.Username, "Foldaat.");
                    if (ActivePlayers.Count(x => x.Allin) == ActivePlayers.Length - 1 && ActivePlayers.First(x => !x.Allin).Checked)
                    {
                        foreach (var player2 in ActivePlayers)
                        {
                            player2.Allin = true;
                        }
                    }
                }
                if (!CheckStatuses())
                {
                    CurrentPlayerIndex = NextActivePlayerIndex(CurrentPlayerIndex);
                }
                return true;
            }
            else
            {
                NotAPlayer(message.From.Username);
            }
            return false;
        }

        public bool Hit(Msg message)
        {
            return false;
        }

        #endregion Public methods

        #region Game Logic

        private void SetNewBet(int newBet, TournamentPlayer player)
        {
            ResetChecks();

            // Increase bet
            CurrentBet = newBet;

            if (CurrentBet > player.StartingFunds)
            {
                CurrentBet = player.StartingFunds;
            }

            Say("Veto nostettu => " + CurrentBet);
        }

        private void NewSidePot(int sidePotMaxFunds, GamePot basePot)
        {
            int basePotIndex = 0;
            for(int i = 0; i < pots.Count; i++)
            {
                if(pots[i] == basePot)
                {
                    basePotIndex = i;
                    break;
                }
            }

            pots.Insert(0, basePot);
            basePotIndex++;

            int sidePotIndex = 0;

            pots[sidePotIndex] = new GamePot();

            GamePot sidePot = pots[sidePotIndex];

            List<PlayerPot> playerPotsToRemoveFromCurrentPot = new List<PlayerPot>();

            foreach (var playerPot in basePot.PlayerPots)
            {
                if (playerPot.Funds > sidePotMaxFunds)
                {
                    sidePot.PlayerPots.Add(
                        new PlayerPot(playerPot.UserId, sidePotMaxFunds)
                        );

                    playerPot.Funds -= sidePotMaxFunds;
                }
                else
                {
                    sidePot.PlayerPots.Add(
                        new PlayerPot(playerPot.UserId, playerPot.Funds)
                        );

                    playerPotsToRemoveFromCurrentPot.Add(playerPot);
                }
            }

            foreach(var playerPot in playerPotsToRemoveFromCurrentPot)
            {
                basePot.PlayerPots.Remove(playerPot);
            }
        }

        /// <summary>
        /// Player is checked and the current bets set to their pots.
        /// </summary>
        private void CheckPlayer(TournamentPlayer player)
        {
            if (!player.Allin)
            {
                // Calculate the player's maximum available funds.
                int playerFunds = PlayerFunds(player);

                // Set the player's bets into previous pots first before processing the current pot.
                if (pots.Count > 1)
                {
                    foreach (var pot in pots)
                    {
                        var playerPot = pot.Get(player.UserId);

                        if (playerPot != null)
                        {
                            if (playerPot.Funds < pot.Max)
                            {
                                if (playerFunds < pot.Max)
                                {
                                    // If the player cannot fully check into a previous sidepot, create a new sidepot. Player is now allin, and a part of the new sidepot.
                                    Pot.Get(player.UserId).Funds = playerFunds;
                                    NewSidePot(playerFunds, pot);
                                    player.Allin = true;
                                    Say(player.Username + " on Allin.");

                                    CheckAllins();
                                    return;
                                }
                                else
                                {
                                    // Set player's funds to the previous pot first and then recalculate their remaining funds before the next iteration.
                                    playerPot.Funds = pot.Max;
                                    playerFunds = PlayerFunds(player);
                                }
                            }
                        }
                        else
                        {
                            // If player doesn't have a bet in the current pot, add a new playerpot and recall CheckPlayer.
                            pot.PlayerPots.Add(new PlayerPot(player.UserId, 0));
                            CheckPlayer(player);
                            return;
                        }
                    }
                }

                // Get the player's current bet, ignoring the current pot
                playerFunds = PlayerBet(player, true);

                if (player.StartingFunds < CurrentBet)
                {
                    Pot.Get(player.UserId).Funds = player.StartingFunds;
                    NewSidePot(player.StartingFunds, Pot);
                    player.Allin = true;
                    Say(player.Username + " on Allin.");
                }
                else
                {
                    if(player.StartingFunds == CurrentBet)
                    {
                        player.Allin = true;
                    }
                    Pot.Get(player.UserId).Funds = CurrentBet - playerFunds;
                    player.Checked = true;
                }
            }

            CheckAllins();
        }

        /// <summary>
        /// Check if only one player is left that isn't allin.
        /// </summary>
        private void CheckAllins()
        {

            if(ActivePlayers.Count(x => x.Allin) == ActivePlayers.Length - 1 && ActivePlayers.First(x => !x.Allin).Checked)
            {
                foreach (var player2 in ActivePlayers)
                {
                    player2.Allin = true;
                }
            }
            else
            {
                CurrentPlayerIndex = NextActivePlayerIndex(CurrentPlayerIndex);
            }
            // If only one player isn't allin, check everyone as allin. No further actions can be taken.
            //if (ActivePlayers.Where(x => !x.Allin && x.Checked).Count() <= 1)
            //{
            //    foreach (var player2 in ActivePlayers)
            //    {
            //        player2.Allin = true;
            //    }
            //}
            //else
            //{
            //    CurrentPlayerIndex = NextActivePlayerIndex(CurrentPlayerIndex);
            //}
        }

        /// <summary>
        /// Clean up the pots. Pots that are left with 0 value bets, or with just one player, are removed.
        /// </summary>
        private void CleanUpPots()
        {
            // Check pots that have only one player in them, or have no funds assigned.
            var potsToRemove = pots.Where(x => x.PlayerPots.Count <= 1 || !x.PlayerPots.Any(x2 => x2.Funds != 0)).ToArray();

            // If found, delete them.
            if (potsToRemove.Length > 0)
            {
                foreach (var potToRemove in potsToRemove)
                {
                    pots.Remove(potToRemove);
                }
            }
        }

        private bool CheckStatuses()
        {
            // Number of players checking
            int checkingCount = 0;

            // Auto-continue if only 1 player left.
            if(ActivePlayers.Length == 1)
            {
                checkingCount = 1;
            }
            else
            {
                foreach (var player in ActivePlayers)
                {
                    if (player.Checked || player.Allin)
                    {
                        checkingCount += 1;
                    }
                }
            }

            if(checkingCount >= ActivePlayers.Length)
            {
                EndPhase();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EndPhase()
        {
            // Clean up the pots
            CleanUpPots();
            ResetChecks();

            if(ActivePlayers.Length > 0)
            {
                switch (CurrentPhase)
                {
                    // Ending the draw phase, moving to flop
                    case Phase.Draw:
                        if (ActivePlayers.Length > 1)
                        {
                            CurrentPhase = CurrentPhase + 1;

                            Deck.Draw();
                            Table.Add(Deck.Draw());
                            Table.Add(Deck.Draw());
                            Table.Add(Deck.Draw());

                            ReportCards();

                            CheckStatuses();
                        }
                        else
                        {
                            EndGame();
                        }
                        break;

                    // Ending the flop/turn phase, moving to turn
                    case Phase.Flop:
                    case Phase.Turn:

                        if (ActivePlayers.Length > 1)
                        {
                            CurrentPhase = CurrentPhase + 1;

                            DrawNext();

                            CheckStatuses();
                        }
                        else
                        {
                            EndGame();
                        }
                        break;

                    // Ending the river phase, moving to gameend
                    case Phase.River:

                        EndGame();
                        break;
                }
            }
            else
            {
                NotEnoughPlayers();
            }

            if(CurrentPhase != Phase.Over)
            {
                // If there are still any active players who are not allin.
                if (ActivePlayers.Any(x => !x.Allin))
                {
                    // Set next active player to the next player after the dealer.
                    CurrentPlayerIndex = NextActivePlayerIndex(CurrentDealerIndex);
                }
                else
                {
                    // Go through the rest of the phases.
                    EndPhase();
                }
            }
        }

        private void EndGame()
        {
            // Substract the players contributions to the pots from the gains
            foreach (var player in Players)
            {
                player.CurrentGains = 0 - PlayerBet(player);
            }

            // Check if game has multiple players left.
            if (ActivePlayers.Length > 0)
            {
                ProcessPlayerHands();

                ProcessPotWins();
            }
            else
            {
                // No players left. Game cancelled.
                CurrentPhase = Phase.Over;
                return;
            }

            // Update the dealer Id for the next round.
            TournmentLoader.UpdateDealerUserId(Players[CurrentDealerIndex].UserId);

            CurrentPhase = Phase.Over;
        }

        /// <summary>
        /// Processes active players cards to a Hand.
        /// </summary>
        private void ProcessPlayerHands()
        {
            // Public message of each player's hands.
            string message = "";

            // Process each player's cards to their highest value combination
            foreach (var player in ActivePlayers)
            {
                // Get player's best hand
                player.Hand = new Hand(player.Cards.Concat(Table).ToArray());
                player.Hand.Owner = Users.Get(player.UserId);

                // Start message with current's player's name in capital letters
                message = player.Username.ToUpper();

                message += "| " + player.Hand.Text;

                // Report player's hand and all of his cards privately
                string privMessage = player.Hand.Text + " | -";

                foreach (var card in player.Hand.Cards)
                {
                    privMessage += " | " + card.Name;
                }

                Bot.Say(player.Username, privMessage);

                if(ActivePlayers.Length > 1)
                {
                    Bot.Action(message);
                }
            }
        }

        private void ProcessPotWins()
        {
            int potNumber = 1;
            foreach(GamePot pot in pots)
            {
                List<Hand> hands = new List<Hand>();

                PositionString PotDescription = "";

                PotDescription.Section(10, "Potti " + potNumber);
                PotDescription.Section(10, pot.Total.ToString());

                string playersInPot = "";
                string winnersOfPot = "";

                // Get the hands of the players in the current pot.
                foreach (var player in ActivePlayers)
                {
                    var playerPot = pot.PlayerPots.FirstOrDefault(x => x.UserId == player.UserId);

                    if(playerPot != null)
                    {
                        playersInPot += player.Username + ", ";
                        hands.Add(player.Hand);
                    }
                }

                playersInPot = playersInPot.Substring(0, playersInPot.Length - 2);
                PotDescription.Section(40, playersInPot);

                // Of only logging one pot, simplify the pot message to just the winners and the win total.
                if(pots.Count == 1)
                {
                    PotDescription = new PositionString();
                }

                // Get the winner(s) of the current pot
                Hand[] winningHands = Hand.ResolveWinningHands(hands.ToArray());

                if(winningHands.Length > 1)
                {
                    winnersOfPot += "VOITTAJAT: ";
                }
                else
                {
                    winnersOfPot += "VOITTAJA: ";
                }

                // Each winner's portion of the pot
                int winTotal = pot.Total / winningHands.Length;

                foreach(var winner in winningHands)
                {
                    var player = Players.First(x => x.UserId == winner.Owner.UserId);

                    winnersOfPot += player.Username.ToUpper() + ", ";

                    player.CurrentGains += winTotal;
                }

                winnersOfPot = winnersOfPot.Substring(0, winnersOfPot.Length - 2);
                
                PotDescription.Section(35, winnersOfPot);
                if(ActivePlayers.Length > 1)
                {
                    PotDescription.Section(30, winningHands[0].Text + " " + winningHands[0].TextAdd);
                }
                PotDescription.Section(25, "VOITTI: " + winTotal);
                PotDescription.Section();

                // Log the pot.
                Say(PotDescription);

                potNumber++;
            }

            // Update the scores
            foreach (var player in Players)
            {
                var user = Users.Get(player.UserId);
                var newScore = new Score(player.UserId, Score.Tournament);

                newScore.Gains(player.CurrentGains);

                if (newScore.TotalGains >= 0)
                {
                    Bot.Say(player.Username, "Voitit: " + newScore.TotalGains);
                }
                else
                {
                    Bot.Say(player.Username, "Hävisit: " + (newScore.TotalGains * -1));
                }
                user.UpdateScore(newScore);
            }
        }

        /*
        private void GameEnd404()
        {
            // Substract the players contributions to the pots from the gains
            foreach (var player in Players)
            {
                player.CurrentGains = 0 - PlayerBet(player);
            }

            // Check if game has multiple players left.
            if (ActivePlayers.Length > 1)
            {
                // Public message of each player's hands.
                string message = "";
                
                // List of players' hands.
                List<Hand> Hands = new List<Hand>();

                // Current hand being processed.
                Hand myHand;

                // Process each player's cards to their highest value combination
                foreach (var player in ActivePlayers)
                {
                    // Get player's best hand
                    myHand = new Hand(player.Cards.Concat(Table).ToArray());
                    myHand.Owner = Users.Get(player.UserId);
                    
                    // Start message with current's player's name in capital letters
                    message = player.Username.ToUpper();

                    message += "| " + myHand.Text;

                    // Report player's hand and all of his cards privately
                    string privMessage = myHand.Text + " | -";

                    foreach (var card in myHand.Cards)
                    {
                        privMessage += " | " + card.Name;
                    }

                    Bot.Say(player.Username, privMessage);

                    Hands.Add(myHand);
                    Bot.Action(message);
                }


                // Get the winners
                Hand[] WinningHands = Hand.ResolveWinningHands(Hands.ToArray());

                if (WinningHands.Length == 1)
                {
                    Hand WinningHand = WinningHands[0];

                    Say("VOITTAJA: " + WinningHand.Owner.Username.ToUpper() + " | " + WinningHand.Text + " " + WinningHand.TextAdd);
                    
                    ProcessPots(
                        Players.Where(x => x.UserId == WinningHand.Owner.UserId).ToArray()
                        );
                }
                else
                {
                    message = "TASAPELI: ";

                    List<TournamentPlayer> winners = new List<TournamentPlayer>();

                    foreach (var WinningHand in WinningHands)
                    {
                        winners.Add(
                            Players.First(x => x.UserId == WinningHand.Owner.UserId)
                            );
                        message += WinningHand.Owner.Username.ToUpper() + ", ";
                    }

                    message = message.Substring(0, message.Length - 2);

                    message += " ||| " + WinningHands[0].Text;

                    Say(message);

                    ProcessPots(winners.ToArray());
                }
            }
            else if (ActivePlayers.Length == 1)
            {
                Say("VOITTAJA: " + ActivePlayers[0].Username.ToUpper());

                ProcessPots(ActivePlayers);
            }
            else
            {
                // No players left. Game cancelled.
                CurrentPhase = Phase.Over;
                return;
            }
            
            // Update the dealer Id for the next round.
            TournmentLoader.UpdateDealerUserId(Players[CurrentDealerIndex].UserId);

            CurrentPhase = Phase.Over;
        }

        /// <summary>
        /// Processes the pots and updates the scores. Must have winners.
        /// </summary>
        /// <param name="winners"></param>
        private void ProcessPots404(TournamentPlayer[] winners)
        {
            int potTotal = 0;

            foreach (var winner in winners)
            {
                var playerPots = PlayersPots(winner);
                potTotal = 0;
                foreach (var pot in playerPots)
                {
                    potTotal += pot.Total;
                    pot.Handled = true;
                }

                potTotal = potTotal / winners.Length;

                winner.CurrentGains += potTotal;
            }
            
            Say("VOITTI: " + winners[0].CurrentGains);

            // Unhandled pots
            foreach (var pot in pots.Where(x => x.Handled == false))
            {
                //pot.PlayerPots = new List<PlayerPot>();
                foreach (var playerPot in pot.PlayerPots)
                {
                    var player = Players.FirstOrDefault(x => x.UserId == playerPot.UserId);
                    if (player != null)
                    {
                        player.CurrentGains += playerPot.Funds;
                    }

                }
            }

            // Update the scores
            foreach (var player in Players)
            {
                var user = Users.Get(player.UserId);
                var newScore = new Score(player.UserId, Score.Tournament);

                newScore.Gains(player.CurrentGains);

                if(newScore.TotalGains >= 0)
                {
                    Bot.Say(player.Username, "Voitit: " + newScore.TotalGains);
                }
                else
                {
                    Bot.Say(player.Username, "Hävisit: " + (newScore.TotalGains * -1));
                }
                user.UpdateScore(newScore);
            }
        }
        */

        #endregion Game Logic

        #region Private utility methods

        private void Say(string message)
        {
            Bot.Say(message.Colorize(Options.ColorOptions.TournamentTextColor));
        }

        private void DrawNext()
        {
            if (CurrentPhase > Phase.Flop)
            {
                Deck.Draw();
                Table.Add(Deck.Draw());

                ReportCards();
            }
        }

        private void ReportCards()
        {
            string message = "";

            foreach (var card in Table)
            {
                message += " | " + card.Name;
            }

            Bot.Action(message);
        }

        private void ResetChecks()
        {
            foreach (var player in Players)
            {
                player.Checked = false;
            }
        }

        private void SetNextPlayer()
        {
            CurrentPlayerIndex = NextActivePlayerIndex(CurrentPlayerIndex);
        }

        private void NotEnoughPlayers()
        {
            Say("Ei tarpeeksi pelaajia. Lopetetaan Holdem.");
            EndGame();
        }

        private void NotAPlayer(string username)
        {
            Bot.Say(username, "Et ole pelissä tai ei ole sinun vuorosi.");
        }

        private bool ValidPlayer(User user)
        {
            var player = Players.FirstOrDefault(x => x.UserId == user.UserId);
            if(player != null)
            {
                if(player.UserId == CurrentPlayerId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the player's bet total in the pots
        /// </summary>
        private int PlayerBet(TournamentPlayer player, bool ignoreLatestPot = false)
        {
            int sum = 0;
            foreach (GamePot pot in pots.Where(x => x.Has(player.UserId)))
            {
                if(ignoreLatestPot == false || pot != Pot)
                {
                    sum += pot.Get(player.UserId).Funds;
                }
            }
            return sum;
        }

        /// <summary>
        /// Gets the total amount the player has placed into the pots
        /// </summary>
        private int PlayerFunds(TournamentPlayer player)
        {
            int totalBets = PlayerBet(player);
            return player.StartingFunds - totalBets;
        }

        private GamePot[] PlayersPots(TournamentPlayer player)
        {
            List<GamePot> playersPots = new List<GamePot>();
            foreach(GamePot pot in pots)
            {
                if (pot.Has(player.UserId))
                {
                    playersPots.Add(pot);
                }
            }
            return playersPots.ToArray();
        }

        /// <summary>
        /// Get the index of the next active player after the given index.
        /// </summary>
        private int NextActivePlayerIndex(int currentIndex, bool informOfTurn = true)
        {
            currentIndex++;
            if (currentIndex >= Players.Count)
            {
                currentIndex = 0;
            }

            int loopCount = 0;

            while (loopCount < 100 && (!Players[currentIndex].Active || Players[currentIndex].Allin))
            {
                currentIndex++;
                if (currentIndex >= Players.Count)
                {
                    currentIndex = 0;
                }
                loopCount++;
            }

            if (informOfTurn && ActivePlayers.Where(x => x.Checked).Count() != ActivePlayers.Length)
            {
                InformOfPlayerTurn(currentIndex);
            }

            return currentIndex;
        }

        private void InformOfPlayerTurn(int currentIndex = -1)
        {
            if(currentIndex == -1)
            {
                currentIndex = CurrentPlayerIndex;
            }
            
            Say("Vuoro: " + Players[currentIndex].Username);
        }

        #endregion Private utility methods

    }

    /// <summary>
    /// Loads Tournament settings from the database.
    /// </summary>
    public static class TournmentLoader
    {
        /// <summary>
        /// ParamAttName for Tournamet params
        /// </summary>
        private const string TournamentParamAttName = "Tournament";

        /// <summary>
        /// Turnauksen aloitus blindi
        /// </summary>
        public const int BaseBlind = 100;

        /// <summary>
        /// ParamText values
        /// </summary>
        private enum TournamentParams
        {
            Dealer,
            RoundsPlayed
        }

        /// <summary>
        /// Adds the user to the current season's tournament. Total Gains set to starting funds.
        /// </summary>
        public static void JoinTournament(User user)
        {
            var tournamentScore = new Score(user.UserId, Score.Tournament);
            tournamentScore.TotalGains = 1000000;
            tournamentScore.GamesPlayed = 0;
            user.UpdateScore(tournamentScore);
        }

        /// <summary>
        /// Checks if the user is a part of the current season's tournament.
        /// </summary>
        public static bool UserIsInTournament(User user)
        {
            return Users.Scores(Score.Tournament).FirstOrDefault(x => x.UserId == user.UserId) != null;
        }

        /// <summary>
        /// Get all the users in the current tournament.
        /// </summary>
        public static List<User> UsersInTournament()
        {
            return Users.All.Where(x => x.Scores.Any(x2 => x2.GameId == Score.Tournament)).ToList();
        }

        /// <summary>
        /// Check if the tournament has already been started. True if games have been played.
        /// </summary>
        public static bool TournamentStarted
        {
            get
            {
                return GetRoundsPlayed() != 0;
            }
        }

        /// <summary>
        /// Get the current Dealer's userId.
        /// </summary>
        public static int GetDealerUserId()
        {
            return GetTournamentParamValue(TournamentParams.Dealer);
        }

        /// <summary>
        /// Get the number of rounds played in the current tournament.
        /// </summary>
        public static int GetRoundsPlayed()
        {
            var tournamentScores = Users.Scores(Score.Tournament);
            if (tournamentScores.Length > 0)
            {
                return tournamentScores.Select(x => x.GamesPlayed).Max();
            }
            else
            {
                return 0;
            }

            //return GetTournamentParamValue(TournamentParams.RoundsPlayed);
        }

        /// <summary>
        /// Gets a specific Tournamet param value.
        /// </summary>
        private static int GetTournamentParamValue(TournamentParams param)
        {
            return Params.ParamAttName(TournamentParamAttName).First(x => x.ParamText == param.ToString()).ParamValue;
        }

        public static void UpdateDealerUserId(int dealerId)
        {
            var param = Params.ParamAttName(TournamentParamAttName).First(x => x.ParamText == TournamentParams.Dealer.ToString());
            param.ParamValue = dealerId;
            param.Update();
        }

        public static void Reset()
        {
            UpdateDealerUserId(0);
        }
    }

    public class TournamentPlayer
    {

        public string Username { get; }

        public int UserId { get; }

        public bool Checked { get; set; }
        
        public bool Allin { get; set; }

        public Card[] Cards { get; set; }

        public Hand Hand { get; set; }

        public bool Active { get; set; }

        public int CurrentBet { get; set; }

        public int Funds
        {
            get
            {
                return StartingFunds - CurrentBet;
            }
        }

        public int StartingFunds { get; }

        public int CurrentGains { get; set; }

        public TournamentPlayer(int userId)
        {
            var user = Users.Get(userId);
            UserId = user.UserId;
            Username = user.Username;
            StartingFunds = user.CurrentSeasonScoreFor(Score.Tournament).TotalGains;
            Active = Funds > 0;
            CurrentGains = 0;
            CurrentBet = 0;
            Checked = false;
            Allin = false;
        }
    }
}
