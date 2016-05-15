using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;
using BaseServer = Assets.Scripts.InMaze.Multiplayer.Normal.ServerController;

namespace Assets.Scripts.InMaze.Multiplayer.CaptureTheFlag
{
    public class ServerController : BaseServer
    {
        private FlagHolder flagHolder;
        // Get flag in 1.5 unit;
        private const float Radius = 3f;
        private const int InvincibleInSeconds = 3;

        [SerializeField]
        private int flaggerId;
        [SerializeField]
        private int invincibleCounter;

        protected override void Start()
        {
            base.Start();
            flagHolder = new FlagHolder()
            {
                // Default holder will be host
                Id = 0
            };
        }

        protected override void PlayerUpdate(PlayerNode player)
        {
            base.PlayerUpdate(player);

			// Contains JExtNode
			if (player.Ext.Length > 0)
				// Is escaped
				if (player.Ext [0] is EscapedInfo) 
					if (player.id == flagHolder.Id)
						flagHolder.Id = PlayerNodes.serverSide.GetFirstPlayer ().id;

            // Check not self
            if (player.id != flagHolder.Id)
            {
				PlayerNode flagger = PlayerNodes.serverSide.GetPlayer (flagHolder.Id);

                // Check if collided
                if (invincibleCounter == 0 &&
                    player.position.x >= flagger.position.x - Radius &&
                    player.position.x <= flagger.position.x + Radius &&
                    player.position.z >= flagger.position.z - Radius &&
                    player.position.z <= flagger.position.z + Radius)
                {
                    // Change flager
                    flagHolder.Id = player.id;
                    // Player will be invincible for n seconds
                    new Thread(InvincibleCountDown).Start();
                }
            }
        }

        void InvincibleCountDown()
        {
            invincibleCounter = InvincibleInSeconds + 1;
            while (--invincibleCounter > 0)
                Thread.Sleep(1000);
        }

        // Update is called once per frame
        void Update()
        {
            // For display in editor only
            flaggerId = flagHolder.Id;

			if (!PlayerNodes.serverSide.ContainsExt (flagHolder)) {
				PlayerNodes.serverSide.AddExt (flagHolder);
			}
        }
    }
}
