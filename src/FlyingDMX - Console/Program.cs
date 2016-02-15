using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace Nuernberger.FlyingDMX.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "FlyingDMX";
           
            bool drvManagerEnabled = true;
            string explizitDriver = null;

            try
            {
                drvManagerEnabled = Convert.ToBoolean(args.Where(item => item.StartsWith("drvmnger")).First().Split('=')[1]);
                explizitDriver = args.Where(item => item.StartsWith("driver")).First().Split('=')[1];
            }
            catch
            {
            }
            
            Base myBase = new Base();
            myBase.Run(drvManagerEnabled, explizitDriver);

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }

    public class Base
    {
        Server flyingServer;
        Server DIRECTflyingServer;
        DMXController controller;
        DriverManager driverManager;

        bool DEBUG = false;

        Driver loadedDriver
        {
            get
            {
                return _loadedDriver;
            }
            set
            {
                _loadedDriver = value;
                if (controller != null)
                    controller.DMXDriver = value;
            }
        }
        private Driver _loadedDriver;

        public void Run(bool driverManagement, string drivertoload = null)
        {
            DIRECTflyingServer = new Server(IPAddress.Parse("192.168.179.255"), 3636);
            DIRECTflyingServer.ServerStart += OnServerStart;
            DIRECTflyingServer.ServerStop += OnServerStop;
            DIRECTflyingServer.CommandIncoming += OnCommandIncoming;

            flyingServer = new Server(IPAddress.Parse("192.168.178.255"), 3636);
            flyingServer.ServerStart += OnServerStart;
            flyingServer.ServerStop += OnServerStop;
            flyingServer.CommandIncoming += OnCommandIncoming;

            controller = new DMXController(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().FullName), "devices"));
            controller.OnDeviceLoaded += OnDeviceLoaded;

            Console.WriteLine("\nLoading devices from folder '" + controller.DeviceDefinitionsLocation + "' ..");
            controller.LoadDevices();

            driverManager = new DriverManager(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            driverManager.OnDriverLoaded += OnDriverLoaded;
            driverManager.OnDriverUnloaded += OnDriverUnloaded;

            if(driverManagement)
            {
                Console.WriteLine("Starting with driver manager enabled.");
                
                driverManager.Load();
                driverManager.StartWatching();
            }
            else
            {
                Console.WriteLine("Starting with driver manager disabled.");

                if(!String.IsNullOrEmpty(drivertoload))
                {
                    Console.WriteLine("But with driver: " + drivertoload);
                    driverManager.Load(false, drivertoload);
                }
                else
                {
                    Console.WriteLine("And *NO* driver choosen!");
                }
            }

            flyingServer.Start(false);
            DIRECTflyingServer.Start(true);
        }

        void OnDriverLoaded(object sender, PluginLoadUnloadEventArgs e)
        {
            Console.WriteLine("Loaded driver {0}", e.Driver.FullName);
            loadedDriver = e.Driver;
        }

        void OnDriverUnloaded(object sender, PluginLoadUnloadEventArgs e)
        {
            Console.WriteLine("Unloaded driver {0}", e.Driver.FullName);
            loadedDriver = null;
        }

        void OnDeviceLoaded(object sender, DMXDeviceLoadedEventArgs e)
        {
            Console.WriteLine(String.Format("\nLoaded Device '{0}':", (String.IsNullOrEmpty(e.Device.Name)) ? this.controller.DMXDevices.Count.ToString() : e.Device.Name));
            Console.WriteLine(
                String.Format("\tMaster: {0}\n\tR: {1}\n\tG: {2}\n\tB: {3}\n\tStrobe: {4}\n\tSound2Light: {5}\n\tLocation: {6}\n\t",
                    (e.Device.Master == -1) ? "Not set" : e.Device.Master.ToString(),
                    (e.Device.R == -1) ? "Not set" : e.Device.R.ToString(),
                    (e.Device.G == -1) ? "Not set" : e.Device.G.ToString(),
                    (e.Device.B == -1) ? "Not set" : e.Device.B.ToString(),
                    (e.Device.Strobe == -1) ? "Not set" : e.Device.Strobe.ToString(),
                    (e.Device.Sound2Light == -1) ? "Not set" : e.Device.Sound2Light.ToString(),
                    e.Device.DeviceLocation.ToString()
                )
            );
        }

        void OnServerStart(object sender, ServerStartStopEventArgs e)
        {
            Console.WriteLine("[{0}] Server started at: " + e.Endpoint.ToString(), DateTime.Now.TimeOfDay);
        }

        void OnServerStop(object sender, ServerStartStopEventArgs e)
        {
            Console.WriteLine("[{0}] Server stopped: " + e.Endpoint.ToString(), DateTime.Now.TimeOfDay);
        }

        void OnCommandIncoming(object sender, IncomingCommandEventArgs e)
        {
            if(DEBUG)
                Console.WriteLine("\nGot command:\n" + String.Format("\tType: {0}\n\tArgs: {1}", e.Command.Type.ToString(), String.Join(" ", e.Command.Args)));

            DMXDevice.Location loc = (DMXDevice.Location)Enum.Parse(typeof(DMXDevice.Location), e.Command.Args[1]);

            switch (e.Command.Type)
            {
                case Command.CommandType.SetBrightness:
                    {
                        
                        controller.SetDeviceMaster(Convert.ToByte(e.Command.Args[0]), loc);

                        break;
                    }
                case Command.CommandType.SetColor:
                    {
                        Color color = System.Drawing.ColorTranslator.FromHtml(e.Command.Args[0]);
                        controller.SetDeviceColor(color, loc);

                        break;
                    }
                case Command.CommandType.SetS2L:
                    {
                        controller.SetS2L(Convert.ToBoolean(e.Command.Args[0]), loc);
                        break;
                    }
                case Command.CommandType.Exit:
                    {
                        flyingServer.Stop();
                        DIRECTflyingServer.Stop();
                        break;
                    }
            }
        }
    }
}
