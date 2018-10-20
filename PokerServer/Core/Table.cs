using HoldemHand;
using PokerServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerServer.Core
{
    /// <summary>
    /// Handles the interactions between players and the game state.
    /// </summary>
    class Table
    {
        public List<Player> Players { get; private set; }

        private Deck _deck;

        private Card[] _faceUpCards;
        private int _cardIndex;

        public int Pot;

        public string Board => string.Join(" ", _faceUpCards.ToList());

        public Table()
        {
            Players = new List<Player>();
            _deck = new Deck();
            _faceUpCards = new Card[5];
        }

        /// <summary>
        /// Starts a new hand of the game.
        /// </summary>
        public void StartGame()
        {
            // Reset state of the game
            _cardIndex = 0;
            _deck.Reset();
            foreach (var player in Players)
                player.HasFolded = false;

            // Deal 2 cards to each player
            foreach (var player in Players)
                player.GiveCard(_deck.DrawCard());
            foreach (var player in Players)
                player.GiveCard(_deck.DrawCard());
        }

        /// <summary>
        /// Flops 3 cards to the table.
        /// </summary>
        public void Flop()
        {
            // Burn card
            _deck.DrawCard();
            for (var i = 0; i < 3; i++)
                _faceUpCards[_cardIndex++] = _deck.DrawCard();
        }

        /// <summary>
        /// Puts either the turn or river card onto the table.
        /// </summary>
        public void TurnOrRiver()
        {
            // Burn card
            _deck.DrawCard();
            _faceUpCards[_cardIndex++] = _deck.DrawCard();
        }

        /// <summary>
        /// Determines which player(s) have the best hands.
        /// </summary>
        /// <returns></returns>
        public List<Player> CalculateWinners()
        {
            var playerScores = new Dictionary<Player, uint>();

            foreach (var player in Players)
            {
                var hand = new Hand(player.Hand, Board);
                playerScores[player] = hand.HandValue;
            }

            return playerScores.GroupBy(kvp => kvp.Value)
                .Aggregate((l, r) => l.Key > r.Key ? l : r)
                .Select(kvp => kvp.Key).ToList();
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }
    }
}
