﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
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
        DriverManager driverManager;
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

        public void Run()
        {
            flyingServer = new Server(3636);
            flyingServer.OnServerStart += OnServerStart;
            flyingServer.OnServerStop += OnServerStop;
            flyingServer.OnCommandIncoming += OnCommandIncoming;
            
            controller = new DMXController(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().FullName), "devices"));
            controller.OnDeviceLoaded += OnDeviceLoaded;

            driverManager = new DriverManager(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            driverManager.OnDriverLoaded += OnDriverLoaded;
            driverManager.OnDriverUnloaded += OnDriverUnloaded;
            driverManager.Load();
            driverManager.StartWatching();

            Console.WriteLine("\nLoading devices from folder '" + controller.DeviceDefinitionsLocation + "' ..");
            controller.LoadDevices();

            flyingServer.Start(true);
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

            switch(e.Command.Type)
            {
                case Command.CommandType.SetBrightness:
                {
                    DMXDevice.Location loc = (DMXDevice.Location)Enum.Parse(typeof(DMXDevice.Location), e.Command.Args[1]);
                    controller.SetDeviceMaster(Convert.ToByte(e.Command.Args[0]), loc);

                    break;
                }
                case Command.CommandType.SetColor:
                {
                    Color color = System.Drawing.ColorTranslator.FromHtml(e.Command.Args[0]);
                    DMXDevice.Location loc = (DMXDevice.Location)Enum.Parse(typeof(DMXDevice.Location), e.Command.Args[1]);
                    controller.SetDeviceColor(color, loc);

                    break;
                }
                case Command.CommandType.Exit:
                {
                    flyingServer.Stop();
                    break;
                }
            }
        }
    }
}
