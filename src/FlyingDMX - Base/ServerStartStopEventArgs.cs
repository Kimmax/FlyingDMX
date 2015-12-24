using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Nuernberger.FlyingDMX
{
    public class ServerStartStopEventArgs : EventArgs
    {
        public EndPoint Endpoint { get; private set; }

        public ServerStartStopEventArgs(EndPoint endpoint)
        {
            this.Endpoint = endpoint;
        }

        public override string ToString()
        {
            return this.Endpoint.ToString();
        }
    }
}
