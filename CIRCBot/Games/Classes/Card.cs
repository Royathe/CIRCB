using System;

namespace CIRCBot.Games
{
    public class Card : IComparable<Card>
    {
        public const int HeartsId = 0;

        public const int DiamondsId = 1;

        public const int SpadesId = 2;

        public const int ClubsId = 3;

        public static readonly Suit[] Suits = new Suit[] {
            new Suit("♥", Color.Red, HeartsId),
            new Suit("♦", Color.LightBlue, DiamondsId),
            new Suit("♣", Color.Green, SpadesId),
            new Suit("♠", Color.Purple, ClubsId)
        };

        public static readonly Value[] Values = new Value[] {
            new Value(0),
            new Value(1, "Ässä", "A"),
            new Value(2),
            new Value(3),
            new Value(4),
            new Value(5),
            new Value(6),
            new Value(7),
            new Value(8),
            new Value(9),
            new Value(10),
            new Value(11, "Jätkä", "J"),
            new Value(12, "Rouva", "Q"),
            new Value(13, "Kuningas", "K"),
            new Value(14, "Ässä", "A")
        };

        public int Suit { get; set; }

        public int Value { get; set;}

        public Suit GetSuit => Suits[Suit];
        public Value GetValue => Values[Value];

        public string Name
        {
            get
            {
                string name = GetSuit.Color;
                name += GetSuit.Name;
                name += GetValue.Short;
                name += Color.Clear;
                return name;
            }
        }

        public Card(int suit, int value)
        {
            Suit = suit;
            Value = value;
        }

        public Card(Suit suit, int value)
        {
            Suit = suit.Id;
            Value = value;
        }

        public Card(Suit suit, Value value)
        {
            Suit = suit.Id;
            Value = value.Id;
        }

        public int CompareTo(Card other)
        {
            if(other.Value > Value)
            {
                return -1;
            }
            else if(other.Value < Value)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class Suit
    {

        public static Suit Hearts => Card.Suits[Card.HeartsId];
        public static Suit Diamonds => Card.Suits[Card.DiamondsId];
        public static Suit Spades => Card.Suits[Card.SpadesId];
        public static Suit Clubs => Card.Suits[Card.ClubsId];

        public int Id { get; }

        public string Name { get; }

        public string Color { get; }

        public Suit(string name, string color, int id)
        {
            Name = name;
            Id = id;
            Color = color;
        }
    }

    public class Value
    {
        public string Long { get; }
        public string Short { get; }

        public int Id { get; }

        public Value(int id, string longName, string shortName)
        {
            Id = id;
            Long = longName;
            Short = shortName;
        }

        public Value(int id)
        {
            Id = id;
            Long = id.ToString();
            Short = id.ToString();
        }
    }
}
