using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Multiplayer;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI
{
    public class DistanceFromFlag : StackableUi
    {
        private Text text;
        private Transform player;
        private PlayerNode flagger;
        private DynamicMaze dynMaze;

        // Use this for initialization
        void Start()
        {
            UpperNode = GameObject.Find("DistUI/Distance")
                .GetComponent<DistanceFromExit>();
            LowerNode = GameObject.Find("PlayerNumUI/PlayerNum")
                .GetComponent<PlayerNumber>();

            // Self destruction if not in capture the flag game mode
            if (TransScene.Present.SelectedGameMode != TransScene.GameMode.CaptureTheFlag)
            {
                Destroy(transform.parent.gameObject);
            }
            else
            {
                // Binding
                text = GetComponent<Text>();
                player = GameObject.Find("Player").transform;
                dynMaze = GameObject.Find("MapScripts").GetComponent<DynamicMaze>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (FlagHolder.Present != null)
            {
                // Not you
                if (FlagHolder.Present.Id != PlayerNode.present.id)
                {
                    // Renew flagger
                    flagger = PlayerNodes.present.GetPlayer(FlagHolder.Present.Id);

                    // Calculate distance between flagger and player
                    text.text =
                        string.Format("{0:F1}", Mathf.Round(
                            (Mathf.Sqrt(
                                Mathf.Pow(player.localPosition.x - flagger.position.x, 2) +
                                Mathf.Pow(player.localPosition.z - flagger.position.z, 2))
                             - 1.125f /* Radius of exit capsule collider + 1/4 of radius of player */)
                            * dynMaze.playerPerWall * 10) / 10);
                    text.text += " m away";
                }
                else
                {
                    text.text = "You Have Flag";
                }
            }
            else
            {
                text.text = "N/A";
            }
        }
    }
}
