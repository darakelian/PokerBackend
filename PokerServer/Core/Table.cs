using HoldemHand;
using Newtonsoft.Json;
using PokerServer.Model;
using PokerServer.Network.ServerPackets;
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
        private Network.PokerServer _server;

        private Card[] _faceUpCards;
        private int _cardIndex;

        public int Pot;
        public bool GameInProgress;

        public string Board => string.Join(" ", _faceUpCards.ToList());

        private readonly object _tableLock = new object();

        public Table(Network.PokerServer server)
        {
            Players = new List<Player>();
            _deck = new Deck();
            _faceUpCards = new Card[5];
            _server = server;
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
            GameInProgress = true;

            // Deal 2 cards to each player
            foreach (var player in Players)
                player.GiveCard(_deck.DrawCard());
            foreach (var player in Players)
                player.GiveCard(_deck.DrawCard());

            // Send network traffic
            foreach (var player in Players)
            {
                var socket = _server.UserIdToSockets[player.Id];
                var initialGamePacket = new InitialGameStatePacket
                {
                    PacketId = 1,
                    InitialChips = player.Chips,
                    Rank1 = player.CardInSpot(0).Rank,
                    Rank2 = player.CardInSpot(1).Rank,
                    Suit1 = (char) player.CardInSpot(0).Suit,
                    Suit2 = (char) player.CardInSpot(1).Suit,
                    NumPlayers = (byte) (Players.Count - 1),
                    PlayersChips = Players.Where(p => p.Id != player.Id).Select(p => p.Chips).ToArray(),
                    PlayerIds = Players.Where(p => p.Id != player.Id).Select(p => p.Id).ToArray()
                };

                socket.Send(JsonConvert.SerializeObject(initialGamePacket));
            }

            // Assign a player the active role.
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
            // Tell players about the new flopped cards
        }

        /// <summary>
        /// Puts either the turn or river card onto the table.
        /// </summary>
        public void TurnOrRiver()
        {
            // Burn card
            _deck.DrawCard();
            _faceUpCards[_cardIndex++] = _deck.DrawCard();
            // Tell players about the turn/river
        }

        /// <summary>
        /// Determines which player(s) have the best hand(s).
        /// </summary>
        /// <returns>The player(s) with the best hand(s)</returns>
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

        public void MarkPlayerFolded(int playerId)
        {
            var player = Players.Where(p => p.Id == playerId).First();
            if (player != null)
                player.HasFolded = true;
        }

        /// <summary>
        /// Distributes the pot to winner(s) of the hand.
        /// </summary>
        /// <param name="players">People receiving the pot this round</param>
        public void DistributePot(List<Player> players)
        {
            var shareOfPot = Pot / players.Count;
            foreach (var player in players)
                player.Chips += shareOfPot;
        }

        public void PlaceBet(int playerId, int betAmount)
        {
            var player = Players.Where(p => p.Id == playerId).First();
            if (player != null && player.Chips >= betAmount)
                player.Chips -= betAmount;
            else
                Console.WriteLine("Player doesn't have enough chips for bet");
        }
    }
}
