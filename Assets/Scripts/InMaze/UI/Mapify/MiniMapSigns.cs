using System.Collections;
using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI.Mapify
{
    public class MiniMapSigns : MonoBehaviour, IMiniMapEvents, IReset
    {
        private MiniMap miniMap;
        private DynamicSigns dynSigns;

        // Use this for initialization
        void Start()
        {
            // Binding
            miniMap = GetComponent<MiniMap>();
            dynSigns = GameObject.Find("MapScripts").GetComponent<DynamicSigns>();
            // Register in MiniMap
            miniMap.InsertMember(this);
        }

        void CreateSign(float x, float y)
        {
            // Add new Sign to minimap
            GameObject sign = new GameObject();
            sign.AddComponent<Image>();
            sign.transform.SetParent(transform);
            // Move to topmost in hierarchy
            sign.transform.SetSiblingIndex(0);
            sign.name = "MiniSign" + x + "/" + y;

            Image img = sign.GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>("SMS Filled-100");
            img.color = Color.yellow;
            img.type = Image.Type.Simple;

            RectTransform rectTran = sign.GetComponent<RectTransform>();
            rectTran.localScale = miniMap.orgScale;
            rectTran.sizeDelta =
                    GameObject.Find("MiniPlayer")
                        .GetComponent<RectTransform>()
                        .sizeDelta;

            miniMap.UpdatePos(
                Vector3.zero,
                new Vector3(x, 0, y),
                sign
            );
            miniMap.ObjectShrink(sign);
        }

        void RemoveSign(float x, float y)
        {
            GameObject sign = GameObject.Find("MiniSign" + x + "/" + y);
            // Remove is available
            if (sign) Destroy(sign);
        }

        IEnumerator CreateRoutine(float x, float y)
        {
            foreach (MazeData.CoordinateDesc sign in dynSigns.signs)
            {
                // If x coordinate matches
                if (sign.X >= x - miniMap.mapRenderSize * 5 &&
                    sign.X <= x + miniMap.mapRenderSize * 5 &&
                    // If y(z) coordinate matches
                    sign.Y >= y - miniMap.mapRenderSize * 5 &&
                    sign.Y <= y + miniMap.mapRenderSize * 5 &&
                    // If NOT exists 
                    !GameObject.Find("MiniSign" + sign.X + "/" + sign.Y))
                {
                    CreateSign(sign.X, sign.Y);
                }

                yield return null;
            }
        }

        IEnumerator RemoveRoutine()
        {
            foreach (MazeData.CoordinateDesc sign in dynSigns.signs)
            {
                RemoveSign(sign.X, sign.Y);
                yield return null;
            }
        }

        public void SyncUpdate(float x, float y)
        {
            StartCoroutine(CreateRoutine(x, y));
        }

        public void BeginNextChunk()
        {
            StartCoroutine(RemoveRoutine());
        }

        public void Reset()
        {
            // Remove all signs on minimap
            StartCoroutine(RemoveRoutine());
        }
    }
}
