using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokerServer.Network
{
    class TcpServer
    {
        private SimpleTCPStandar.SimpleTcpServer _server;

        public TcpServer()
        {
            _server = new SimpleTCPStandar.SimpleTcpServer();
            _server.Start(IPAddress.Parse("localhost"), 9999);
            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnClientDisconnected;
            _server.DataReceived += OnDataReceived;
        }

        private void OnClientConnected(object sender, TcpClient client)
        {

        }

        private void OnClientDisconnected(object sender, TcpClient client)
        {

        }

        private void OnDataReceived(object sender, SimpleTCPStandar.Message message)
        {

        }
    }
}
