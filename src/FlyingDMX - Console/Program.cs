using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuernberger.FlyingDMX;

namespace Nuernberger.FlyingDMX.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Base myBase = new Base();
            myBase.Run();
        }
    }

    public class Base
    {
        Server flyingServer;

        public void Run()
        {
            flyingServer = new Server(3636);
            flyingServer.OnServerStart += OnServerStart;
            flyingServer.OnClientConnect += OnClientConnect;
            flyingServer.OnCommandIncoming += OnCommandIncoming;
            flyingServer.Start();
        }

        void OnServerStart(object sender, ServerStartEventArgs e)
        {
            Console.WriteLine("Server started at: " + e.Endpoint.ToString());
        }

        void OnClientConnect(object sender, ClientConnectedEventArgs e)
        {
            Console.WriteLine("\nNew client connected! ID: " + e.Client.ID);
        }

        void OnCommandIncoming(object sender, IncomingCommandEventArgs e)
        {
            Console.WriteLine("\nGot command:\n" + String.Format("\tType: {0}\n\tArgs: {1}", e.Command.Type.ToString(), String.Join(" ", e.Command.Args)));
        }
    }
}
