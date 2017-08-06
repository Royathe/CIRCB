namespace CIRCBot.Games
{
    /// <summary>
    /// Game controller.
    /// </summary>
    interface IGame
    {
        /// <summary>
        /// Has game ended.
        /// </summary>
        bool GameOver { get; }

        bool Join(User user);

        bool Check(Msg message);

        bool Fold(Msg message);

        bool Cancel(Msg message);

        bool Allin(Msg message);

        bool Hit(Msg message);

        bool Bet(Msg message);
    }
}
