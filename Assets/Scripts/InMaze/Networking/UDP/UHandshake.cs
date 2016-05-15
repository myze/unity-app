using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Assets.Scripts.InMaze.Networking.Jsonify;
using SimpleJSON;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking.UDP
{
    public class UHandshake : UDP
    {
        public string MapId;
        public TransScene.GameMode GameMode;
        public bool IsDone;

        private readonly string broadcastAddr;
        private readonly int timesRetry;
        private readonly int timeout;
        private string response;

        public UHandshake(string broadcastAddress, int timesRetry, int timeout) :
            base(
                new UdpClient(UClient.PORT),
                new IPEndPoint(
                    Essentials.GetCurrentIP(),
				UClient.PORT
            ))
        {
            msg_prefix = "Handshake: ";
            this.timesRetry = timesRetry;
            this.timeout = timeout;
            broadcastAddr = broadcastAddress;
        }

        private bool checkResponse(string response)
        {
            return response != null &&
                   !response.Contains("Status") &&
                   response.Contains("\"MapId\":") &&
                   response.Contains("\"GameMode\":");
        }

        public override void start()
        {
            try
            {
                string packet = new Spectator(Essentials.GetCurrentIP().ToString())
                    .ToString();
                // Resend spectator(handshake) JSON for N times
                for (int i = 0; i < timesRetry && !checkResponse(response); i++)
                {
                    // Send spectator JSON to broadcast address for map id resolving
                    sendOutBound(
                        packet,
                        broadcastAddr,
                        UServer.PORT
                    );

                    // Start receive thread
                    new Thread(() => { response = receiveInBound(); }).Start();

                    // Wait till has reponse / timeout
                    for (int j = 0; j < timeout && !checkResponse(response); j++)
                        Thread.Sleep(1);

                    // If no response
                    if (!checkResponse(response))
                        // Loopback error code
                        sendOutBound(
                            StatusCode.Request_Timeout.ToString(),
                            "127.0.0.1",
							UClient.PORT
                        );
                }

                if (checkResponse(response))
                {
                    // Resolved map id successfully
                    JSONClass jc = (JSONClass) JSON.Parse(response);
                    MapId = jc["MapId"];
                    GameMode = Essentials.ParseEnum<TransScene.GameMode>(jc["GameMode"]);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception from UHandshake.start(): \n" + e);
            }
            finally
            {
                // Indicate end of thread
                IsDone = true;
            }
        }
    }
}
