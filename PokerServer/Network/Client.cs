using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PokerServer.Network
{
    class Client
    {
        public string ID { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        Socket s;
        public Client(Socket accepted)
        {
            s = accepted;
            ID = Guid.NewGuid().ToString();
            EndPoint = (IPEndPoint)s.RemoteEndPoint;
            s.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
        }

        void callback(IAsyncResult ar)
        {
            try
            {
                s.EndReceive(ar);

                byte[] buf = new byte[8192];

                int rec = s.Receive(buf, buf.Length, 0);

                if (rec < buf.Length)
                {
                    Array.Resize<byte>(ref buf, rec);
                }

                if (Received != null)
                {
                    Received(this, buf);
                }

                s.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Close();
                if(Disconnected != null)
                {
                    Disconnected(this);
                }
            }
            
        }

        public void Close()
        {
            s.Close();
            s.Dispose();
        }

        public delegate void ClientReceivedHandler(Client sender, byte[] data);
        public delegate void ClientDisconnectedHandler(Client sender);

        public event ClientReceivedHandler Received;
        public event ClientDisconnectedHandler Disconnected;
    }
}
