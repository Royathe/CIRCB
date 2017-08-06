using CIRCBot.XTimers;

namespace CIRCBot
{
    public delegate void BotSay(string message, string to = "");
    public delegate void BotNotice(string message, string to = "");
    public delegate void BotAction(string message);

    /// <summary>
    /// Relays messages to the IRC object's StreamWriter.
    /// </summary>
    class BotMessager
    {
        public event BotSay eventSay;
        public event BotNotice eventNotice;
        public event BotAction eventAction;

        private SoloTimer Timer { get; }

        private bool CanMessage { get; set; }

        #region Messaging

        public void Say(string message)
        {
            FloodProtection();
            eventSay(message);
        }

        public void Say(string to, string message)
        {
            FloodProtection();
            eventSay(message, to);
        }

        public void Action(string message)
        {
            FloodProtection();
            eventAction(message);
        }

        public void Notice(string to, string message)
        {
            FloodProtection();
            eventNotice(message, to);
        }

        #endregion Messaging

        #region Constructor

        public BotMessager()
        {
            CanMessage = true;
            Timer = new SoloTimer();
        }

        #endregion Constructor

        #region Flood protection

        /// <summary>
        /// Delay consecutive commands
        /// </summary>
        private void FloodProtection()
        {
            if (!CanMessage)
            {
                System.Threading.Thread.Sleep(500);
                EnableMessaging();
            }
            else
            {
                CanMessage = false;
                //Timer.Set(0.5f, EnableMessaging);
            }
        }

        private void EnableMessaging()
        {
            CanMessage = true;
        }

        #endregion Flood protection

    }
}
