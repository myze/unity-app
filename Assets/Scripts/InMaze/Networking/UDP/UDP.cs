using System.Net;
using System.Net.Sockets;
using System.Text;
using lz_string_csharp;
using SimpleJSON;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking.UDP
{
    public class StatusCode
    {
        private const string MSG_MODEL = "{\"Status\":\"0\"}";
        public const int SERVER_CLOSED = 503;
        public const int REQUEST_TIMEOUT = 408;
        public const int NOT_FOUND = 404;

        private static JSONClass makeJSON(int errorCode)
        {
            return (JSONClass)JSON.Parse(
                MSG_MODEL.Replace("0", errorCode.ToString())
            );
        }

        /* * * * * * * * * * * * * *\
         *  Actual Error Messages  *
        \* * * * * * * * * * * * * */

        public static JSONClass Server_Closed
        {
            get { return makeJSON(SERVER_CLOSED); }
        }

        public static JSONClass Request_Timeout
        {
            get { return makeJSON(REQUEST_TIMEOUT); }
        }

        public static JSONClass Not_Found
        {
            get { return makeJSON(NOT_FOUND); }
        }
    }

    public abstract class UDP
    {
        protected UdpClient listener;
        protected IPEndPoint groupEP;
        protected string msg_prefix = "";

        public bool isStopped { get; private set; }

        public int statusCode { get; protected set; }

        private const string
            MSG_OUTBOUND = "Outbound fired to {0} :\n {1}\n",
            MSG_WAIT = "Waiting inbound...",
            MSG_INBOUND = "Received inbound from {0} :\n {1}\n",
            MSG_PRE_DOWN = "Closing...";

        protected const int TICK_PAUSE = 1;

        // Mute all logs
        public const bool LOG_MUTE = false;

        protected UDP(UdpClient udpClient, IPEndPoint ipEndPoint)
        {
            listener = udpClient;
            groupEP = ipEndPoint;
            isStopped = false;
        }

        public abstract void start();

        public void stop()
        {
            listener.Close();
            isStopped = true;
            printLog(MSG_PRE_DOWN);
        }

        protected void printLog(string message)
        {
            if (!LOG_MUTE)
                Debug.Log(msg_prefix + message);
        }

        protected void printLog(string model, object[] args)
        {
            printLog(string.Format(model, args));
        }

        protected void sendOutBound(string data, string ipAddress, int port)
        {
            printLog(MSG_OUTBOUND, new object[] { ipAddress, data });

            Socket s = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            // For enabling broadcasting
            s.EnableBroadcast = true;

            IPAddress broadcast = IPAddress.Parse(ipAddress);

            byte[] sendbuf = Encoding.ASCII.GetBytes(data);
            IPEndPoint ep = new IPEndPoint(broadcast, port);

            s.SendTo(sendbuf, ep);
        }

        protected virtual string receiveInBound()
        {
            printLog(MSG_WAIT);
            byte[] bytes = listener.Receive(ref groupEP);

            string response = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

            printLog(MSG_INBOUND, new object[] { groupEP, response });

            return response;
        }
    }
}