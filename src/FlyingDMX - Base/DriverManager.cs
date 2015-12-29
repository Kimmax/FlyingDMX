using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Management;

namespace Nuernberger.FlyingDMX
{
    public class DriverManager
    {
        private string[] dllFileNames = null;

        public string DriverLocation { get; set; }

        WqlEventQuery insertQuery;
        ManagementEventWatcher insertWatcher;

        WqlEventQuery removeQuery;
        ManagementEventWatcher removeWatcher;

        ICollection<Driver> drivers;

        public event EventHandler<PluginLoadUnloadEventArgs> OnDriverLoaded;
        public event EventHandler<PluginLoadUnloadEventArgs> OnDriverUnloaded;

        public DriverManager(string driverLocation)
        {
            this.DriverLocation = driverLocation;
        }

        public void Load()
        {
            if (Directory.Exists(this.DriverLocation))
            {
                dllFileNames = Directory.GetFiles(this.DriverLocation, "*.dll");
            }

            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames)
            {
                try
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }
                catch
                {

                }
            }

            Type pluginType = typeof(Driver);
            ICollection<Type> pluginTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.BaseType != typeof(Driver) || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            pluginTypes.Add(type);
                        }
                    }
                }
            }

            drivers = new List<Driver>(pluginTypes.Count);
            foreach (Type type in pluginTypes)
            {
                Driver plugin = (Driver)Activator.CreateInstance(type, 30);
                drivers.Add(plugin);
            }

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                foreach (Driver driver in this.drivers)
                {
                    if (driver.HardwareIDs.Any(hID => (device.GetPropertyValue("DeviceID") as string).StartsWith(hID)))
                    {
                        driver.Start();

                        if (this.OnDriverLoaded != null)
                            this.OnDriverLoaded(this, new PluginLoadUnloadEventArgs(driver));

                        goto End;
                    }
                }
            }
        End:
            return;
        }

        public void StartWatching()
        {
            insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach(Driver driver in this.drivers)
            {
                if (driver.HardwareIDs.Any(hID => (instance.Properties["DeviceID"].Value as string).StartsWith(hID)))
                {
                    driver.Start();

                    if (this.OnDriverLoaded != null)
                        this.OnDriverLoaded(this, new PluginLoadUnloadEventArgs(driver));

                    break;
                }
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (Driver driver in this.drivers)
            {
                if (driver.HardwareIDs.Any(hID => (instance.Properties["DeviceID"].Value as string).StartsWith(hID)))
                {
                    driver.Stop();

                    if (this.OnDriverUnloaded != null)
                        this.OnDriverUnloaded(this, new PluginLoadUnloadEventArgs(driver));

                    break;
                }
            }
        }

        public void StopWatching()
        {
            this.insertWatcher.Stop();
            this.insertWatcher = null;

            this.removeWatcher.Stop();
            this.removeWatcher = null;
        }
    }
}
