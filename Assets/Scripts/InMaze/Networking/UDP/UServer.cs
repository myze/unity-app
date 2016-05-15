using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Assets.Scripts.InMaze.Networking.Jsonify;
using SimpleJSON;
using UnityEngine;

// Reference using UDP Services
// https://msdn.microsoft.com/en-us/library/tst0kwb1(v=vs.110).aspx
namespace Assets.Scripts.InMaze.Networking.UDP
{
    class PlayerDomain
    {
        // For avoiding update in 2 threads simultaneously
        public static bool IsLocked;

        public class DomainNode
        {
            public int id, timeOut;
            public Color color;
        }

        private const int TIMEOUT = 2000;
        private readonly Dictionary<string, DomainNode> domainDict =
            new Dictionary<string, DomainNode>();
        private int _id = -1;

        public int nextId { get { return ++_id; } }

        public void register(string ip, int _id)
        {
            System.Random rand = new System.Random();

            domainDict.Add(ip, new DomainNode()
            {
                id = _id,
                timeOut = 0,
                // Generate a random color
                color = Essentials.ParseColor(string.Format(
                    "{0:X2}{1:X2}{2:X2}{3:X2}",
                    rand.Next(0x77, 0xFF),
                    rand.Next(0x77, 0xFF),
                    rand.Next(0x77, 0xFF),
                    0xFF
                ))
            });
        }

        public void quit(string ip)
        {
            domainDict.Remove(ip);
        }

        public Color getColor(string ip)
        {
            return domainDict[ip].color;
        }

        public Color getColor(int id)
        {
            return domainDict
                .FirstOrDefault(x => x.Value.id == id).Value.color;
        }

        public void updateTimeOut(string ip)
        {
            domainDict[ip].timeOut++;
        }

        public void resetTimeOut(string ip)
        {
            domainDict[ip].timeOut = 0;
        }

        public int getTimeOut(string ip)
        {
            return domainDict[ip].timeOut;
        }

        public int getId(string ip)
        {
            return domainDict[ip].id;
        }

        public bool isTimeOut(string ip)
        {
            return domainDict[ip].timeOut >= TIMEOUT;
        }

        public Dictionary<string, DomainNode>.KeyCollection getIPs()
        {
            return domainDict.Keys;
        }

        public bool hasIP(string ip)
        {
            return domainDict.ContainsKey(ip);
        }
    }

    public class UServer : UDP
    {
        public const int PORT = 8964;
        private const string MSG_KICK = "Player {0} is kicked due to timeout";
        private const string MSG_TIMEOUTTICK_STOP = "TimeOutTick stopped";
        private const string MSG_DOMAIN_LOCKED = "PlayerDomain locked";

        private readonly PlayerDomain domain;
        private static bool serverLock = false;
        // Default null event
        private OnPlayerUpdatedEvent updateEvent = p => { };

        public delegate void OnPlayerUpdatedEvent(PlayerNode player);

        public UServer() : base(
            new UdpClient(PORT),
            new IPEndPoint(
                Essentials.GetCurrentIP(),
                PORT))
        {
            // Constructor chaining
            msg_prefix = "Server: ";
            domain = new PlayerDomain();
        }

