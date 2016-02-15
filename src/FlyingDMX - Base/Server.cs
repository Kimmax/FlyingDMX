using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nuernberger.FlyingDMX
{
    public class Server
    {
        // UDPClient.Receive needs a reference as parameter
        private IPEndPoint _endPoint;
        public IPEndPoint Endpoint 
        { 
            get { return _endPoint; }
            private set { _endPoint = value; } 
        }

        private UdpClient listener;
        private Task serverTask;
        private ManualResetEvent threadBlocker = new ManualResetEvent(false);
        private CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken cancelToken;

        // How long should the server try to receive a packet?
        private const int SERVER_READ_TIMEOUT_MS = 25;
        // How often should the server look for new packets?
        private const int SERVER_LOOP_LIMIT_MS = 5;
        // UDP has different error codes, which need to be checked in the error handler. This one is for timeout's
        private const int UDP_TIMEOUT_ERRORCODE = 10060;
        // On what port should the server listen, if no port is passed to the constructor?
        private const short SERVER_DEFAULT_PORT = 3636;

        public event EventHandler<IncomingCommandEventArgs> CommandIncoming;
        public event EventHandler<ServerStartStopEventArgs> ServerStart;
        public event EventHandler<ServerStartStopEventArgs> ServerStop;

        /// <summary>
        /// Initalizies a new instance of the FlyingDMX.Server-Class and bind's it to the given port
        /// </summary>
        /// <param name="port"> The port the server should be bound to</param>
        public Server(IPAddress ip, short port = SERVER_DEFAULT_PORT)
        {
            this.Endpoint = new IPEndPoint(ip, port);
            this.listener = new UdpClient(port);
            this.listener.EnableBroadcast = true;

            this.listener.Client.ReceiveTimeout = SERVER_READ_TIMEOUT_MS;

            cancelToken = cts.Token;
        }

        /// <summary>
        /// Starts listening on the given port for UDP packet's
        /// </summary>
        /// <param name="blockThread"> Blocks the method until the listening loop exists</param>
        public void Start(bool blockThread = false)
        {
            serverTask = Task.Factory.StartNew(async () =>
            {
                if (this.ServerStart != null)
                    this.ServerStart(this, new ServerStartStopEventArgs(this.Endpoint));
            
                while (true)
                {
                    try
                    {
                        var data = await this.listener.ReceiveAsync();
                        var receivedString = Encoding.ASCII.GetString(data.Buffer);

                        if (this.CommandIncoming != null)
                            this.CommandIncoming(this, new IncomingCommandEventArgs(Command.TryParse(receivedString)));
                    }
                    catch(SocketException ex)
                    {
                        if (ex.ErrorCode != UDP_TIMEOUT_ERRORCODE)
                        {
                            // Handle the error. 10060 is a timeout error, which is expected.
                        }
                    }

                    Thread.Sleep(SERVER_LOOP_LIMIT_MS);
                }
            }, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            if (blockThread)
                threadBlocker.WaitOne();
        }

        /// <summary>
        /// Stops the server and unblocks the thread if needed.
        /// </summary>
        public void Stop()
        {
            this.listener.Close();

            cts.Cancel();
            this.serverTask = null;

            // Release the thread block on the Start method
            this.threadBlocker.Set();

            if (this.ServerStop != null)
                this.ServerStop(this, new ServerStartStopEventArgs(this.Endpoint));
        }
    }
}
