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
        private IPEndPoint _endPoint;
        public IPEndPoint Endpoint 
        { 
            get { return _endPoint; }
            private set { _endPoint = value; } 
        }

        public bool Listening { get; private set; }

        private UdpClient listener;
        private Thread serverThread;
        private ManualResetEvent threadBlocker = new ManualResetEvent(false);

        public event EventHandler<IncomingCommandEventArgs> OnCommandIncoming;
        public event EventHandler<ServerStartStopEventArgs> OnServerStart;
        public event EventHandler<ServerStartStopEventArgs> OnServerStop;

        public Server(short port = 3636)
        {
            this.Endpoint = new IPEndPoint(IPHelper.GetBroadcastIP(), port);
            this.listener = new UdpClient();
            this.listener.EnableBroadcast = true;
            this.listener.Connect(this.Endpoint);

            this.listener.Client.SendTimeout = 500;
            this.listener.Client.ReceiveTimeout = 25;
        }

        public void Start(bool blockThread = false)
        {
            this.serverThread = new Thread(() =>
            {
                this.Listening = true;

                if (this.OnServerStart != null)
                    this.OnServerStart(this, new ServerStartStopEventArgs(this.Endpoint));

                while (this.Listening)
                {
                    try
                    {
                        Byte[] data = this.listener.Receive(ref this._endPoint);
                        string message = Encoding.ASCII.GetString(data);

                        if (this.OnCommandIncoming != null)
                            this.OnCommandIncoming(this, new IncomingCommandEventArgs(Command.TryParse(message)));
                    }
                    catch(SocketException ex)
                    {
                        if (ex.ErrorCode != 10060)
                        {
                            // Handle the error. 10060 is a timeout error, which is expected.
                        }
                    }
                }
            });
            
            this.serverThread.Start();

            if (blockThread)
                threadBlocker.WaitOne();
        }

        public void Stop()
        {
            this.Listening = false;
            this.listener.Close();

            this.serverThread.Join(510);
            this.serverThread = null;

            this.threadBlocker.Set();

            if (this.OnServerStop != null)
                this.OnServerStop(this, new ServerStartStopEventArgs(this.Endpoint));
        }
    }
}
