using System;
using System.Collections.Generic;
using System.Linq;

namespace CIRCBot.Games.Tools
{
    public class Hand : IComparable<Hand>, IEquatable<Hand>
    {

        #region Subclasses

        private class Score
        {
            public const int High = 0;
            public const int Pair = 25;
            public const int TwoPairs = 50;
            public const int Triples = 100;
            public const int Straight = 150;
            public const int Flush = 250;
            public const int FullHouse = 500;
            public const int Quad = 750;
            public const int RoyalFlush = 1000;
        }

        private class CardValueCount
        {
            public int Value { get; }

            public int Count { get; set; }

            public int[] Suits { get; }

            public CardValueCount(int value)
            {
                Value = value;
                Count = 0;
                Suits = new int[Card.Suits.Length];
            }

            public static CardValueCount[] NewArray()
            {
                List<CardValueCount> counts = new List<CardValueCount>();

                foreach(Value value in Card.Values)
                {
                    counts.Add(new CardValueCount(value.Id));
                }

                return counts.ToArray();
            }
        }

        private class CardSuitCount
        {
            public int Suit { get; }

            public int Count { get; set; }

            public CardSuitCount(int suit)
            {
                Suit = suit;
                Count = 0;
            }

            public static CardSuitCount[] NewArray()
            {
                List<CardSuitCount> counts = new List<CardSuitCount>();

                foreach (Suit suit in Card.Suits)
                {
                    counts.Add(new CardSuitCount(suit.Id));
                }

                return counts.ToArray();
            }
        }

        private class MultiCardHandCounts
        {
            public MultiCardHandCount Pair { get; }

            public List<int> Pairs { get; }

            public MultiCardHandCount Triples { get; }

            public MultiCardHandCount FullHouse { get; }

            public MultiCardHandCount Quad { get; }

            public MultiCardHandCounts()
            {
                Pair = new MultiCardHandCount();
                Pairs = new List<int>();
                Triples = new MultiCardHandCount();
                FullHouse = new MultiCardHandCount();
                Quad = new MultiCardHandCount();
            }

            public class MultiCardHandCount
            {
                public int Count { get; set; }

                public int Highest { get; set; }

                public MultiCardHandCount()
                {
                    Count = 0;
                    Highest = 0;
                }

                public void Increase(int value)
                {
                    Count++;
                    if (value > Highest)
                    {
                        Highest = value;
                    }
                }
            }
        }

        private class HighestStraight
        {
            public int HighestCardValue { get; set; }

            public bool IsFlush { get; set; }
        }

        #endregion Subclasses

        #region Public accessors

        /// <summary>
        /// Who the hand belongs to.
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// Description of the best hand.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The High card that had this hand win over an equivelant hand.
        /// </summary>
        public string TextAdd { get; set; }

        public int[] HandScores { get; set; }

        public int[] CardScores { get; set; }

        public Card[] Cards { get; }

        #endregion Public accessors

        #region Private accessors

        private List<int> handScores { get; set; }

        private List<int> cardScores { get; set; }

        private CardValueCount[] ValueCounts { get; set; }

        private CardSuitCount[] SuitCounts { get; set; }

        private MultiCardHandCounts HandCounts { get; set; }

        #endregion Private accessors

        #region Constructor

        public Hand(Card[] cards)
        {
            ValueCounts = CardValueCount.NewArray();
            SuitCounts = CardSuitCount.NewArray();
            HandCounts = new MultiCardHandCounts();

            Text = "";
            TextAdd = "";
            handScores = new List<int>();
            cardScores = new List<int>();

            Cards = cards;

            Process();
            Finish();
        }

        #endregion Constructor

        #region Construction methods

