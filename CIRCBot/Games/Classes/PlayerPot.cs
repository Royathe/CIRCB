namespace CIRCBot.Games
{
    class PlayerPot
    {
        public int UserId { get; }

        public int Funds { get; set; }

        public PlayerPot(int userId, int funds)
        {
            UserId = userId;
            Funds = funds;
        }
    }
}
