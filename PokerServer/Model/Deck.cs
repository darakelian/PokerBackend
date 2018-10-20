using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerServer.Model
{
    /// <summary>
    /// Represents a deck of cards.
    /// </summary>
    class Deck
    {
        /// <summary>
        /// The cards in the deck, sorted by the Fisher-Yates algorithm.
        /// </summary>
        private List<Card> _cards;

        /// <summary>
        /// Index representing the current "top" of the deck.
        /// </summary>
        private int _index;

        /// <summary>
        /// The random number generator used for the deck.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Constructs a new deck of cards to be used in a game.
        /// </summary>
        /// <param name="seed">An optional seed for the random number generator</param>
        public Deck(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _cards = new List<Card>();

            // Generates the cards, should result in 52
            foreach (var suit in Enum.GetValues(typeof(Suits)).Cast<Suits>())
                for (var i = 1; i <= 13; i++)
                    _cards.Add(new Card(suit, i));

            Shuffle();
        }

        /// <summary>
        /// Shuffles the contents of the deck.
        /// </summary>
        private void Shuffle()
        {
            var n = _cards.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                var value = _cards[k];
                _cards[k] = _cards[n];
                _cards[n] = value;
            }
        }

        /// <summary>
        /// Draws the top card of the deck.
        /// </summary>
        /// <returns>The card that was drawn</returns>
        public Card DrawCard()
        {
            if (_index >= _cards.Count)
                throw new InvalidOperationException("Error: no more cards left to draw.");

            return _cards[_index++];
        }
    }
}
