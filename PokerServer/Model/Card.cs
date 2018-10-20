namespace PokerServer.Model
{
    /// <summary>
    /// Represents a single card.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Card's suit.
        /// </summary>
        internal CardSuit Suit { get; private set; }
        /// <summary>
        /// Card's rank. a = Ace, j = Jack, q = Queen, k = King
        /// </summary>
        internal char Rank { get; private set; }

        /// <summary>
        /// Creates a new card for a specific suit and rank.
        /// </summary>
        /// <param name="suit">The suit of the card</param>
        /// <param name="rank">The rank of the card</param>
        public Card(CardSuit suit, char rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"{Rank}{(char)Suit}";
        }
    }
}
