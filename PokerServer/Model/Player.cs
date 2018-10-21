using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerServer.Model
{
    /// <summary>
    /// Represents a player at the table.
    /// </summary>
    class Player
    {
        /// <summary>
        /// Flag representing if the player is the dealer.
        /// </summary>
        bool HasDealersButton { get; set; }
        /// <summary>
        /// Flag representing if the player is SB.
        /// </summary>
        bool IsSmallBlind { get; set; }
        /// <summary>
        /// Flag representing if the player is BB.
        /// </summary>
        bool IsBigBlind { get; set; }
        /// <summary>
        /// Flag representing if the player has folded current hand.
        /// </summary>
        public bool HasFolded { get; set; }

        public int Chips;
        private Card[] _hand;
        public int Id;

        public string Hand => string.Join(" ", _hand.ToList());
        public Card CardInSpot(int index) => _hand[index];

        public Player(int id)
        {
            Id = id;
            _hand = new Card[2];
            Chips = 1000;
        }

        /// <summary>
        /// Attempts to deal a card to the player. Will throw an error if the
        /// player already has 2 cards.
        /// </summary>
        /// <param name="card">Card being dealt</param>
        public void GiveCard(Card card)
        {
            if (_hand[0] != null && _hand[1] != null)
                throw new InvalidOperationException("Error: Player already has two cards.");

            if (!_hand.Any(c => c != null))
                _hand[0] = card;
            else
                _hand[1] = card;
        }
    }
}
