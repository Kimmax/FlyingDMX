using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public static class IPHelper
    {
        public static IPAddress GetBroadcastIP()
        {
            IPAddress maskIP = GetHostMask();
            IPAddress hostIP = GetHostIP();

            if (maskIP == null || hostIP == null)
                return null;

            byte[] complementedMaskBytes = new byte[4];
            byte[] broadcastIPBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                complementedMaskBytes[i] = (byte)~(maskIP.GetAddressBytes().ElementAt(i));
                broadcastIPBytes[i] = (byte)((hostIP.GetAddressBytes().ElementAt(i)) | complementedMaskBytes[i]);
            }

            return new IPAddress(broadcastIPBytes);

        }


        public static IPAddress GetHostMask()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface Interface in Interfaces)
            {
                IPAddress hostIP = GetHostIP();

                UnicastIPAddressInformationCollection UnicastIPInfoCol = Interface.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.Address.ToString() == hostIP.ToString())
                    {
                        try
                        {
                            return UnicatIPInfo.IPv4Mask;
                        }
                        catch(NotImplementedException)
                        {
                            return IPAddress.Parse(IPInfoTools.GetIPv4Mask(Interface.Description));
                        }
                    }
                }
            }

            return null;
        }

        public static IPAddress GetHostIP()
        {
            foreach (IPAddress ip in (Dns.GetHostEntry(Dns.GetHostName())).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }

            return null;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct ifa_ifu
        {
            [FieldOffset(0)]
            public IntPtr ifu_broadaddr;

            [FieldOffset(0)]
            public IntPtr ifu_dstaddr;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ifaddrs
        {
            public IntPtr ifa_next;
            public string ifa_name;
            public uint ifa_flags;
            public IntPtr ifa_addr;
            public IntPtr ifa_netmask;
            public ifa_ifu ifa_ifu;
            public IntPtr ifa_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct sockaddr_in
        {
            public ushort sin_family;
            public ushort sin_port;
            public uint sin_addr;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct sockaddr_in6
        {
            public ushort sin6_family;   /* AF_INET6 */
            public ushort sin6_port;     /* Transport layer port # */
            public uint sin6_flowinfo; /* IPv6 flow information */
            public in6_addr sin6_addr;     /* IPv6 address */
            public uint sin6_scope_id; /* scope id (new in RFC2553) */
        }

        [StructLayout(LayoutKind.Sequential)]
        struct in6_addr
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] u6_addr8;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct sockaddr_ll
        {
            public ushort sll_family;
            public ushort sll_protocol;
            public int sll_ifindex;
            public ushort sll_hatype;
            public byte sll_pkttype;
            public byte sll_halen;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] sll_addr;
        }

        internal static class IPInfoTools
        {
            const int AF_INET = 2;
            const int AF_INET6 = 10;
            const int AF_PACKET = 17;

            [DllImport("libc")]
            static extern int getifaddrs(out IntPtr ifap);

            [DllImport("libc")]
            static extern void freeifaddrs(IntPtr ifap);

            internal static string GetIPv4Mask(string networkInterfaceName)
            {
                IntPtr ifap;
                if (getifaddrs(out ifap) != 0)
                {
                    throw new SystemException("getifaddrs() failed");
                }

                try
                {
                    var next = ifap;
                    while (next != IntPtr.Zero)
                    {
                        var addr = (ifaddrs)Marshal.PtrToStructure(next, typeof(ifaddrs));
                        var name = addr.ifa_name;

                        if (addr.ifa_addr != IntPtr.Zero)
                        {
                            var sockaddr = (sockaddr_in)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr_in));
                            switch (sockaddr.sin_family)
                            {
                                case AF_INET6:
                                    //sockaddr_in6 sockaddr6 = (sockaddr_in6)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr_in6));
                                    break;
                                case AF_INET:
                                    if (name == networkInterfaceName)
                                    {
                                        var netmask = (sockaddr_in)Marshal.PtrToStructure(addr.ifa_netmask, typeof(sockaddr_in));
                                        var ipAddr = new IPAddress(netmask.sin_addr);  // IPAddress to format into default string notation
                                        return ipAddr.ToString();
                                    }
                                    break;
                                case AF_PACKET:
                                    {
                                        var sockaddrll = (sockaddr_ll)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr_ll));
                                        if (sockaddrll.sll_halen > sockaddrll.sll_addr.Length)
                                        {
                                            Console.Error.WriteLine("Got a bad hardware address length for an AF_PACKET {0} {1}",
                                                                    sockaddrll.sll_halen, sockaddrll.sll_addr.Length);
                                            next = addr.ifa_next;
                                            continue;
                                        }
                                    }
                                    break;
                            }
                        }

                        next = addr.ifa_next;
                    }
                }
                finally
                {
                    freeifaddrs(ifap);
                }

                return null;
            }
        }
    }
}
