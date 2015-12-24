using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nuernberger.FlyingDMX;

namespace Nuernberger.FlyingDMX.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "FlyingDMX";
            Base myBase = new Base();
            myBase.Run();

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }

    public class Base
    {
        Server flyingServer;
        DMXController controller;

        public void Run()
        {
            flyingServer = new Server(3636);
            flyingServer.OnServerStart += OnServerStart;
            flyingServer.OnServerStop += OnServerStop;
            flyingServer.OnCommandIncoming += OnCommandIncoming;
            
            controller = new DMXController(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().FullName), "devices"));
            controller.OnDeviceLoaded += OnDeviceLoaded;

            Console.WriteLine("\nLoading devices from folder '" + controller.DeviceDefinitionsLocation + "' ..");
            controller.LoadDevices();

            flyingServer.Start(true);
        }

        void OnDeviceLoaded(object sender, DMXDeviceLoadedEventArgs e)
        {
            Console.WriteLine(String.Format("\nLoaded Device '{0}':", (String.IsNullOrEmpty(e.Device.Name)) ? this.controller.DMXDevices.Count.ToString() : e.Device.Name));
            Console.WriteLine(
                String.Format("\tMaster: {0}\n\tR: {1}\n\tG: {2}\n\tB: {3}\n\tStrobe: {4}\n\tSound2Light: {5}\n\tLocation: {6}\n\t",
                    (e.Device.Master == -1) ? "Not set" : e.Device.Master.ToString(),
                    (e.Device.R == -1) ? "Not set" : e.Device.Master.ToString(),
                    (e.Device.G == -1) ? "Not set" : e.Device.Master.ToString(),
                    (e.Device.B == -1) ? "Not set" : e.Device.Master.ToString(),
                    (e.Device.Strobe == -1) ? "Not set" : e.Device.Master.ToString(),
                    (e.Device.Sound2Light == -1) ? "Not set" : e.Device.Master.ToString(),
                    e.Device.DeviceLocation.ToString()
                )
            );
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
