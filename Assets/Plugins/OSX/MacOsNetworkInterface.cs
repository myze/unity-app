// From GitHub developer gyurisc
// Repo NetworkTest, https://github.com/gyurisc/NetworkTest/blob/master/NetworkTest/
// All rights reserved to gyurisc.

using System;
using System.Runtime.InteropServices; 
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation; 

namespace NetworkTest.Platform
{
	// class is adapted from mono baseclass 
	// mcs/class/System/System.Net.NetworkInformation/NetworkInterface.cs
	public class MacOsNetworkInterface
	{
		// TODO: refactor this class and get rid of the unused stuff
		[DllImport("libc")]
		static extern int if_nametoindex(string ifname);

		[DllImport ("libc")]
		static extern int getifaddrs (out IntPtr ifap);

		[DllImport ("libc")]
		static extern void freeifaddrs (IntPtr ifap);


		const int AF_INET  = 2;
		const int AF_INET6 = 30;
		const int AF_LINK  = 18;

		public MacOsNetworkInterface()
		{
		}


		public static NetworkInterfaceInformation [] GetIpAddressTable ()
		{
			var interfaces = new Dictionary <string, NetworkInterfaceInformation> ();
			IntPtr ifap;
			if (getifaddrs (out ifap) != 0)
				throw new SystemException ("getifaddrs() failed");

			try {
				IntPtr next = ifap;
				while (next != IntPtr.Zero) {
					MacOsStructs.ifaddrs addr = (MacOsStructs.ifaddrs) Marshal.PtrToStructure (next, typeof (MacOsStructs.ifaddrs));
					IPAddress address = IPAddress.None;
					IPAddress mask = IPAddress.None;
					string    name = addr.ifa_name;
					int       index = -1;
					byte[]    macAddress = null;
					NetworkInterfaceType type = NetworkInterfaceType.Unknown;

					// Netmask 
					if(addr.ifa_netmask != IntPtr.Zero) { 
						MacOsStructs.sockaddr mapsockaddr = (MacOsStructs.sockaddr) Marshal.PtrToStructure (addr.ifa_netmask, typeof (MacOsStructs.sockaddr));

						if (mapsockaddr.sa_family == AF_INET) {
							MacOsStructs.sockaddr_in sockaddrin = (MacOsStructs.sockaddr_in) Marshal.PtrToStructure (addr.ifa_netmask, typeof (MacOsStructs.sockaddr_in));
							mask = new IPAddress (sockaddrin.sin_addr);
						}
					}

					// Address
					if (addr.ifa_addr != IntPtr.Zero) {
						MacOsStructs.sockaddr sockaddr = (MacOsStructs.sockaddr) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr));

						if (sockaddr.sa_family == AF_INET6) {
							MacOsStructs.sockaddr_in6 sockaddr6 = (MacOsStructs.sockaddr_in6) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in6));
							address = new IPAddress (sockaddr6.sin6_addr.u6_addr8, sockaddr6.sin6_scope_id);
						} else if (sockaddr.sa_family == AF_INET) {
							MacOsStructs.sockaddr_in sockaddrin = (MacOsStructs.sockaddr_in) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in));
							address = new IPAddress (sockaddrin.sin_addr);
						} else if (sockaddr.sa_family == AF_LINK) {
							MacOsStructs.sockaddr_dl sockaddrdl = new MacOsStructs.sockaddr_dl ();
							sockaddrdl.Read (addr.ifa_addr);

							macAddress = new byte [(int) sockaddrdl.sdl_alen];
							Array.Copy (sockaddrdl.sdl_data, sockaddrdl.sdl_nlen, macAddress, 0, Math.Min (macAddress.Length, sockaddrdl.sdl_data.Length - sockaddrdl.sdl_nlen));
							index = sockaddrdl.sdl_index;

							int hwtype = (int) sockaddrdl.sdl_type;
							if (Enum.IsDefined (typeof (MacOsArpHardware), hwtype)) {
								switch ((MacOsArpHardware) hwtype) {
								case MacOsArpHardware.ETHER:
									type = NetworkInterfaceType.Ethernet;
									break;

								case MacOsArpHardware.ATM:
									type = NetworkInterfaceType.Atm;
									break;

								case MacOsArpHardware.SLIP:
									type = NetworkInterfaceType.Slip;
									break;

								case MacOsArpHardware.PPP:
									type = NetworkInterfaceType.Ppp;
									break;

								case MacOsArpHardware.LOOPBACK:
									type = NetworkInterfaceType.Loopback;
									macAddress = null;
									break;

								case MacOsArpHardware.FDDI:
									type = NetworkInterfaceType.Fddi;
									break;
								}

							}
						}
					}

					NetworkInterfaceInformation iface = null;

					if (!interfaces.TryGetValue (name, out iface)) {
						iface = new NetworkInterfaceInformation (name);
						interfaces.Add (name, iface);
					}

					if (!address.Equals (IPAddress.None))
						iface.Addresses.Add (address);

					if(!mask.Equals(IPAddress.None))
					{
						iface.SubnetMask = mask;
					}

					if (macAddress != null || type == NetworkInterfaceType.Loopback)
						iface.Index = index;
					iface.MacAddress = macAddress;
					iface.Type = type;
					next = addr.ifa_next;
				}
			} finally {
				freeifaddrs (ifap);
			}

			NetworkInterfaceInformation [] result = new NetworkInterfaceInformation [interfaces.Count];
			int x = 0;
			foreach (NetworkInterfaceInformation thisInterface in interfaces.Values) {
				result [x] = thisInterface;
				x++;
			}
			return result;
		}
	}
}