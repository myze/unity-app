using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking.UDP
{
    public class USpectator : UDP
    {
        public const string DEFAULT_URL = "myze.xyz";

        public const int PORT = 7689;
        private new const int TICK_PAUSE = UDP.TICK_PAUSE * 100;
        private string ip;

        public USpectator(string address_or_domain) : base(
            new UdpClient(PORT),
            new IPEndPoint(
                Essentials.GetCurrentIP(),
                PORT
            ))
        {
            msg_prefix = "Spectator: ";
            ip = address_or_domain;
        }

        public override void start()
        {
            try
            {
                // Resolve domain name
                IPAddress[] ips = Dns.GetHostAddresses(ip);
                if (ips.Length > 0)
                {
                    ip = ips[0].ToString();
                    while (!isStopped)
                    {
                        // Loopback for sending spectating JSON to server
                        sendOutBound(
                            new Spectator(ip).ToString(),
                            "127.0.0.1",
                            UServer.PORT
                            );
                        // Thread pause
                        Thread.Sleep(TICK_PAUSE);
                    }
                }
                else
                {
                    throw new Exception("Invalid IP address / domain name");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception from USpectator.start(): \n" + e.ToString());
            }
        }
    }
}