        private void Process()
        {
            //Process card and suit counts
            foreach (Card card in Cards)
            {
                // Aces are handled as "14", as an Ace is the highest value card
                if (card.Value == 1)
                {
                    // Log card as also a 1 for "straight" calculation
                    ValueCounts[card.Value].Count++;
                    ValueCounts[card.Value].Suits[card.Suit]++;
                    card.Value = 14;
                }

                // Increase card and suit counts
                ValueCounts[card.Value].Count++;
                ValueCounts[card.Value].Suits[card.Suit]++;
                SuitCounts[card.Suit].Count++;
            }

            // Process win type counts
            for (int i = ValueCounts.Length - 1; i > 1; i--)
            {
                int count = ValueCounts[i].Count;
                int value = ValueCounts[i].Value;

                switch (count)
                {
                    case 2:
                        HandCounts.Pair.Increase(value);
                        HandCounts.Pairs.Add(value);
                        break;
                    case 3:
                        HandCounts.Triples.Increase(value);
                        break;
                    case 4:
                        HandCounts.Quad.Increase(value);
                        break;
                }
            }

            var flush = GetFlush();

            var straight = GetBestStraight(flush);

            // Check for straight flush
            if (straight.IsFlush)
            {
                RoyalFlush(Card.Suits[flush], straight.HighestCardValue);
                return;
            }

            // Check for four-of-a-kind
            if (HandCounts.Quad.Count > 0)
            {
                FourOfAKind(HandCounts.Quad.Highest);
                return;
            }

            // Check for a full house
            if (HandCounts.Triples.Count > 0 && HandCounts.Pair.Count > 0)
            {
                FullHouse();
                return;
            }

            // Check for a flush
            if (flush != 0)
            {
                Flush(Card.Suits[flush]);
                return;
            }

            // Check for a straight
            if (straight.HighestCardValue != 0)
            {
                Straight(straight.HighestCardValue);
                return;
            }

            // Check for triple
            if (HandCounts.Triples.Count > 0)
            {
                Triples(HandCounts.Triples.Highest);
                return;
            }

            // Check for more than 1 pair
            if (HandCounts.Pairs.Count > 1)
            {
                TwoPairs();
                return;
            }

            // Check for pair
            if (HandCounts.Pair.Count > 0)
            {
                Pair();
                return;
            }

            // Order cards by value to scores
            High();
        }

        private void Finish()
        {
            CardScores = cardScores.ToArray();
            HandScores = handScores.ToArray();

            //if(CardScores.Length > 0)
            //{
            //    Text += " | ";
            //    foreach (int card in CardScores)
            //    {
            //        Text += Card.Values[card].Short + ", ";
            //    }
            //    Text = Text.Substring(0, Text.Length - 2);
            //}
        }

        #endregion Construction methods

        #region Private methods

