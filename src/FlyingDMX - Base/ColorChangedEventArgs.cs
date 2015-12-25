using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Nuernberger.FlyingDMX
{
    public class ColorChangedEventArgs : EventArgs
    {
        public Color NewColor { get; private set; }
        public DMXDevice Device { get; private set; }

        public ColorChangedEventArgs(Color newColor, DMXDevice dev)
        {
            this.NewColor = newColor;
            this.Device = dev;
        }
    }
}
