using CIRCBot.XTimers;

namespace CIRCBot.Games
{
    /// <summary>
    /// Base class of Bot Games.
    /// </summary>
    class GameBase
    {
        protected BotMessager Bot { get; }

        protected SoloTimer Timer { get; set; }

        public GameBase(BotMessager messager)
        {
            Bot = messager;
            Timer = new SoloTimer();
        }
    }
}
