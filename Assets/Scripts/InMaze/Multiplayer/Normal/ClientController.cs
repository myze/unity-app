using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;

namespace Assets.Scripts.InMaze.Multiplayer.Normal
{
    public class ClientController : AbsClientController
    {
        private DynamicExit dynExit;

        protected override void Awake()
        {
            base.Awake();
            // Binding
            dynExit = GameObject
                .Find("MapScripts")
                .GetComponent<DynamicExit>();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            // If EscapedSeq received
            if (PlayerNodes.present != null)
                foreach (JExtNode node in PlayerNodes.present.Ext)
                    if (node is EscapeSeq)
                    {
                        // Enter spectator mode
                        Spectator.Present = new Spectator(
                            Essentials.GetCurrentIP().ToString()
                        );
                    }
        }

        // Update current player
        protected override PlayerNode UpdatePlayer()
        {            
            PlayerNode pl = base.UpdatePlayer();
            if (!pl.ContainsExt(typeof(EscapedInfo)) && dynExit.isEscaped)
                pl.AddExt(new EscapedInfo());

            return pl;
        }
    }
}
