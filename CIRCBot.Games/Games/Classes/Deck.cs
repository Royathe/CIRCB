using System.Collections.Generic;
using System.Linq;

namespace CIRCBot.Games
{
    class Deck
    {
        /// <summary>
        /// Cards in the deck
        /// </summary>
        public List<Card> Cards { get; set; }

        /// <summary>
        /// Constructor. 
        /// 
        /// Create decks and shuffle them together.
        /// </summary>
        /// <param name="numberOfDecks">Number of decks to shuffle together</param>
        public Deck(int numberOfDecks = 1)
        {
            Cards = new List<Card>();

            for(int i = 0; i < numberOfDecks; i++)
            {
                foreach(Suit suit in Card.Suits)
                {
                    for(int value = 1; value < 14; value++)
                    {
                        Cards.Add(new Card(suit, value));
                    }
                }
            }

            // Shuffle decks
            Cards.Shuffle();
        }

        /// <summary>
        /// Create a deck with a specific set of cards.
        /// </summary>
        /// <param name="cards"></param>
        public Deck(params Card[] cards)
        {
            Cards = cards.ToList();
        }

        /// <summary>
        /// Draw the top card from the deck.
        /// </summary>
        public Card Draw()
        {
            Card card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }
    }
}
