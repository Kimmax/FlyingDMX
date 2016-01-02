using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public abstract class Driver
    {
        public abstract string FullName { get; set; }
        public virtual string Description { get; set; }
        public abstract string[] HardwareIDs { get; set; }

        public virtual string Classname { get; set; }

        public virtual int FrameRate { get; set; }

        public Driver(int framerate) 
        {
            this.FrameRate = framerate;
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract void SetDMXValue(int channel, byte value);
    }
}
