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

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }

    public class Base
    {
        Server flyingServer;

        public void Run()
        {
            flyingServer = new Server(3636);
            flyingServer.OnServerStart += OnServerStart;
            flyingServer.OnServerStop += OnServerStop;
            flyingServer.OnCommandIncoming += OnCommandIncoming;
            
            flyingServer.Start(true);
        }

        void OnServerStart(object sender, ServerStartStopEventArgs e)
        {
            Console.WriteLine("Server started at: " + e.Endpoint.ToString());
        }

        void OnServerStop(object sender, ServerStartStopEventArgs e)
        {
            Console.WriteLine("Server stopped: " + e.Endpoint.ToString());
        }

        void OnCommandIncoming(object sender, IncomingCommandEventArgs e)
        {
            Console.WriteLine("\nGot command:\n" + String.Format("\tType: {0}\n\tArgs: {1}", e.Command.Type.ToString(), String.Join(" ", e.Command.Args)));

            if(e.Command.Type == Command.CommandType.Exit)
            {
                flyingServer.Stop();
            }
        }
    }
}
