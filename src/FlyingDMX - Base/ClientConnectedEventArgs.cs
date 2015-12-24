using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public FlyingClient Client { get; private set; }

        public ClientConnectedEventArgs(FlyingClient client)
        {
            this.Client = client;
        }
    }
}