        /// <summary>
        /// Get the player's flush. 
        /// </summary>
        /// <returns>Suit of the flush. 0 if not flush.</returns>
        private int GetFlush()
        {
            for(int i = 0; i < SuitCounts.Length; i++)
            {
                if(SuitCounts[i].Count >= 5)
                {
                    return SuitCounts[i].Suit;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the best straight. Returns a Straight object, with the highest's straights end value and whether the straight was a flush or not.
        /// </summary>
        /// <param name="flushSuit">Suit of the flush. 0 if not flush.</param>
        /// <returns>Highest regular straight, or the highest straight flush if present. Returns 0 if no straight.</returns>
        private HighestStraight GetBestStraight(int flushSuit)
        {
            var lastCard = ValueCounts[0]; // Last card that had a count above 0
            int consecutive = -1; // The number of cards in the current straight
            int consecutiveFlush = -1; // The number of cards in the current straight that are also of the same suit

            int highestStraight = 0; // The highest card of the current straight
            int highestStraightFlush = 0; // The highest card of the current straight that is also a flush
            
            CardValueCount card; // Card of current loop iteration

            for(int i = 1; i < ValueCounts.Length; i++)
            {
                card = ValueCounts[i];

                if(card.Count > 0)
                {
                    // If card follows the last card that had count above 0
                    if(i == (lastCard.Value + 1))
                    {
                        consecutive += 1;

                        // If both this card and the previous are part of the flush
                        if(card.Suits[flushSuit] > 0 && lastCard.Suits[flushSuit] > 0)
                        {
                            consecutiveFlush += 1;
                        }
                        else
                        {
                            consecutiveFlush = 0;
                        }
                    }
                    else
                    {
                        consecutive = 0;
                        consecutiveFlush = 0;
                    }

                    // If this card and previous 4 have been consecutive, log it as the highest straight
                    if (consecutive >= 4)
                    {
                        highestStraight = i;

                        // If this card and previous 4 have been consecutive and part of the flush, log it as the highest straight flush
                        if (consecutiveFlush >= 4)
                        {
                            highestStraightFlush = i;
                        }
                    }

                    lastCard = card;
                }
            }

            // Return highest straight flush if present, or highest straight if not.
            if(highestStraightFlush != 0)
            {
                return new HighestStraight() {
                    HighestCardValue = highestStraightFlush,
                    IsFlush = true
                };
            }
            else
            {
                return new HighestStraight()
                {
                    HighestCardValue = highestStraight,
                    IsFlush = false
                };
            }
        }

        /// <summary>
        /// Adds values of cards to score's array in order of value
        /// </summary>
        /// <param name="cards">Array of cards to process</param>
        /// <param name="numberOfCardsToAdd">The number of cards added to the scores</param>
        private void SortCardValuesToScores(Card[] cards, int numberOfCardsToAdd)
        {
            // Number of card values added to scores
            int cardsAdded = 0;

            for (int i = ValueCounts.Length - 1; i > 1; i--)
            {
                var cardCount = ValueCounts[i];

                for (int j = 0; j < cards.Length; j++)
                {
                    if (cards[j].Value == cardCount.Value)
                    {
                        if (cardsAdded < numberOfCardsToAdd)
                        {
                            cardScores.Add(cardCount.Value);
                            cardsAdded += 1;
                        }
                    }
                }
            }
        }

        #endregion Private methods

        #region Score calculations

        private void High()
        {
            handScores.Add(0);
            SortCardValuesToScores(Cards, 5);

            Console.WriteLine("HIGH: " + Card.Values[cardScores[0]].Short);

            Text = Card.Values[cardScores[0]].Short + " Hai";
        }

        private void Pair()
        {
            handScores.Add(Score.Pair + HandCounts.Pair.Highest);
            SortCardValuesToScores(
                Cards.Where(x => x.Value != HandCounts.Pair.Highest).ToArray(), 
                3);

            Console.WriteLine("PAIR: "
                + Card.Values[HandCounts.Pair.Highest].Short + ", "
                + Card.Values[HandCounts.Pair.Highest].Short
                );

            Text = Card.Values[HandCounts.Pair.Highest].Short + " Pari";
        }

        private void TwoPairs()
        {
            int highestPair = HandCounts.Pairs.Max();
            int lowerPair = HandCounts.Pairs.Where(x => x != highestPair).Max();
            int high = Cards.Where(x => x.Value != highestPair && x.Value != lowerPair).Max().Value;

            handScores.Add(Score.TwoPairs + highestPair);
            handScores.Add(lowerPair);
            cardScores.Add(high);

            Console.WriteLine("TWO PAIRS: "
                + Card.Values[highestPair].Short + ", "
                + Card.Values[highestPair].Short + ", "
                + Card.Values[lowerPair].Short + ", "
                + Card.Values[lowerPair].Short + ", "
                );

            Text = "Kaksi paria: "
                + Card.Values[highestPair].Short + ", "
                + Card.Values[highestPair].Short + ", "
                + Card.Values[lowerPair].Short + ", "
                + Card.Values[lowerPair].Short
                ;
        }

        private void Triples(int triple)
        {
            handScores.Add(Score.Triples + HandCounts.Triples.Highest);

            SortCardValuesToScores(
                Cards.Where(x => x.Value != HandCounts.Triples.Highest).ToArray(),
                2);

            Console.WriteLine("TRIPLE: " + Card.Values[triple].Short);

            Text = Card.Values[triple].Short + " Kolmoset";
        }

        private void Straight(int highest)
        {
            handScores.Add(Score.Straight + highest);

            Console.WriteLine("STRAIGHT: " + highest + " ||| "
                + Card.Values[highest].Short + ", "
                + Card.Values[highest - 1].Short + ", "
                + Card.Values[highest - 2].Short + ", "
                + Card.Values[highest - 3].Short + ", "
                + Card.Values[highest - 4].Short
                );

            Text = "Suora: "
                + Card.Values[highest].Short + ", "
                + Card.Values[highest - 1].Short + ", "
                + Card.Values[highest - 2].Short + ", "
                + Card.Values[highest - 3].Short + ", "
                + Card.Values[highest - 4].Short;
        }

        private void Flush(Suit suit)
        {
            handScores.Add(Score.Flush);

            // Sort cards that are a part of the flush to the scores.
            SortCardValuesToScores(
                Cards.Where(x => x.Suit == suit.Id).ToArray(), 
                5);

            Console.WriteLine("FLUSH: " + suit.Name);

            Text = suit.Color + suit.Name + Color.Clear + " Väri";
        }

        private void FullHouse()
        {
            int triple = HandCounts.Triples.Highest;
            int pair = HandCounts.Pair.Highest;

            handScores.Add(Score.FullHouse + triple);
            handScores.Add(pair);

            Console.WriteLine("FULL HOUSE: "
                + Card.Values[triple].Short + ", "
                + Card.Values[triple].Short + ", "
                + Card.Values[triple].Short + ", "
                + Card.Values[pair].Short + ", "
                + Card.Values[pair].Short
                );

            Text = "Täyskäsi: "
                + Card.Values[triple].Short + ", "
                + Card.Values[triple].Short + ", "
                + Card.Values[triple].Short + ", "
                + Card.Values[pair].Short + ", "
                + Card.Values[pair].Short;
        }

        private void FourOfAKind(int quadValue)
        {
            handScores.Add(Score.Quad + quadValue);

            // Add the highest card to the scores
            SortCardValuesToScores(
                Cards.Where(x => x.Value != quadValue).ToArray(),
                1);

            Console.WriteLine("FOUR OF A KIND: " + Card.Values[quadValue].Short);

            Text = Card.Values[quadValue].Short + " Neloset";
        }

        private void RoyalFlush(Suit flush, int highest)
        {
            handScores.Add(Score.RoyalFlush + highest);

            Console.WriteLine("STRAIGHT FLUSH (" + flush.Name + "): " + highest + " ||| "
                + Card.Values[highest].Short + ", "
                + Card.Values[highest-1].Short + ", "
                + Card.Values[highest-2].Short + ", "
                + Card.Values[highest-3].Short + ", "
                + Card.Values[highest-4].Short
                );

            Text = flush.Name + " Värisuora: "
                + Card.Values[highest].Short + ", "
                + Card.Values[highest - 1].Short + ", "
                + Card.Values[highest - 2].Short + ", "
                + Card.Values[highest - 3].Short + ", "
                + Card.Values[highest - 4].Short;
        }

        #endregion Score calculations

        #region General methods

        public static Hand[] ResolveWinningHands(Hand[] hands)
        {
            return hands.Where(x => x.Equals(hands.Max())).ToArray();
        }

        public int CompareTo(Hand other)
        {
            for (int i = 0; i < HandScores.Length; i++)
            {
                if (other.HandScores[i] > HandScores[i])
                {
                    return -1;
                }
                else if (other.HandScores[i] < HandScores[i])
                {
                    return 1;
                }
            }
            for (int i = 0; i < CardScores.Length; i++)
            {
                if (other.CardScores[i] > CardScores[i])
                {
                    if(other.handScores[0] != 0)
                    {
                        other.TextAdd = Card.Values[other.CardScores[i]].Short + " Hai";
                    }
                    return -1;
                }
                else if (other.CardScores[i] < CardScores[i])
                {
                    if(handScores[0] != 0)
                    {
                        TextAdd = Card.Values[CardScores[i]].Short + " Hai";
                    }
                    return 1;
                }
            }
            other.TextAdd = TextAdd;
            return 0;
        }

        public bool Equals(Hand other)
        {
            return CompareTo(other) == 0;
        }

        #endregion General methods

    }
}
