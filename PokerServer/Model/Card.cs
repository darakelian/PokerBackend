namespace PokerServer.Model
{
    /// <summary>
    /// Represents a single card.
    /// </summary>
    class Card
    {
        /// <summary>
        /// Card's suit.
        /// </summary>
        internal Suits Suit { get; private set; }
        /// <summary>
        /// Card's rank. 1 = Ace, 11 = Jack, 12 = Queen, 13 = King
        /// </summary>
        internal int Rank { get; private set; }

        /// <summary>
        /// Creates a new card for a specific suit and rank.
        /// </summary>
        /// <param name="suit">The suit of the card</param>
        /// <param name="rank">The rank of the card</param>
        internal Card(Suits suit, int rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}
