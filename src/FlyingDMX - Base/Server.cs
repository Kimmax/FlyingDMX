using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Nuernberger.FlyingDMX
{
    public class Server
    {
        public short Port { get; set; }

        public bool Listening { get; private set; }

        private TcpListener Listener;

        private List<FlyingClient> clients = new List<FlyingClient>();

        public event EventHandler<IncomingCommandEventArgs> OnCommandIncoming;
        public event EventHandler<ClientConnectedEventArgs> OnClientConnect;
        public event EventHandler<ServerStartEventArgs> OnServerStart;

        public Server(short port = 3636)
        {
            this.Port = port;
            this.Listener = new TcpListener(IPAddress.Any, this.Port);
        }

        public void Start()
        {
            new Thread(() =>
            {
                this.Listener.Start();
                this.Listening = true;

                if (this.OnServerStart != null)
                    this.OnServerStart(this, new ServerStartEventArgs(this.Listener.LocalEndpoint));

                while (this.Listening)
                {
                    if (this.Listener.Pending())
                    {
                        FlyingClient newClient = new FlyingClient(this.Listener.AcceptTcpClient(), this.clients.Count +1, false);
                        newClient.OnCommandIncoming += FlyingClient_OnCommandIncoming;
                        newClient.Init();

                        this.clients.Add(newClient);

                        if (this.OnClientConnect != null)
                            this.OnClientConnect(this, new ClientConnectedEventArgs(newClient));
                    }

                    Thread.Sleep(100);
                }
            }).Start();
        }

        void FlyingClient_OnCommandIncoming(object sender, IncomingCommandEventArgs e)
        {
            if(e.Command.Type != Command.CommandType.Direct)
            {
                IPAddress hopDest = IPAddress.Parse(e.Command.Args[0]);
                short hopPort = Convert.ToInt16(e.Command.Args[1]);

                TcpClient client = new TcpClient();
                client.Connect(hopDest, hopPort);

                FlyingClient newClient = new FlyingClient(client, this.clients.Count +1, false);
                newClient.OnCommandIncoming += FlyingClient_OnCommandIncoming;
                newClient.Init();

                if(newClient.Active)
                {
                    foreach (FlyingClient fc in this.clients)
                        fc.Stop();

                    this.Listening = false;
                    this.Listener.Stop();
                }
            }
            else
            {
                if (this.OnCommandIncoming != null)
                    this.OnCommandIncoming(this, e);
            }
        }
    }
}
