using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI.Mapify
{
    public class MiniMapEscape : MonoBehaviour, IMiniMapEvents, IReset
    {
        private MiniMap miniMap;
        private GameObject escape;
        private DynamicExit dynExit;

        // Use this for initialization
        void Start()
        {
            if (MazeData.present.escape != null)
            {
                // Binding
                miniMap = GetComponent<MiniMap>();
                dynExit = GameObject.Find("MapScripts").GetComponent<DynamicExit>();
                // Register in MiniMap
                miniMap.InsertMember(this);
            }
            else
            {
                // No escape point
                Destroy(this);
            }
        }

        void CreateEscape()
        {
            if (escape == null)
            {
                // Add new Escape to minimap
                escape = new GameObject();
                escape.AddComponent<Image>();
                escape.transform.SetParent(gameObject.transform);
                // Move to topmost in hierarchy
                escape.transform.SetSiblingIndex(0);
                escape.name = "EscapeSign";

                Image img = escape.GetComponent<Image>();
                img.sprite = Resources.Load<Sprite>("Exit Sign-100");
                img.color = Color.green;
                img.type = Image.Type.Simple;

                RectTransform rectTran = escape.GetComponent<RectTransform>();
                rectTran.localScale = miniMap.orgScale;
                rectTran.sizeDelta =
                    GameObject.Find("MiniPlayer")
                        .GetComponent<RectTransform>()
                        .sizeDelta;

                miniMap.UpdatePos(Vector3.zero, dynExit.escapeCoord, escape);
                miniMap.ObjectShrink(escape);
            }
        }

        void RemoveEscape()
        {
            // If gameObject is available
            if (escape != null)
                // Remove
                Destroy(escape);
        }

        public void SyncUpdate(float x, float y)
        {
            // If x coordinate matches
            if (dynExit.escapeCoord.x >= x - miniMap.mapRenderSize * 5 &&
                dynExit.escapeCoord.x <= x + miniMap.mapRenderSize * 5 &&
                // If y(z) coordinate matches
                dynExit.escapeCoord.z >= y - miniMap.mapRenderSize * 5 &&
                dynExit.escapeCoord.z <= y + miniMap.mapRenderSize * 5)
            {
                CreateEscape();
            }
        }

        public void BeginNextChunk()
        {
            RemoveEscape();
        }

        public void Reset()
        {
            RemoveEscape();
        }
    }
}
