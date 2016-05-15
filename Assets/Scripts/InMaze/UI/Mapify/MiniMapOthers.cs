using System.Collections;
using Assets.Scripts.InMaze.Multiplayer;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI.Mapify
{
    public class MiniMapOthers : MonoBehaviour, IMultiplayer, IMiniMapEvents, IReset
    {
        private MiniMap miniMap;

        // Use this for initialization
        void Start()
        {
            // Binding
            miniMap = GetComponent<MiniMap>();
            // Register for MiniMap
            miniMap.InsertMember(this);
        }

        void CreateOther(PlayerNode p)
        {
            // Add new UiOthers to minimap
            GameObject other = new GameObject();
            other.AddComponent<Image>();
            other.transform.SetParent(gameObject.transform);

            other.name = "OtherPlayer" + p.id;
            other.tag = "UiOthers";

            other.GetComponent<Image>().sprite = Resources.Load<Sprite>("GPS Device-100");
            other.GetComponent<Image>().type = Image.Type.Simple;
            other.GetComponent<Image>().color = p.color;

            other.GetComponent<RectTransform>().localScale = miniMap.orgScale;
            other.GetComponent<RectTransform>().sizeDelta =
                GameObject.Find("MiniPlayer")
                .GetComponent<RectTransform>()
                .sizeDelta;

            miniMap.ObjectShrink(other);
        }

        void RemoveOther(PlayerNode p)
        {
            // If gameObject OtherPlayerN is available
            if (GameObject.Find("OtherPlayer" + p.id))
                // Remove such player
                Destroy(GameObject.Find("OtherPlayer" + p.id));
        }

        IEnumerator Others(float x, float y)
        {
            // Loop through every players in PlayerNodes
            foreach (PlayerNode p in PlayerNodes.present.GetPlayers())
            {
                // If not oneself
                if (p.id != PlayerNode.present.id)
                    // If x coordinate matches
                    if (p.position.x >= x - miniMap.mapRenderSize * 5 &&
                        p.position.x <= x + miniMap.mapRenderSize * 5 &&
                        // If y(z) coordinate matches
                        p.position.z >= y - miniMap.mapRenderSize * 5 &&
                        p.position.z <= y + miniMap.mapRenderSize * 5)
                    {
                        // If away from this chunk previously
                        if (GameObject.Find("OtherPlayer" + p.id) == null)
                            // Add back to current chunk
                            CreateOther(p);

                        // Update position
                        miniMap.UpdatePos(
                            p.eulerAngles,
                            p.position,
                            GameObject.Find("OtherPlayer" + p.id)
                        );

                        yield return null;
                    }
                    else
                    {
                        // Remove such player from current chunk
                        RemoveOther(p);
                        yield return null;
                    }
            }
        }

        public void Join(PlayerNode p)
        {
            // If minimap is active
            if (gameObject.activeSelf)
                // Create such player on minimap
                CreateOther(p);
        }

        public void Quit(PlayerNode p)
        {
            // If minimap is active
            if (gameObject.activeSelf)
                // Create such player on minimap
                RemoveOther(p);
        }

        public void SyncUpdate(float x, float y)
        {
            if (GameObject.Find("MultiplayerScripts") == null)
            {
                // If NOT in multiplayer mode
                miniMap.RemoveMember(this);
                Destroy(this);
            }
            else
            {
                // If all presented
                if (PlayerNode.present != null &&
                    PlayerNode.present.id != -1 &&
                    PlayerNodes.present != null)
                {
                    // Create / update other players in mini map
                    StartCoroutine(Others(x, y));
                }
            }
        }

        public void BeginNextChunk()
        {
            // Clear all otherPlayers
            miniMap.DestroyFromTag("UiOthers");
        }

        public void Reset()
        {
            // Clear all otherPlayers
            miniMap.DestroyFromTag("UiOthers");
        }
    }
}