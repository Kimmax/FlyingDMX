using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    class DriverDummy : Driver  
    {
        public override string FullName
        {
            get
            {
                return "DriverDummy";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private int commandCounter = 0;

        public override string[] HardwareIDs
        {
            get
            {
                return new string[0];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DriverDummy(int framerate) : base(framerate) { }
        int lastY;

        public override void Start()
        {
            Console.WriteLine("Driver Dummy started.");
            commandCounter = 0;
        }

        public override void Stop()
        {
            Console.WriteLine("Driver Dummy stopped.");
        }

        public override void SetDMXValue(int channel, byte value)
        {
            if(Console.CursorTop == lastY)
            {
                Console.CursorLeft = 0;
                Console.Write(String.Format("No driver real driver loaded, but got command. [{0}]", commandCounter.ToString()));
            }
            else
            {
                Console.WriteLine(String.Format("No driver real driver loaded, but got command. [{0}]", commandCounter.ToString()));
            }

            lastY = Console.CursorTop;
            commandCounter++;
        }
    }
}
