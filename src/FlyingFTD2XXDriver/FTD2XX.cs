using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Nuernberger.FlyingDMX.Drivers
{
    public class FTD2XX : Driver
    {
        public override string FullName
        { 
            get { return "FTD2XX Usb-Interface"; } 
            set {} 
        }

        public override string Description
        { 
            get { return "Driver for the FTD2xx usb-to-dmx interface"; } 
            set {} 
        }

        public override string[] HardwareIDs
        { 
            get { return new string[] { @"USB\VID_0403&PID_6001" }; } 
            set {} 
        }

        public byte[] buffer { get; private set; }
        public uint handle;
        public bool done = false;
        public int bytesWritten = 0;
        public FT_STATUS status;
 
        public const byte BITS_8 = 8;
        public const byte STOP_BITS_2 = 2;
        public const byte PARITY_NONE = 0;
        public const UInt16 FLOW_NONE = 0;
        public const byte PURGE_RX = 1;
        public const byte PURGE_TX = 2;

        Thread workThread;

        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_Open(UInt32 uiPort, ref uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_Close(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_Read(uint ftHandle, IntPtr lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesReturned);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_Write(uint ftHandle, IntPtr lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesWritten);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetDataCharacteristics(uint ftHandle, byte uWordLength, byte uStopBits, byte uParity);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetFlowControl(uint ftHandle, char usFlowControl, byte uXon, byte uXoff);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_GetModemStatus(uint ftHandle, ref UInt32 lpdwModemStatus);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_Purge(uint ftHandle, UInt32 dwMask);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetRts(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_ClrRts(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetDtr(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_ClrDtr(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetBreakOn(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetBreakOff(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_GetStatus(uint ftHandle, ref UInt32 lpdwAmountInRxQueue, ref UInt32 lpdwAmountInTxQueue, ref UInt32 lpdwEventStatus);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_ResetDevice(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_SetDivisor(uint ftHandle, char usDivisor);
        [DllImport("FTD2XX.dll")]
        public static extern FT_STATUS FT_Rescan();

        public FTD2XX(int frameRate) : base(frameRate)
        {
            this.buffer = new byte[16];
        }

        public override void Start()
        {
            var ports = SerialPort.GetPortNames();
            foreach(string port in ports)
            {
                SerialPort sPort = new SerialPort(port);
                sPort.Open();
                sPort.Close();
                sPort.Dispose();
            }

            handle = 0;
            status = FT_Open(0, ref handle);
            done = false;
            workThread = new Thread(new ThreadStart(writeData));
            workThread.Start();
        }

        public override void SetDMXValue(int channel, byte value)
        {
            if (buffer != null)
            {
                buffer[channel] = value;
            }
        }
 
        public void writeData()
        {
            while (!done)
            {
                InitOpenDMX();
                FT_SetBreakOn(handle);
                FT_SetBreakOff(handle);
                bytesWritten = write(handle, buffer, buffer.Length);
                Thread.Sleep(FrameRate);
            }
        }
 
        public  int write(uint handle, byte[] data, int length)
        {
            IntPtr ptr = Marshal.AllocHGlobal((int)length);
            Marshal.Copy(data, 0, ptr, (int)length);
            uint bytesWritten = 0;
            status = FT_Write(handle, ptr, (uint)length, ref bytesWritten);
            return (int)bytesWritten;
        }
 
        public void InitOpenDMX()
        {
            status = FT_ResetDevice(handle);
            status = FT_SetDivisor(handle, (char)12);  // set baud rate
            status = FT_SetDataCharacteristics(handle, BITS_8, STOP_BITS_2, PARITY_NONE);
            status = FT_SetFlowControl(handle, (char)FLOW_NONE, 0, 0);
            status = FT_ClrRts(handle);
            status = FT_Purge(handle, PURGE_TX);
            status = FT_Purge(handle, PURGE_RX);
        }

        public override void Stop()
        {
            this.done = true;
            this.workThread.Abort();
            this.workThread = null;
            status = FT_ResetDevice(handle);
            status = FT_Close(handle);
        }

        /// <summary>
        /// Enumaration containing the varios return status for the DLL functions.
        /// </summary>
        public enum FT_STATUS
        {
            FT_OK = 0,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,
            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_OTHER_ERROR
        };
    }
}