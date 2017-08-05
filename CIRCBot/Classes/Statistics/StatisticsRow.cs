using System;
using System.Linq;

namespace CIRCBot
{
    public class StatisticsRow
    {

        private const string section = " | ";

        private User User { get; set; }

        [Order]
        public int Played { get; set; }

        [Order]
        public int Won { get; set; }

        [Order]
        public decimal WinPercentage { get; set; }

        [Order]
        public int Forfeitted { get; set; }

        [Order]
        public decimal ForfeitPercentage { get; set; }

        [Order]
        public int Total { get; set; }


        public int? SeasonsWon { get; set; }


        public int? SeasonsLost { get; set; }

        [Extra]
        public bool? Seasons { get; set; }

        public StatisticsRow(Score score)
        {
            User = Users.Get(score.UserId);
            Played = score.GamesPlayed;
            Won = score.GamesWon;
            WinPercentage = percentage(Won, Played);
            Forfeitted = score.GamesForfeitted;
            ForfeitPercentage = percentage(Forfeitted, Played);
            Total = score.TotalGains;
        }

        public static string[] GetOrderProperties()
        {
            return GetPropertiesWithAttribute<OrderAttribute>();
        }

        public static string[] GetExtraProperties()
        {
            return GetPropertiesWithAttribute<ExtraAttribute>();
        }

        private static string[] GetPropertiesWithAttribute<T>()
        {
            return typeof(StatisticsRow).GetProperties().Where(x => Attribute.IsDefined(x, typeof(T))).Select(x => x.Name).ToArray();
        }

        public new string ToString()
        {
            PositionString message = "";
            message.Position(13, "| " + User.Username.ToUpper());
            message.Section(15, "Pelejä: " + Played);
            message.Section(17, "Voittoja: " + Won);
            message.Section(11, WinPercentage.ToFixed() + "%");
            message.Section(18, "Unohtunut: " + Forfeitted);
            message.Section(11, ForfeitPercentage.ToFixed() + "%");
            if(SeasonsWon.HasValue)     message.Section(22, "Voitetut kaudet: " + SeasonsWon);
            if(SeasonsLost.HasValue)    message.Section(21, "Hävityt kaudet: " + SeasonsLost);
            message.Section(25, "Voitot: " + addSplitsToInt(Total, 3));
            message.Section(5, "");
            return message;
        }

        private string addSplitsToInt(int value, int split = 3, int firstSplit = 3)
        {
            bool isNegative = false;
            string text = value.ToString();

            if(text[0] == '-')
            {
                isNegative = true;
                text = text.Substring(1);
            }

            for(int i = text.Length - firstSplit; i > 0; i -= split)
            {
                text = text.Position(i, " ");
            }

            if (isNegative)
            {
                text = "-" + text;
            }
            else
            {
                text = " " + text;
            }

            return text;
        }

        private decimal percentage(decimal value1, decimal value2)
        {
            if(value2 == 0.0m)
            {
                return 0.0m;
            }
            return Math.Round(((value1 / value2) * 100), 2);
        }


        public static implicit operator String(StatisticsRow SR)
        {
            return SR.ToString();
        }
    }
}
