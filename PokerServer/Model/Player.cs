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
        bool HasDealersButton;
        int ChipsRemaining;
        Card[] Hand;
        Guid Id;

        public Player()
        {
            Id = Guid.NewGuid();
            Hand = new Card[2];
        }

        /// <summary>
        /// Attempts to deal a card to the player. Will throw an error if the
        /// player already has 2 cards.
        /// </summary>
        /// <param name="card">Card being dealt</param>
        public void GiveCard(Card card)
        {
            if (Hand[0] != null && Hand[1] != null)
                throw new InvalidOperationException("Error: Player already has two cards.");

            if (!Hand.Any())
                Hand[0] = card;
            else
                Hand[1] = card;
        }
    }
}
