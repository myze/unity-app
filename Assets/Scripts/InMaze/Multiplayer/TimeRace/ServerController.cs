using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;
using BaseServer = Assets.Scripts.InMaze.Multiplayer.Normal.ServerController;

namespace Assets.Scripts.InMaze.Multiplayer.TimeRace
{
    public class ServerController : BaseServer
    {
        private ServerEscTime set;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            set = new ServerEscTime()
            {
                // Server's best time on such map
                BestTime = PlayerPrefs.GetInt(TransScene.Present.MapId)
            };
        }

        public void Update()
        {
            if (!PlayerNodes.serverSide.ContainsExt(set))
                PlayerNodes.serverSide.AddExt(set);
        }
    }
}
