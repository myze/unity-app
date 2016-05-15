using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI
{
    public class PlayerNumber : StackableUi
    {
        // Use this for initialization
        void Start()
        {
            // Set next node
            LowerNode = GameObject.Find("TimeToBeat/TimePanel")
                .GetComponent<Counting>();
            UpperNode = GameObject.Find("FlagUI/Distance")
                .GetComponent<DistanceFromFlag>();

            // Self destruction if not in multiplayer mode
            if (GameObject.Find("MultiplayerScripts") == null)
            {
                Destroy(transform.parent.gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerNodes.present != null)
                // Update number of player(s) (including self)
                GetComponent<Text>().text = PlayerNodes.present.Count.ToString();
        }
    }
}
