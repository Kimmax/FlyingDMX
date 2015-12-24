using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public class DMXDeviceLoadedEventArgs : EventArgs
    {
        public DMXDevice Device { get; private set; }

        public DMXDeviceLoadedEventArgs(DMXDevice device)
        {
            this.Device = device;
        }
    }
}
