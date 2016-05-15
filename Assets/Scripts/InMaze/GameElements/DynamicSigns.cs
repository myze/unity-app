using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.GameElements
{
    public class DynamicSigns : MonoBehaviour
    {
        private bool isUpdating;
        private DynamicMaze dynMaze;

        public MazeData.CoordinateDesc[] signs;
        public Color MessageColor = Essentials.ParseColor("FFFFFF00");
        public float FadeInSpeed = 0.2f;

        // Use this for initialization
        void Start()
        {
            // Binding
            dynMaze = GameObject.Find("MapScripts").GetComponent<DynamicMaze>();
            signs = MazeData.present.signs;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isUpdating)
                StartCoroutine(UpdateRoutine());
        }

        IEnumerator UpdateRoutine()
        {
            isUpdating = true;
            foreach (MazeData.CoordinateDesc sign in signs)
            {
                // If x coordinate matches
                if (sign.X >= dynMaze.centerPosition.x - dynMaze.renderSize * 15 &&
                    sign.X <= dynMaze.centerPosition.x + dynMaze.renderSize * 15 &&
                    // If z(y) coordinate matches
                    sign.Y >= dynMaze.centerPosition.z - dynMaze.renderSize * 15 &&
                    sign.Y <= dynMaze.centerPosition.z + dynMaze.renderSize * 15)
                {
                    StartCoroutine(CreateSign(sign));
                }
                else
                {
                    StartCoroutine(RemoveSign(sign));
                }
                yield return null;
            }
            isUpdating = false;
        }

        IEnumerator CreateSign(MazeData.CoordinateDesc coord)
        {
            if (dynMaze.isMapReady &&
                // If NOT exists
                !GameObject.Find("Sign" + coord.X + "/" + coord.Y))
            {
                GameObject sign = Instantiate(
                    Resources.Load<GameObject>("prefab/signPin"));
                sign.name = "Sign" + coord.X + "/" + coord.Y;
                sign.transform.localPosition = new Vector3(
                    coord.X,
                    Resources.Load<GameObject>("prefab/signPin")
                        .transform.localPosition.y,
                    coord.Y
                );

                // Initializing text element
                GameObject textGameObject = sign.transform.Find("Canvas/Text")
                    .gameObject;
                Text text = textGameObject.GetComponent<Text>();
                // Setup message content
                text.text = coord.Description;
                RectTransform rectT = textGameObject
                    .GetComponent<RectTransform>();
                // Setup width
                rectT.sizeDelta = new Vector2(
                    Essentials.GetStringWidth(
                    coord.Description,
                    text.fontSize,
                    text.font) + 5,
                    rectT.sizeDelta.y
                );
                // Make transparent for appearing effect
                text.color = MessageColor;

                yield return null;
            }
        }

        IEnumerator RemoveSign(MazeData.CoordinateDesc coord)
        {
            GameObject sign = GameObject.Find("Sign" + coord.X + "/" + coord.Y);
            if (sign)
            {
                Destroy(sign);
                yield return null;
            }
        }
    }
}
