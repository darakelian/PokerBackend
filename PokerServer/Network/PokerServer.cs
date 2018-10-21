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
            // Compare packet IDs to the client packets
            switch (packetId)
            {
                // Login request from client
                case 0:
                    if (_table.GameInProgress)
                        return;

                    var uid = packet.Value<int>("UserId");
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
                    break;
            }
        }
    }
}
