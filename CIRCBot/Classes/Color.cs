using System;
using System.Linq;

namespace CIRCBot
{

    public static class Color
    {
        public static MircColor Clear { get { return new MircColor().Clear; } }
        public static MircColor White { get { return new MircColor().White; } }
        public static MircColor Black { get { return new MircColor().Black; } }
        public static MircColor Blue { get { return new MircColor().Blue; } }
        public static MircColor Green { get { return new MircColor().Green; } }
        public static MircColor Red { get { return new MircColor().Red; } }
        public static MircColor Brown { get { return new MircColor().Brown; } }
        public static MircColor Purple { get { return new MircColor().Purple; } }
        public static MircColor Orange { get { return new MircColor().Orange; } }
        public static MircColor Yellow { get { return new MircColor().Yellow; } }
        public static MircColor LightGreen { get { return new MircColor().LightGreen; } }
        public static MircColor Cyan { get { return new MircColor().Cyan; } }
        public static MircColor LightCyan { get { return new MircColor().LightCyan; } }
        public static MircColor LightBlue { get { return new MircColor().LightBlue; } }
        public static MircColor Pink { get { return new MircColor().Pink; } }
        public static MircColor Grey { get { return new MircColor().Grey; } }
        public static MircColor LightGrey { get { return new MircColor().LightGrey; } }
    }

    public class MircColor
    {

        private const string clear = "\u0003";

        private string color { get; set; }

        private ColorCodes textColor { get; set; }

        private ColorCodes bgColor { get; set; }

        public MircColor()
        {
            color = String.Empty;
        }

        public MircColor Colorize(string colorName)
        {
            string[] colorCodesNames = Enum.GetNames(typeof(ColorCodes));

            string realColorName = colorCodesNames.FirstOrDefault(x => x.ToLower() == colorName.ToLower());

            if (realColorName != null)
            {
                ColorCodes newColor = (ColorCodes)Enum.Parse(typeof(ColorCodes), realColorName);
                return colorize(newColor);
            }
            else
            {
                return this;
            }
        }

        private MircColor colorize(ColorCodes colorCode)
        {
            if (color == String.Empty)
            {
                textColor = colorCode;
                color = clear + (int)textColor;
            }
            else if (!color.Contains(','))
            {
                bgColor = colorCode;
                color = clear + (int)textColor + "," + (int)bgColor;
            }
            return this;
        }

        public MircColor Clear { get { color = clear; return this; } }
        public MircColor White { get { return colorize(ColorCodes.White); } }
        public MircColor Black { get { return colorize(ColorCodes.Black); } }
        public MircColor Blue { get { return colorize(ColorCodes.Blue); } }
        public MircColor Green { get { return colorize(ColorCodes.Green); } }
        public MircColor Red { get { return colorize(ColorCodes.LightRed); } }
        public MircColor Brown { get { return colorize(ColorCodes.Brown); } }
        public MircColor Purple { get { return colorize(ColorCodes.Purple); } }
        public MircColor Orange { get { return colorize(ColorCodes.Orange); } }
        public MircColor Yellow { get { return colorize(ColorCodes.Yellow); } }
        public MircColor LightGreen { get { return colorize(ColorCodes.LightGreen); } }
        public MircColor Cyan { get { return colorize(ColorCodes.Cyan); } }
        public MircColor LightCyan { get { return colorize(ColorCodes.LightCyan); } }
        public MircColor LightBlue { get { return colorize(ColorCodes.LightBlue); } }
        public MircColor Pink { get { return colorize(ColorCodes.Pink); } }
        public MircColor Grey { get { return colorize(ColorCodes.Grey); } }
        public MircColor LightGrey { get { return colorize(ColorCodes.LightGrey); } }

        public enum ColorCodes
        {
            White,
            Black,
            Blue,
            Green,
            LightRed,
            Brown,
            Purple,
            Orange,
            Yellow,
            LightGreen,
            Cyan,
            LightCyan,
            LightBlue,
            Pink,
            Grey,
            LightGrey
        }

        public static implicit operator String(MircColor c)
        {
            return c.color;
        }
    }
}
