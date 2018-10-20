using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PokerServer.Network
{
    class Server
    {
        Listener listener;
        public List<Client> Clients { get; private set; }
        public Server()
        {
            Clients = new List<Client>();
            listener = new Listener(8);
            listener.SocketAccepted += new Listener.SocketAcceptedHandler(listener_SocketAccepted);
            
        }

        void listener_SocketAccepted(Socket e)
        {
            Client client = new Client(e);
            client.Received += new Client.ClientReceivedHandler(cleint_Received);
            client.Disconnected += new Client.ClientDisconnectedHandler(client_Disconnected);
            
        }

        void client_Disconnected(Client sender)
        {
            Clients.Remove(sender);
        }

        void cleint_Received(Client sender, byte[] data)
        {
            Clients.Add(sender);
        }
    }
}
