using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Assets.Scripts.InMaze.Networking.Jsonify;
using SimpleJSON;
using UnityEngine;

// Reference using UDP Services
// https://msdn.microsoft.com/en-us/library/tst0kwb1(v=vs.110).aspx

namespace Assets.Scripts.InMaze.Networking.UDP
{
    public class UClient : UDP
    {
        public const int PORT = 4689;
        public const int WAIT_FOR = 100;
        public const int TIMEOUT = 10;

        private string serverIP;
        private int notFoundCount;

        public UClient(string serverIP) : base(
            new UdpClient(PORT),
            new IPEndPoint(
                Essentials.GetCurrentIP(),
                PORT
            ))
        {
            // Constructor chaining
            this.serverIP = serverIP;
            msg_prefix = "Client: ";
        }

        private void loopback(JSONClass message)
        {
            // Send a closing message to listening port
            sendOutBound(message.ToString(), "127.0.0.1", PORT);
        }

        private bool isValidResponse(string response)
        {
            try
            {
                // Check if it can parse into JSON
                response = ((JSONClass)JSON.Parse(response)).ToString();
                return response != StatusCode.Request_Timeout.ToString() &&
                       (response.Contains("Status") || response.Contains("Players"));
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string receiveCycle()
        {
            string response = StatusCode.Request_Timeout;
            do
            {
                // Send outbound to server
                sendOutBound(
                    // Check if in spectating mode
                    (Spectator.Present == null) ? 
                    PlayerNode.present.ToString() : Spectator.Present.ToString(),
                    serverIP,
                    UServer.PORT
                );

                new Thread(() => { response = receiveInBound(); }).Start();

                // Wait till has reponse / timeout
                for (int i = 0; i < WAIT_FOR && !isValidResponse(response); i++)
                    Thread.Sleep(1);

                if (!isValidResponse(response))
                    // Send status message to localhost
                    loopback(
                        (notFoundCount++ < TIMEOUT) ? StatusCode.Request_Timeout : StatusCode.Not_Found
                    );
                else
                    break;

            } while (!isStopped);

            return response;
        }

        public override void start()
        {
            try
            {
                while (!isStopped)
                {
                    // Start receive cycle
                    string response = receiveCycle();

                    // Get JSON object from server byte stream
                    JSONClass serverJSON = (JSONClass)JSON.Parse(response);

                    // Check if contains handshaking ID
                    if (serverJSON["Uid"] != null)
                        PlayerNode.present.id = serverJSON["Uid"].AsInt;

                    // Check if contains server's IP instead of broadcast address
                    if (serverJSON["IP"] != null)
                        serverIP = serverJSON["IP"];

                    // Check for status message
                    if (serverJSON["Status"] != null)
                    {
                        statusCode = serverJSON["Status"].AsInt;
                        if (serverJSON["Status"].AsInt == StatusCode.SERVER_CLOSED)
                        {
                            stop();
                            throw new Exception("Status code = " + serverJSON["Status"].AsInt);
                        }
                    }
                    else
                    {
                        // Reset notFoundCount & statusCode
                        statusCode = notFoundCount = 0;

                        // Update received server's data
                        PlayerNodes.present = PlayerNodes.Parse(serverJSON);
                    }

                    // Thread pause
                    Thread.Sleep(TICK_PAUSE);
                }

            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception from UClient.start(): \n" + e.ToString());
            }
        }
    }
}
