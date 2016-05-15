using System.Collections;
using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;

namespace Assets.Scripts.InMaze.Multiplayer
{
    public class CostarsController : MonoBehaviour, IMultiplayer
    {
        private DynamicMaze dynamicMaze;

        IEnumerator createCostar(PlayerNode p)
        {
            GameObject costar = Instantiate(Resources.Load<GameObject>("prefab/unitychan_costar"));
            costar.name = "Costar" + p.id;

            // Initalize Costar.cs index
            costar.GetComponent<Costar>().id = p.id;

            yield return null;

            // Hair dyeing
            Material hair = Instantiate(costar
                .transform
                .Find("mesh_root/tail")
                .GetComponent<SkinnedMeshRenderer>()
                .material);
            hair.color = p.color;

            // Dye all hair related parts
            foreach (GameObject o in GameObject.FindGameObjectsWithTag("UnityChan_hair"))
                // If belongs to current costar
                if (o.transform.root == costar.transform)
                {
                    o.GetComponent<SkinnedMeshRenderer>().material = hair;
                    yield return null;
                }

            yield return null;
        }

        IEnumerator removeCostar(PlayerNode p)
        {
            // If gameObject CostarN is available
            GameObject costar = GameObject.Find("Costar" + p.id);
            if (costar)
            {
                // Remove such player
                Destroy(costar);
                yield return null;
            }
        }

        public void Quit(PlayerNode p)
        {
            StartCoroutine(removeCostar(p));
        }

        public void Join(PlayerNode p)
        {
            StartCoroutine(createCostar(p));
        }

        // Use this for initialization
        void Start()
        {
            // Binding
            dynamicMaze = GameObject.Find("MapScripts").GetComponent<DynamicMaze>();
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerNode.present != null &&
                PlayerNode.present.id != -1 &&
                PlayerNodes.present != null)
                // Loop through every players in PlayerNodes
                foreach (PlayerNode p in PlayerNodes.present.GetPlayers())
                {
                    // If not oneself
                    if (p.id != PlayerNode.present.id)
                        // If x coordinate matches
                        if (p.position.x >= dynamicMaze.centerPosition.x - dynamicMaze.renderSize * 15 &&
                            p.position.x <= dynamicMaze.centerPosition.x + dynamicMaze.renderSize * 15 &&
                            // If z(y) coordinate matches
                            p.position.z >= dynamicMaze.centerPosition.z - dynamicMaze.renderSize * 15 &&
                            p.position.z <= dynamicMaze.centerPosition.z + dynamicMaze.renderSize * 15)
                        {
                            // If away from this chunk previously
                            if (GameObject.Find("Costar" + p.id) == null)
                                // Add back to current chunk
                                StartCoroutine(createCostar(p));
                        }
                        else
                        {
                            // Remove such player from current chunk
                            StartCoroutine(removeCostar(p));
                        }
                }
        }
    }
}
