// From GitHub developer gyurisc
// Repo NetworkTest, https://github.com/gyurisc/NetworkTest/blob/master/NetworkTest/
// All rights reserved to gyurisc.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation; 
using System.Linq;

namespace NetworkTest.Platform
{
	public class NetworkInterfaceInformation 
	{
		public NetworkInterfaceInformation(string name)
		{
			Name = name; 
			Addresses = new List<IPAddress> ();
		}

		public string Name {
			get;
			set;
		}

		public int Index {
			get;
			set;
		}
		public List<IPAddress> Addresses {
			get;
			set;
		}

		public byte[] MacAddress {
			get;
			set;
		}

		public string MacAddressString {
			get { 
				if (MacAddress != null) {
					return string.Join (":", (from z in MacAddress
						select z.ToString ("X2")).ToArray ());
				}

				return string.Empty;
			}
		}

		public IPAddress SubnetMask {
			get;
			set;
		}

		public NetworkInterfaceType Type {
			get;
			set;
		}
	}
}
