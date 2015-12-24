using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public class IncomingCommandEventArgs : EventArgs
    {
        public Command Command { get; private set; }

        public IncomingCommandEventArgs(string command)
        {
            this.Command = Command.TryParse(command);
        }

        public IncomingCommandEventArgs(Command command)
        {
            this.Command = command;
        }
    }
}
