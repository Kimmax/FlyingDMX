using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    abstract class Driver
    {
        public abstract int FrameRate { get; set; }

        public Driver(int framerate) 
        {
            this.FrameRate = framerate;
        }
        
        public abstract void Start();

        public abstract void Stop();

        public abstract void SetDMXValue(int channel, byte value);
    }
}
