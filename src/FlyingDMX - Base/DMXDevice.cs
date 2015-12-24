using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public class DMXDevice
    {
        public enum Location
        {
            Left,
            Right,
            Front,
            Back,
            All
        }

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _id;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _name;

        public int Master
        {
            get { return _master; }
            set { _master = value; }
        }
        private int _master = -1;

        public int R
        {
            get { return _r; }
            set { _r = value; }
        }
        private int _r = -1;

        public int G
        {
            get { return _g; }
            set { _g = value; }
        }
        private int _g = -1;

        public int B
        {
            get { return _b; }
            set { _b = value; }
        }
        private int _b = -1;

        public int Strobe
        {
            get { return _strobe; }
            set { _strobe = value; }
        }
        private int _strobe = -1;

        public int Sound2Light
        {
            get { return _s2l; }
            set { _s2l = value; }
        }
        private int _s2l = -1;

        public Location DeviceLocation
        {
            get { return _deviceLoc; }
            set { _deviceLoc = value; }
        }
        private Location _deviceLoc = Location.All;
    }
}
