using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.StartupScreens.MenuScreen;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI.Mapify
{
    public class MiniMap : MonoBehaviour, IReset
    {
        [SerializeField]
        private float shrunkenRate;
        [SerializeField]
        private float mapX, mapY;

        private readonly LinkedList<IMiniMapEvents> miniMapScripts =
            new LinkedList<IMiniMapEvents>();
        private GameObject miniPlayer;
        private float canvasWidth, canvasHeight;
        private bool shouldReset;

        [NonSerialized]
        public Vector3 orgScale;
        // RenderSize per 10 units
        [Range(1f, 7f)]
        public float mapRenderSize;

        public void ObjectShrink(GameObject player = null)
        {
            // Miniplayer is processed by default
            if (player == null) player = miniPlayer;

            // Shrink Mini Player in relation to mapRenderSize
            player.transform.localScale = new Vector3(
                player.transform.localScale.x / mapRenderSize,
                player.transform.localScale.y / mapRenderSize,
                0
            );
        }

        public void UpdatePos(Vector3 eulerAngles, Vector3 position, GameObject player = null)
        {
            // Miniplayer is processed by default
            if (player == null) player = miniPlayer;

            player.transform.localRotation = Quaternion.Euler(
                0,
                0,
                360 - eulerAngles.y
            );

            player.transform.localPosition = new Vector3(
                position.x / mapRenderSize * shrunkenRate
                - mapX / mapRenderSize * shrunkenRate,
                position.z / mapRenderSize * shrunkenRate
                - mapY / mapRenderSize * shrunkenRate,
                -0.002f
            );
        }

        public void DestroyFromTag(string tag)
        {
            // Remove all objects with certain tag
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < gameObjects.Length; i++)
                Destroy(gameObjects[i]);
        }

        // For self adding in miniMapScripts externally
        public void InsertMember(IMiniMapEvents ime)
        {
            miniMapScripts.AddLast(ime);
        }

        // For self removal in miniMapScripts externally
        public void RemoveMember(IMiniMapEvents ime)
        {
            miniMapScripts.Remove(ime);
        }

        // Use this for initialization
        void Start()
        {
            // Load from TransScene
            if (TransScene.Present.MiniMapRenderSize > 0)
                mapRenderSize = TransScene.Present.MiniMapRenderSize;

            // Binding
            miniPlayer = GameObject.Find("MiniPlayer");

            // Backup rectTransform of MiniPlayer
            orgScale = miniPlayer.GetComponent<RectTransform>().localScale;

            RectTransform transform = GameObject.Find("SliderHUD")
                .GetComponent<RectTransform>();
            canvasHeight = transform.sizeDelta.x;
            canvasWidth = transform.sizeDelta.y;

            SubStart();
        }

        // Extracted from Start() for invoking in Reset()
        void SubStart()
        {
            GameObject player = GameObject.Find("Player");

            // Shrink Mini Player in relation to mapRenderSize
            ObjectShrink();

            UpdatePos(
                Camera.main.transform.eulerAngles,
                player.transform.position
            );

            StartCoroutine(ui(
                player.transform.position.x,
                player.transform.position.z
            ));
        }

        // Update is called once per frame
        void Update()
        {
            float x = mapX, y = mapY;

            // Update MiniMap
            if (canvasHeight / -2 >= miniPlayer.transform.localPosition.x)
            {
                x = x - mapRenderSize * 10;
            }
            else if (canvasHeight / 2 <= miniPlayer.transform.localPosition.x)
            {
                x = x + mapRenderSize * 10;
            }

            if (canvasWidth / -2 >= miniPlayer.transform.localPosition.y)
            {
                y = y - mapRenderSize * 10;
            }
            else if (canvasWidth / 2 <= miniPlayer.transform.localPosition.y)
            {
                y = y + mapRenderSize * 10;
            }

            // Check if moved to another plate
            if (!(x == mapX && y == mapY))
            {
                StopAllCoroutines();
                StartCoroutine(ui(x, y));

                // When minimap renders a new chunk
                foreach (IMiniMapEvents ime in miniMapScripts.ToArray())
                    ime.BeginNextChunk();
            }

            // Execute all sub-updates
            foreach (IMiniMapEvents ime in miniMapScripts.ToArray())
                ime.SyncUpdate(x, y);

            UpdatePos(
                Camera.main.transform.eulerAngles,
                GameObject.Find("Player").transform.position
            );

            // Reset if necessery
            if (shouldReset)
                Reset();
        }

        IEnumerator ui(float x, float y)
        {
            // Get wallTexture extra spreading factor from DynamicMaze.cs
            float extraSpread = GameObject.Find("MapScripts").GetComponent<DynamicMaze>().wallExtraSpread / 200f;

            // Remove all UiWalls
            DestroyFromTag("UiWall");

            ArrayList al = DynamicMaze.NineGrid(x, y, mapRenderSize, MazeData.present.coordinates);

            // Update MapXY
            mapX = x;
            mapY = y;

            for (int i = 0; i < al.Count; i++)
            {
                GameObject image = new GameObject();
                image.AddComponent<RectTransform>();
                image.AddComponent<Image>();
                image.transform.SetParent(gameObject.transform);

                image.name = "UiWall" + x + "/" + y + "/" + i;
                image.tag = "UiWall";
                image.GetComponent<Image>().color = Color.white;
                image.GetComponent<Image>().sprite = Resources.Load<Sprite>("Square");
                image.GetComponent<Image>().type = Image.Type.Tiled;

                image.GetComponent<RectTransform>().localScale = new Vector3(
                    extraSpread +
                    ((((RetnWall)al.ToArray()[i]).localScale.x == 0.1f) ? 0.003f : ((RetnWall)al.ToArray()[i]).localScale.x / mapRenderSize * shrunkenRate),
                    extraSpread +
                    ((((RetnWall)al.ToArray()[i]).localScale.z == 0.1f) ? 0.003f : ((RetnWall)al.ToArray()[i]).localScale.z / mapRenderSize * shrunkenRate),
                    1
                    );
                image.GetComponent<RectTransform>().localPosition = new Vector3(
                    ((RetnWall)al.ToArray()[i]).position.x / mapRenderSize * shrunkenRate
                    - x / mapRenderSize * shrunkenRate,
                    ((RetnWall)al.ToArray()[i]).position.z / mapRenderSize * shrunkenRate
                    - y / mapRenderSize * shrunkenRate,
                    0
                    );

                if (((RetnWall)al.ToArray()[i]).hasRotation())
                {
                    image.GetComponent<RectTransform>().localRotation = Quaternion.Euler(
                        0,
                        0,
                        360 - ((RetnWall)al.ToArray()[i]).rotation.eulerAngles.y
                        );
                }
                else
                {
                    image.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
                }
                image.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

                // Destroy object if one point in grid only
                if (((RetnWall)al.ToArray()[i]).localScale.x != 0.1f &&
                    ((RetnWall)al.ToArray()[i]).localScale.z != 0.1f)
                {
                    Destroy(image);
                }

                yield return null;
            }
        }

        public void Reset()
        {
            // Reset if active only
            if (gameObject.activeSelf)
            {
                shouldReset = false;

                // Reset all IMiniMapEvents members
                foreach (IMiniMapEvents member in miniMapScripts.ToArray())
                    // If reset should be required
                    if (member is IReset)
                        ((IReset) member).Reset();

                // Reset miniPlayer size
                miniPlayer.transform.localScale = orgScale;

                SubStart();
            }
            else
            {
                // Wait till active to invoke Reset()
                shouldReset = true;
            }
        }
    }
}
