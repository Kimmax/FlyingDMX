using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Nuernberger.FlyingDMX
{
    public class DMXController
    {
        public string DeviceDefinitionsLocation { get; private set; }

        public List<DMXDevice> DMXDevices { get; private set; }

        public event EventHandler<DMXDeviceLoadedEventArgs> OnDeviceLoaded;
        public event EventHandler<ColorChangedEventArgs> OnColorChange;

        public Driver DMXDriver { get; set; }

        public DMXController(string deviceDefinationLocation)
        {
            this.DeviceDefinitionsLocation = deviceDefinationLocation;
            this.DMXDevices = new List<DMXDevice>();
        }

        public void SetDeviceMaster(byte value, DMXDevice.Location loc)
        {
            foreach (DMXDevice device in DMXDevices.Where(device => device.DeviceLocation == loc || loc == DMXDevice.Location.All))
            {
                DMXDriver.SetDMXValue(device.Master, value);
            }
        }

        public void SetDeviceColor(Color color, DMXDevice.Location loc)
        {
            foreach(DMXDevice device in DMXDevices.Where(device => device.DeviceLocation == loc || loc == DMXDevice.Location.All))
            {
                DMXDriver.SetDMXValue(device.R, color.R);
                DMXDriver.SetDMXValue(device.G, color.G);
                DMXDriver.SetDMXValue(device.B, color.B);

                if (this.OnColorChange != null)
                    this.OnColorChange(this, new ColorChangedEventArgs(color, device));
            }
        }

        public void LoadDevices()
        {
            foreach (string file in Directory.GetFiles(this.DeviceDefinitionsLocation, "*.flyDev"))
            {
                IniFile deviceDefinition = new IniFile(file);
                foreach(string device in deviceDefinition.GetSectionsA())
                {
                    DMXDevice dmxDevice = new DMXDevice();
                    dmxDevice.Name = device;

                    Dictionary<string, string> channelDefinitions = deviceDefinition.GetKeyValuesPair(device);
                    foreach(KeyValuePair<string, string> channel in channelDefinitions)
                    {
                        switch (channel.Key)
                        {
                            case "Name":
                            {
                                dmxDevice.Name = channel.Value;
                                break;
                            }
                            case "R":
                            {
                                dmxDevice.R = Convert.ToInt32(channel.Value);
                                break;
                            }
                            case "G":
                            {
                                dmxDevice.G = Convert.ToInt32(channel.Value);
                                break;
                            }
                            case "B":
                            {
                                dmxDevice.B = Convert.ToInt32(channel.Value);
                                break;
                            }
                            case "Master":
                            {
                                dmxDevice.Master = Convert.ToInt32(channel.Value);
                                break;
                            }
                            case "Strobe":
                            {
                                dmxDevice.Strobe = Convert.ToInt32(channel.Value);
                                break;
                            }
                            case "Sound2Light":
                            {
                                dmxDevice.Sound2Light = Convert.ToInt32(channel.Value);
                                break;
                            }
                            case "Location":
                            {
                                dmxDevice.DeviceLocation = (DMXDevice.Location)Enum.Parse(typeof(DMXDevice.Location), channel.Value, true);
                                break;
                            }
                        }
                    }

                    this.DMXDevices.Add(dmxDevice);

                    if (this.OnDeviceLoaded != null)
                        this.OnDeviceLoaded(this, new DMXDeviceLoadedEventArgs(dmxDevice));
                }
            }
        }
    }
}
