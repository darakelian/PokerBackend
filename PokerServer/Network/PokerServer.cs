using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokerServer.Core;
using PokerServer.Network.ClientPackets;
using PokerServer.Network.ServerPackets;
using SimpleTCPStandar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PokerServer.Network
{
    class PokerServer
    {
        public Dictionary<int, IWebSocketConnection> UserIdToSockets;
        private readonly object _userIdMapLock = new object();

        private readonly Table _table;

        private const int _maxPlayers = 2;

        public PokerServer()
        {
            UserIdToSockets = new Dictionary<int, IWebSocketConnection>();
            _table = new Table(this);

            var server = new WebSocketServer("ws://0.0.0.0:9000");
            server.Start(socket =>
            {
                socket.OnOpen = () => Console.WriteLine("Open!");
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = (message) => OnMessage(socket, message);
            });
        }

        /// <summary>
        /// Handles receiving bytes of data over the WebSocket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        private void OnMessage(IWebSocketConnection socket, string message)
        {
            Console.WriteLine($"Socket {socket.ConnectionInfo} received {message}.");
            var packet = JToken.Parse(message);
            var packetId = packet.Value<int>("PacketId");
            var uid = packet.Value<int>("UserId");
            // Compare packet IDs to the client packets
            switch (packetId)
            {
                // Login request from client
                case 0:
                    if (_table.GameInProgress)
                        return;
                    
                    if (uid == -1)
                    {
                        Console.WriteLine("Creating new user.");
                        // unknown user
                        uid = Guid.NewGuid().GetHashCode();
                        Console.WriteLine($"User was created with UID {uid}.");
                    }

                    lock (_userIdMapLock)
                    {
                        UserIdToSockets[uid] = socket;
                    }

                    var loginStatus = default(byte);

                    if (_table.Players.Count >= _maxPlayers)
                        loginStatus = 1;
                    else
                    {
                        // Player is able to be seated at the table.
                        loginStatus = 0;
                        _table.AddPlayer(new Model.Player(uid));
                    }

                    var responsePacket = new LoginResponse
                    {
                        PacketId = 0,
                        LoginStatus = loginStatus,
                        UserId = uid
                    };

                    socket.Send(JsonConvert.SerializeObject(responsePacket));

                    if (_table.Players.Count == _maxPlayers)
                        _table.StartGame();
                    break;
                // Bet/call request
                case 1:
                    if (uid != _table.ActivePlayer.Id || !_table.GameInProgress)
                        return;

                    Console.WriteLine($"Player {uid} submitted bid for {packet.Value<int>("BetAmount")}");
                    if (_table.PlaceBet(uid, packet.Value<int>("BetAmount")))
                    {
                        var updateChipPacket = new UpdatePlayerChipCount
                        {
                            PacketId = 5,
                            UserId = uid,
                            NewChipCount = _table.ActivePlayer.Chips,
                            NewPotCount = _table.Pot
                        };
                        
                        BroadcastMessage(JsonConvert.SerializeObject(updateChipPacket));
                        _table.CycleActivePlayer();
                    }
                    break;
                // General action request
                case 2:
                    if (uid != _table.ActivePlayer.Id || !_table.GameInProgress)
                        return;

                    var action = packet.Value<GeneralAction>("Action");
                    var handled = false;
                    if (action == GeneralAction.Check)
                    {
                        handled = _table.HandleCheck(uid);
                    }
                    else
                    {
                        handled = _table.MarkPlayerFolded(uid);
                    }

                    if (handled)
                        _table.CycleActivePlayer();
                    break;
            }
        }

        /// <summary>
        /// Broadcasts a message to all clients regardless of who generated the
        /// original message.
        /// </summary>
        /// <param name="message">The message being sent</param>
        public void BroadcastMessage(string message)
        {
            foreach (var socket in UserIdToSockets.Values)
                socket.Send(message);
        }

        public void SendGameMessage(int playerId, string message)
        {
            var gameMessagePacket = new GameMessagePacket
            {
                PacketId = 255,
                Message = message
            };
            Console.WriteLine($"Sending game message packet(msg={message})");
            UserIdToSockets[playerId].Send(JsonConvert.SerializeObject(gameMessagePacket));
        }
    }
}
