using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Nuernberger.FlyingDMX
{
    public class FlyingClient
    {
        public TcpClient Client { get; private set; }

        public bool Active { get; private set; }

        public int ID { get; private set; }

        private StreamWriter sWriter;
        private StreamReader sReader;

        internal event EventHandler<IncomingCommandEventArgs> OnCommandIncoming;

        public FlyingClient(TcpClient client, int id, bool autoStart = true)
        {
            this.Client = client;
            this.ID = id;

            if (autoStart)
                Init();
        }

        public void Init()
        {
            if(this.Client.Connected && !this.Active)
            {
                this.sReader = new StreamReader(this.Client.GetStream());
                this.sWriter = new StreamWriter(this.Client.GetStream());

                ReadLoop();
            }
        }

        private void ReadLoop()
        {
            if(!this.Active)
            {
                this.Active = true;

                new Thread(() =>
                {
                    while(this.Active && this.Client.Connected)
                    {
                        if (this.OnCommandIncoming != null)
                            this.OnCommandIncoming(this, new IncomingCommandEventArgs(this.sReader.ReadLine()));
                    }
                }).Start();
            }
        }

        public void WriteLine(string text)
        {
            this.sWriter.WriteLine(text);
            this.sWriter.Flush();
        }

        public void Stop()
        {
            try
            {
                this.Active = false;
                this.sReader.Close();
                this.sWriter.Close();
                this.Client.Close();
            }
            catch
            {

            }
        }
    }
}
