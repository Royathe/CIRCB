namespace CIRCBot.Games
{
    /// <summary>
    /// Player in an ongoing game.
    /// </summary>
    class Player
    {

        public int PlayerUserId { get; }

        public string PlayerName { get; }

        public Player(User user)
        {
            PlayerName = user.Username;
            PlayerUserId = user.UserId;
        }
    }
}
