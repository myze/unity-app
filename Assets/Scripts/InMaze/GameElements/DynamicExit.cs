using System.Collections;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;

namespace Assets.Scripts.InMaze.GameElements
{
    public class DynamicExit : MonoBehaviour
    {
        private DynamicMaze dynMaze;
        public Vector3 escapeCoord;
        public bool isEscaped;

        // Use this for initialization
        void Start()
        {
            if (MazeData.present.escape != null)
            {
                // Binding
                dynMaze = GameObject.Find("MapScripts").GetComponent<DynamicMaze>();
                escapeCoord = new Vector3(
                    MazeData.present.escape.X,
                    Resources.Load<GameObject>("prefab/escapeSpot")
                        .transform.localPosition.y,
                    MazeData.present.escape.Y
                );
            }
            else
            {
                // No escape point
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // If x coordinate matches
            if (escapeCoord.x >= dynMaze.centerPosition.x - dynMaze.renderSize * 15 &&
                escapeCoord.x <= dynMaze.centerPosition.x + dynMaze.renderSize * 15 &&
                // If z(y) coordinate matches
                escapeCoord.z >= dynMaze.centerPosition.z - dynMaze.renderSize * 15 &&
                escapeCoord.z <= dynMaze.centerPosition.z + dynMaze.renderSize * 15)
            {
                StartCoroutine(CreateExit());
            }
            else
            {
                StartCoroutine(RemoveExit());
            }
        }

        IEnumerator CreateExit()
        {
            // If gameObject Exit isn't available
            if (!GameObject.Find("Escape") && dynMaze.isMapReady)
            {
                GameObject escape = Instantiate(
                    Resources.Load<GameObject>("prefab/escapeSpot"));
                escape.name = "Escape";
                escape.transform.localPosition = escapeCoord;
                yield return null;
            }
        }

        IEnumerator RemoveExit()
        {
            // If gameObject Exit is available
            GameObject escape = GameObject.Find("Escape");
            if (escape)
            {
                // Remove such player
                Destroy(escape);
                yield return null;
            }
        }
    }
}
