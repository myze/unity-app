using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;
using NetworkTest.Platform;

namespace Assets.Scripts
{
    public class Essentials
    {
        public static Color ParseColor(string rrggbbaa)
        {
            float r = ToColorVal(rrggbbaa.Substring(0, 2));
            float g = ToColorVal(rrggbbaa.Substring(2, 2));
            float b = ToColorVal(rrggbbaa.Substring(4, 2));
            float a = 1f;

            if (rrggbbaa.Length > 6)
                a = ToColorVal(rrggbbaa.Substring(6, 2));

            return new Color(r, g, b, a);
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static bool HasInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private static float ToColorVal(string subString)
        {
            return int.Parse(
                subString,
                System.Globalization.NumberStyles.HexNumber
            ) / 255f;
        }

        public static int GetTimestamp()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)))
                .TotalSeconds;
        }

        public static PhysicalAddress GetMAC()
        {
#if UNITY_STANDALONE_WIN
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Select(nic => nic.GetPhysicalAddress())
                .FirstOrDefault();
#elif UNITY_IOS || UNITY_STANDALONE_OSX
			// Note that since iOS 7 all request for MAC addr will return 02:00:00:00:00:00
			return NetworkInterface.GetAllNetworkInterfaces ()
				.Where (x => x.Name.Equals ("en0"))
				.First ()
				.GetPhysicalAddress ();
#elif UNITY_ANDROID
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                // Get current java context
                AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
                // Get Context class
                AndroidJavaClass Context = new AndroidJavaClass("android.content.Context");
                // Get WIFI_SERVICE reference
                string wifi_service = Context.GetStatic<string>("WIFI_SERVICE");
                // Get WifiManager
                AndroidJavaObject wifiManager = context.Call<AndroidJavaObject>("getSystemService", new object[] { wifi_service });
                // Get WifiInfo
                AndroidJavaObject d = wifiManager.Call<AndroidJavaObject>("getConnectionInfo");
                // Get netmask
                return PhysicalAddress.Parse(d.Call<string>("getMacAddress").Replace(":", ""));
            }
#else
			throw new NotImplementedException();
#endif
        }

        public static IPAddress GetCurrentIP()
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
			// From User reza, http://stackoverflow.com/a/17843977
			string ip = NetworkInterface.GetAllNetworkInterfaces ()
				.Where (x => x.Name.Equals ("en0"))
				.First ()
				.GetIPProperties ()
				.UnicastAddresses.Where (x => x.Address.AddressFamily == AddressFamily.InterNetwork)
				.First ().Address.ToString ();
			return IPAddress.Parse (ip);
#else
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => Regex.IsMatch(ip.ToString(), "\\d+\\.\\d+\\.\\d+\\.\\d+"));
#endif
        }

        public static IPAddress GetSubnetMask()
        {
#if UNITY_STANDALONE_WIN
            // From Jean-Paul Mikkers
            // http://www.java2s.com/Code/CSharp/Network/GetSubnetMask.htm

            IPAddress address = GetCurrentIP();
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", address));
#elif UNITY_STANDALONE_OSX || UNITY_IOS
			NetworkInterfaceInformation[] niis = MacOsNetworkInterface.GetIpAddressTable ();
			foreach (NetworkInterfaceInformation nii in niis) {
				// WLAN interface for OSX
				if (nii.Name == "en0")
					return nii.SubnetMask;
			}
			return null;
#elif UNITY_ANDROID
            try
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    // Get current java context
                    AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
                    // Get Context class
                    AndroidJavaClass Context = new AndroidJavaClass("android.content.Context");
                    // Get WIFI_SERVICE reference
                    string wifi_service = Context.GetStatic<string>("WIFI_SERVICE");
                    // Get WifiManager
                    AndroidJavaObject wifiManager = context.Call<AndroidJavaObject>("getSystemService",
                        new object[] { wifi_service });
                    // Get Dhcp Info
                    AndroidJavaObject d = wifiManager.Call<AndroidJavaObject>("getDhcpInfo");
                    // Get netmask
                    return new IPAddress(d.Get<int>("netmask"));
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error b " + e);
            }
            return null;
#else
            throw new NotImplementedException();
#endif
        }

        public static Texture TextureDecode(string ciphertext)
        {
            byte[] data = Convert.FromBase64String(ciphertext);
            Texture2D texture2D = new Texture2D(0, 0);
            texture2D.LoadImage(data);
            return texture2D;
        }

        // From knom 31 Dec 2008 6:31 AM
        // http://blogs.msdn.com/b/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            try
            {
                byte[] ipAdressBytes = address.GetAddressBytes();
                byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

                if (ipAdressBytes.Length != subnetMaskBytes.Length)
                    throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

                byte[] broadcastAddress = new byte[ipAdressBytes.Length];
                for (int i = 0; i < broadcastAddress.Length; i++)
                {
                    broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
                }
                return new IPAddress(broadcastAddress);
            }
            catch (Exception e)
            {
                Debug.Log("Error a " + e);
            }
            return null;
        }

        public static int GetStringWidth(string s, int size, Font font, FontStyle fontStyle = FontStyle.Normal)
        {
            // Request characters to be added to the font texture
            font.RequestCharactersInTexture(s, size, fontStyle);

            int retn = 0;
            foreach (char t in s)
            {
                CharacterInfo info = new CharacterInfo();
                if (font.GetCharacterInfo(t, out info, size))
                    retn += info.advance;
            }
            return retn;
        }

        public static float GetStringHeight(string s, int size, int startFrom = 1)
        {
            return Mathf.Sqrt(size * size * 2f) * (s.Count(c => c == '\n') + startFrom);
        }
    }
}
