using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Nuernberger.FlyingDMX
{
    public class SimpleSender
    {
        private UdpClient sender;
        private IPEndPoint endpoint;

        public SimpleSender(int port)
        {
            sender = new UdpClient();
            endpoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), port);
        }

        public void SendCommand(Command cmd)
        {
            SendLine(cmd.ToString());
        }

        public void SendLine(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes("Hello UDP Server!!!");
            sender.Send(data, data.Length, endpoint);
            sender.Close();
        }
    }
}
