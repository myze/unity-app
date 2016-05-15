using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;

namespace Assets.Scripts.InMaze.Multiplayer.Normal
{
    public class ServerController : AbsServerController
    {
        // Parameter for gamemode "Normal"
        private Dictionary<int, int> escapeSequence;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            escapeSequence = new Dictionary<int, int>();
        }

        protected override void PlayerUpdate(PlayerNode player)
        {
            // Contains JExtNode
            if (player.Ext.Length > 0)
                // Is escaped
                if (player.Ext[0] is EscapedInfo)
                {
                    // If not in dictionary
                    if (!escapeSequence.ContainsKey(player.id))
                    {
                        escapeSequence.Add(
                            player.id,
                            (escapeSequence.Count > 0) ?
                            escapeSequence.Values.Max() + 1 : 1
                        );
                        // Create return object
                        EscapeSeq seq = new EscapeSeq()
                        {
                            Place = escapeSequence[player.id],
                            TargetId = player.id
                        };
                        PlayerNodes.serverSide.AddExt(seq);
                    }
                }
        }
    }
}
