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
            endpoint = new IPEndPoint(IPAddress.Parse("192.168.178.255"), port);
            sender = new UdpClient();
            sender.EnableBroadcast = true;
        }

        public void SendCommand(Command cmd)
        {
            SendLine(cmd.ToString());
        }

        public void SendLine(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            sender.Send(data, data.Length, endpoint);
        }

        public void Close()
        {
            sender.Close();
        }
    }
}