        private void timeOutTick()
        {
            try
            {
                while (!isStopped)
                {
                    try
                    {
                        var buffer = new List<string>(domain.getIPs());
                        foreach (string ip in buffer)
                        {
                            // Increment of timeout reference
                            domain.updateTimeOut(ip);
                            // If timeout
                            if (domain.isTimeOut(ip))
                                // If process is NOT hooked up
                                if (!serverLock)
                                {
                                    printLog(MSG_KICK, new object[] { domain.getId(ip) });

                                    // Remove player in PlayerNodes
                                    PlayerNodes.serverSide.RemovePlayer(domain.getId(ip));
                                    // unregister player in domain
                                    domain.quit(ip);
                                }
                        }
                        // This thread runs every 1 ms
                        Thread.Sleep(1);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Ignored IndexOutOfRangeException
                        // 
                        // The reason for this exception is that
                        // a collision on update between threads
                        // UServer and TimeOutTick
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception from UServer.timeOutTick(): \n" + e.ToString());
            }
            finally
            {
                printLog(MSG_TIMEOUTTICK_STOP);
            }
        }

        public UServer SetOnPlayerUpdatedEventHandler(OnPlayerUpdatedEvent updateEvent)
        {
            this.updateEvent = updateEvent;
            return this;
        }

        private void substart(string response)
        {
            try
            {
                // Prevent simultaneously updating
                if (!PlayerDomain.IsLocked)
                {
                    // Lock PlayerDomain
                    PlayerDomain.IsLocked = true;
                    // Get JSON object from client byte stream
                    JSONClass clientJson = (JSONClass)JSON.Parse(response);
                    JSONClass players;

                    // If client is a player
                    if (clientJson["Spectator"] == null)
                    {
                        PlayerNode pl = PlayerNode.Parse(clientJson);

                        // If NOT registered in idDomain
                        if (!domain.hasIP(clientJson["IP"]))
                        {
                            // Register player in domain
                            domain.register(
                                clientJson["IP"],
                                domain.nextId
                                );

                            pl.id = domain.getId(clientJson["IP"]);

                            // Add to players
                            PlayerNodes.serverSide.AddPlayer(pl);
                        }
                        else
                        {
                            pl.id = domain.getId(clientJson["IP"]);

                            // Update received player's data
                            PlayerNodes.serverSide.AlterPlayer(
                                domain.getId(clientJson["IP"]),
                                pl
                                );
                        }

                        // Invoke delegate
                        updateEvent(pl);

                        // Reset player's timeout reference
                        domain.resetTimeOut(clientJson["IP"]);

                        players = (JSONClass)JSON.Parse(
                            PlayerNodes
                                .serverSide
                                .ToString(domain.getId(clientJson["IP"]))
                            );

                        // Assign ID
                        players.Add(
                            "Uid",
                            new JSONData(domain.getId(clientJson["IP"]))
                            );
                    }
                    // if client is a spectator
                    else
                    {
                        // If registered, quit
                        if (domain.hasIP(clientJson["IP"]))
                        {
                            PlayerNodes.serverSide.RemovePlayer(domain.getId(clientJson["IP"]));
                            domain.quit(clientJson["IP"]);
                        }
                        players = PlayerNodes.serverSide.ToJson();

                        // Add current map id
                        players.Add("MapId", new JSONData(TransScene.Present.MapId));
                        // Add current gamemode
                        players.Add("GameMode", 
                            new JSONData(TransScene.Present.SelectedGameMode.ToString()));
                    }

                    // Add colors to player
                    foreach (JSONClass jc in players["Players"].AsArray)
                    {
                        Color color = domain.getColor(jc["Id"].AsInt);
                        jc.Add("Color", new JSONData(string.Format(
                            "{0:X2}{1:X2}{2:X2}{3:X2}",
                            (int)(color.r * 0xFF),
                            (int)(color.g * 0xFF),
                            (int)(color.b * 0xFF),
                            0xFF
                            )));
                    }

                    // Put server IP into response message
                    players.Add("IP", new JSONData(Essentials.GetCurrentIP().ToString()));

                    // Send outbound to client
                    sendOutBound(
                        players.ToString(),
                        clientJson["IP"],
                        UClient.PORT
                        );
                }
                else
                {
                    // Indicates playerdomain is locked for accessing
                    printLog(MSG_DOMAIN_LOCKED);
                }
                // Unlock
                PlayerDomain.IsLocked = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception from UServer.substart(): \n" + e);
            }
        }

        public override void start()
        {
            // Create new serverSide of PlayerNodes
            PlayerNodes.serverSide = new PlayerNodes();
            // Start TimeOutTick
            new Thread(timeOutTick).Start();
            try
            {
                while (!isStopped)
                {
                    serverLock = false;
                    // Wait until any inbound
                    string response = receiveInBound();
                    serverLock = true;

                    // Run substart() in another thread
                    new Thread(() => { substart(response); }).Start();

                    // Thread pause
                    Thread.Sleep(TICK_PAUSE);
                }

            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception from UServer.start(): \n" + e);
            }
            finally
            {
                // Send Message of close indicator to all connected clients
                foreach (string ip in domain.getIPs())
                    sendOutBound(
                        StatusCode.Server_Closed.ToString(),
                        ip,
                        UClient.PORT
                    );
            }
        }
    }
}