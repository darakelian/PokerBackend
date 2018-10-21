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
        private int _checks;
        private int _lastRaisedId;

        public int Pot;
        public bool GameInProgress;
        private int _activePlayerIndex;
        public Player ActivePlayer => Players[_activePlayerIndex];
        private int _playersLeft => Players.Where(p => !p.HasFolded).Count();

        private int _lastBet;
        private Dictionary<int, int> _playerBets;

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
            _playerBets = Players.ToDictionary(p => p.Id, p => 0);
            // Reset state of the game
            _cardIndex = 0;
            _deck.Reset();
            foreach (var player in Players)
            {
                player.HasFolded = false;
                player.EmptyHand();
            }
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
                    Pot = Pot,
                    NumPlayers = (byte) (Players.Count - 1),
                    PlayersChips = Players.Where(p => p.Id != player.Id).Select(p => p.Chips).ToArray(),
                    PlayerIds = Players.Where(p => p.Id != player.Id).Select(p => p.Id).ToArray()
                };

                socket.Send(JsonConvert.SerializeObject(initialGamePacket));
            }

            // Assign a player the active role.
            _activePlayerIndex = 0;
            var markActivePacket = new MarkActivePlayer
            {
                PacketId = 4,
                UserId = ActivePlayer.Id
            };
            _server.BroadcastMessage(JsonConvert.SerializeObject(markActivePacket));
        }

        /// <summary>
        /// Flops 3 cards to the table.
        /// </summary>
        public void Flop()
        {
            // Burn card
            _deck.DrawCard();
            var drawnCards = new List<Card>();
            for (var i = 0; i < 3; i++)
            {
                drawnCards.Add(_faceUpCards[_cardIndex++] = _deck.DrawCard());
            }
            // Tell players about the new flopped cards
            ResetBettingState();

            var flopPacket = new UpdateFlopCards
            {
                PacketId = 2,
                Ranks = drawnCards.Select(c => c.Rank).ToArray(),
                Suits = drawnCards.Select(c => (char)c.Suit).ToArray()
            };
            _server.BroadcastMessage(JsonConvert.SerializeObject(flopPacket));
        }

        /// <summary>
        /// Puts either the turn or river card onto the table.
        /// </summary>
        public void TurnOrRiver()
        {
            // Burn card
            _deck.DrawCard();
            var drawnCard = _faceUpCards[_cardIndex++] = _deck.DrawCard();
            // Tell players about the turn/river
            ResetBettingState();

            var turnOrRiverPacket = new UpdateTurnOrRiverCard
            {
                PacketId = 3,
                Rank = drawnCard.Rank,
                Suit = (char)drawnCard.Suit
            };
            _server.BroadcastMessage(JsonConvert.SerializeObject(turnOrRiverPacket));
        }

        /// <summary>
        /// Resets various counting/tracking variables related to the betting in
        /// the current round (i.e. pre-flop, post-flop, post-turn, post-river).
        /// </summary>
        private void ResetBettingState()
        {
            _checks = 0;
            foreach (var key in _playerBets.Keys.ToList())
            {
                _playerBets[key] = 0;
            }
            _lastBet = 0;
            _lastRaisedId = -1;
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

        public bool MarkPlayerFolded(int playerId)
        {
            var player = Players.Where(p => p.Id == playerId).First();
            if (player != null)
            {
                player.HasFolded = true;
                if (_playersLeft == 1)
                    EndGame();
                return true;
            }
            return false;
        }

        private void EndGame()
        {
            var winners = CalculateWinners();
            DistributePot(winners);
            var winnerPacket = new UpdateWinners
            {
                PacketId = 6,
                WinnerIds = winners.Select(p => p.Id).ToArray(),
                NewWinnerPots = winners.Select(p => p.Chips).ToArray()
            };
            _server.BroadcastMessage(JsonConvert.SerializeObject(winnerPacket));
            StartGame();
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

        public bool PlaceBet(int playerId, int betAmount)
        {
            var player = Players.Where(p => p.Id == playerId).First();
            if (player != null && player.Chips >= betAmount)
            {
                // See if the bet needs to be a certain amonut
                var minimumBet = _lastBet - _playerBets[playerId];
                if (minimumBet > betAmount)
                {
                    _server.SendGameMessage(playerId, $"Error: Bid amount must be large enough to call. Expected {minimumBet}, you bet {betAmount}");
                    return false;
                }
                player.Chips -= betAmount;
                if (betAmount > _lastBet)
                {
                    Console.WriteLine("Player has raised for this betting roung.");
                    _lastRaisedId = playerId;
                }
                // Reset the number of checks that have happened this betting round
                _checks = 0; 
                _lastBet = betAmount;
                Pot += betAmount;
                _playerBets[playerId] += betAmount;
                return true;
            }
            else
            {
                _server.SendGameMessage(playerId, "Error: Don't have enough chips for that bet.");
                return false;
            }
        }

        /// <summary>
        /// Handles a player choosing to check for their turn.
        /// </summary>
        /// <param name="checkingPlayerId"></param>
        public bool HandleCheck(int checkingPlayerId)
        {
            // See if the player can actually check. If there is a bet that
            // they haven't acted on they can't check.
            if (_playerBets[checkingPlayerId] < _lastBet)
            {
                _server.SendGameMessage(checkingPlayerId, "Error: Unable to check due to existing bid on the table.");
                return false;
            }

            _checks++;
            if (_checks == _playersLeft)
            {
                DetermineNextCardAction();
            }
            return true;
        }

        private void DetermineNextCardAction()
        {
            // Taking a flop
            if (_faceUpCards[0] == null)
                Flop();
            // Either turn or river here
            else if (_faceUpCards[3] == null || _faceUpCards[4] == null)
                TurnOrRiver();
            // Calculate the winners
            else
                EndGame();
        }

        public void CycleActivePlayer()
        {
            // Cycle to the next active player that hasn't folded.
            do
            {
                _activePlayerIndex = _activePlayerIndex + 1 < Players.Count ? ++_activePlayerIndex : 0;
            }
            while (ActivePlayer.HasFolded);

            foreach (var (id, socket) in _server.UserIdToSockets)
            {
                var markActivePacket = new MarkActivePlayer
                {
                    PacketId = 4,
                    UserId = ActivePlayer.Id,
                    HighestBid = _lastBet,
                    PersonalBid = _playerBets[id]
                };
                socket.Send(JsonConvert.SerializeObject(markActivePacket));
            }

            // This conditional checks if the action has gotten back to the person
            // who originally raised. That player isn't allow to raise again and
            // the game immediately proceedes to the next card.
            if (_lastRaisedId == ActivePlayer.Id)
            {
                DetermineNextCardAction();
            }
        }
    }
}
