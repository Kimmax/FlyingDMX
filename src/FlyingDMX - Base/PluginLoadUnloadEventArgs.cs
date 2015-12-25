using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public class PluginLoadUnloadEventArgs : EventArgs
    {
        public Driver Driver { get; private set; }

        public PluginLoadUnloadEventArgs(Driver driver)
        {
            this.Driver = driver;
        }
    }
}
