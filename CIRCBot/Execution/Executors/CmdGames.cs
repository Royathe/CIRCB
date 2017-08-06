using System;
using CIRCBot.XTimers;
using CIRCBot.Games;

namespace CIRCBot.Execution.Executors
{
    /// <summary>
    /// Passes commands to game objects.
    /// </summary>
    [ClassName("Game", "Pelikomennot")]
    class CmdGames : BaseExecutor, IExecutor
    {

        #region Constants

        /// <summary>
        /// Timer lock key for locking the "check" command. Locked for 1 second when a player goes allin.
        /// </summary>
        private const string CheckLockKey = "GameCheckedLockKey";

        #endregion Constants

        #region Context

        private enum Games
        {
            None,
            Blackjack,
            Holdem,
            BlindHoldem,
            Tournament
        }

        private IGame CurrentGame { get; set; }

        #endregion Context

        #region Constructor

        public CmdGames(BotMessager messager) : base(messager)
        {
            Add("Kuittaa pelaajan valmiiksi. Ei enää odoteta komentoja tältä pelaajalta.", CmdCheck, "check", "c", "jää");
            Add("Poistaa pelaajan pelistä. Pelaajalta peritään sen hetkinen betti.", CmdFold, "fold", "föld", "fould", "fouldem", "f");
            Add("Peruu pelin aloituksen.", CmdCancel, "cancel", "peru", "lopeta");
            Add("Nostaa betin joko pelin maksimiin, tai pelaajan maksimiin jos pelaajalla rahaa alle pelin maksimin.", CmdAllin, "allin", "allesin", "allesineen", "kaikki", "syteentaisaveen");
            Add("Ottaa seuraavan kortin.", CmdHit, "hit", "lyö", "turpaan", "hittiä", "hitteä");
            Add("Nostaa pelin bettia annetulla määrällä.", CmdBet, "bet", "raise");
            Add("Aloittaa Blackjack pelin", CmdBlackjack, "blackjack", "niggagiggel", "mustajaakko");
            Add("Aloittaa Holdem pelin", CmdHoldem, "holdem");
            Add("Aloittaa Holdem turnauksen", CmdTournament, "tournament", "turnaus");
            Admin(CmdStopCurrentGame, "stopgame");
        }

        #endregion Constructor

        #region Execution

        public new void Execute(Msg message)
        {
            base.Execute(message);

            // If a game exists and it has ended, clear current game.
            if (CurrentGame != null)
            {
                if (CurrentGame.GameOver)
                {
                    CurrentGame = null;
                }
            }

            LogResults(RunCommand());
        }

        private void CreateAndJoinGame(Games context)
        {
            // Create a new game
            if(CurrentGame == null)
            {
                switch(context)
                {
                    case Games.Blackjack:
                        CurrentGame = new Blackjack(Bot);
                        break;
                    case Games.Holdem:
                        CurrentGame = new Holdem(Bot);
                        break;
                    case Games.BlindHoldem:
                        CurrentGame = new BlindHoldem(Bot);
                        break;
                    case Games.Tournament:
                        CurrentGame = new HoldemTournament(Bot);
                        break;
                }
            }

            // Join the currenet game
            if(CurrentGame != null)
            {
                CurrentGame.Join(Message.From);
            }
        }

        #endregion Execution

        #region Commands
        
        private void CmdStopCurrentGame()
        {
            CurrentGame = null;
        }

        private void CmdBlackjack()
        {
            CreateAndJoinGame(Games.Blackjack);
        }

        private void CmdHoldem()
        {
            if(Message.NextCommand() == "allin")
            {
                CreateAndJoinGame(Games.BlindHoldem);
            }
            else
            {
                CreateAndJoinGame(Games.Holdem);
            }
        }

        private void CmdTournament()
        {
            string nextCommand = Message.NextCommand();
            if (nextCommand == "join")
            {
                if (TournmentLoader.UserIsInTournament(Message.From))
                {
                    Bot.Say(Message.From.Username, "Olet jo holdem turnauksessa.");
                }
                else if(!TournmentLoader.TournamentStarted)
                {
                    TournmentLoader.JoinTournament(Message.From);
                    Bot.Say(Message.From.Username, "Liityit tämän kauden holdem turnaukseen.");
                }
                else
                {
                    Bot.Say(Message.From.Username, "Turnaus on jo aloitettu. Et voi enää littyä.");
                }
            }
            else if (nextCommand == "start")
            {
                if (Message.IsAdmin)
                {
                    CreateAndJoinGame(Games.Tournament);
                }
            }
            else
            {
                if (TournmentLoader.UserIsInTournament(Message.From))
                {
                    if (TournmentLoader.TournamentStarted)
                    {
                        CreateAndJoinGame(Games.Tournament);
                    }
                    else
                    {
                        Bot.Say(Message.From.Username, "Turnausta ei ole vielä käynnistetty. Pyydä adminia aloittamaan turnaus.");
                    }
                }
                else
                {
                    Bot.Say(Message.From.Username, "Liity holdem turnaukseen komennolla: !Tournament Join");
                }
            }
        }

        private void CmdCheck()
        {
            if (TimerLocks.IsLocked(CheckLockKey)) return;
            if(ValidateGameCommand())
            {
                CurrentGame.Check(Message);
            }
        }

        private void CmdFold()
        {
            if (ValidateGameCommand())
            {
                CurrentGame.Fold(Message);
            }
        }

        private void CmdCancel()
        {
            if (ValidateGameCommand())
            {
                if(CurrentGame.GetType() == typeof(HoldemTournament))
                {
                    if (Message.IsAdmin)
                    {
                        CurrentGame = null;
                    }
                }
                else
                {
                    CurrentGame.Cancel(Message);
                }
            }
        }

        private void CmdAllin()
        {
            if (ValidateGameCommand())
            {
                if (CurrentGame.Allin(Message))
                {
                    TimerLocks.Set(CheckLockKey, 1);
                }
            }
        }

        private void CmdHit()
        {
            if (ValidateGameCommand())
            {
                CurrentGame.Hit(Message);
            }
        }

        private void CmdBet()
        {
            if (ValidateGameCommand())
            {
                CurrentGame.Bet(Message);
            }
        }
        
        private bool ValidateGameCommand()
        {
            if(CurrentGame != null)
            {
                return true;
            }
            return false;
        }

        #endregion Commands

    }
}
