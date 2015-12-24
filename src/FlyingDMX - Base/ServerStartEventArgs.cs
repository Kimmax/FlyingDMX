using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Nuernberger.FlyingDMX
{
    public class ServerStartEventArgs : EventArgs
    {
        public EndPoint Endpoint { get; private set; }

        public ServerStartEventArgs(EndPoint endpoint)
        {
            this.Endpoint = endpoint;
        }

        public override string ToString()
        {
            return this.Endpoint.ToString();
        }
    }
}
