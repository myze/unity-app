using System.Threading;
using Assets.Scripts.InMaze.Controllers;
using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using Assets.Scripts.InMaze.Networking.UDP;
using Assets.Scripts.InMaze.UI.Mapify;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.InMaze.Multiplayer
{
    public abstract class AbsClientController : MonoBehaviour
    {
        // Should be instantiated or not for this class
        public static bool IsPresent;

        protected UDP Client;
        protected PlayerNodes Previous;
        protected MiniMapOthers Others;
        protected CostarsController CostarController;

        // Correction value for escalated Player
        private float yCorrection = 1f;

        protected virtual void Awake()
        {
            // Bindings
            Others = GameObject
                .Find("SliderHUD/MapUI")
                .GetComponent<MiniMapOthers>();
            CostarController = GameObject
                .Find("MultiplayerScripts")
                .GetComponent<CostarsController>();
        }

        // Use this for initialization
        protected virtual void Start()
        {
            // Use broadcast address as Anonymous targeting server
            Client = new UClient(Essentials.GetBroadcastAddress(
                Essentials.GetCurrentIP(),
                Essentials.GetSubnetMask()
                ).ToString());

            new Thread(() => { Client.start(); }).Start();

            UpdatePlayer();
        }

        protected virtual void ClientUpdate()
        {
            // Invoke all Join()/Quit()
            if (PlayerNodes.present != null)
                // If player quitted
                if (Previous != null && Previous.Count > PlayerNodes.present.Count)
                {
                    // Find which player left
                    foreach (PlayerNode p in Previous.GetPlayers())
                        if (PlayerNodes.present.GetPlayer(p.id) == null)
                        {
                            Others.Quit(p);
                            CostarController.Quit(p);
                            break;
                        }
                }
                // If player joined
                else if (Previous == null || Previous.Count < PlayerNodes.present.Count)
                {
                    // Find which player joined
                    foreach (PlayerNode p in PlayerNodes.present.GetPlayers())
                        if (Previous == null || Previous.GetPlayer(p.id) == null)
                            // if not my id
                            if (p.id != PlayerNode.present.id)
                            {
                                Others.Join(p);
                                CostarController.Join(p);
                                break;
                            }
                }

            // Check if statusCode encountered in client
            switch (Client.statusCode)
            {
                case StatusCode.NOT_FOUND:
                    new MessageBox(
                        this,
                        "Server connection error",
                        MessageBox.DURATION_SHORT,
                        MessageBox.Y_LOW)
                        .ShowOnlyAvailable();
                    break;
                case StatusCode.SERVER_CLOSED:
                    new MessageBox(
                        this,
                        "Server closed",
                        MessageBox.DURATION_LONG,
                        MessageBox.Y_LOW)
                        .SetFadedEventHandler(() =>
                        {
                            SceneManager.LoadScene("MenuScreen");
                        })
                        .Show();
                    break;
            }

            // Update previous PlayerNodes
            Previous = PlayerNodes.present;
        }

        // Update current player
        protected virtual PlayerNode UpdatePlayer()
        {
            GameObject player = GameObject.Find("Player");

            if (PlayerNode.present == null)
                PlayerNode.present = new PlayerNode();
            PlayerNode pl = PlayerNode.present;

            pl.id = (PlayerNode.present != null) ? PlayerNode.present.id : -1;
            // Y-axis 0 for fixing character floating
            pl.position = new Vector3(
                player.transform.localPosition.x,
                player.transform.localPosition.y - yCorrection,
                player.transform.localPosition.z
            );
            pl.eulerAngles = Camera.main.transform.eulerAngles;
            // Check if paused
            InputController ic = GameObject.Find("MapScripts").GetComponent<InputController>();
            if (ic != null && !ic.IsPaused)
            {
                pl.jump = Input.GetKeyDown(TransScene.Present.Controller.A);
                pl.horizontal = TransScene.Present.Controller.GetAxis("Horizontal");
                pl.vertical = TransScene.Present.Controller.GetAxis("Vertical");
            }

            return pl;
        }

        // Leave scene
        protected virtual void OnDestroy()
        {
            if (Client != null) Client.stop();
            IsPresent = false;
            StopAllCoroutines();
            MessageBox.Clear();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            // Base update cycle
            ClientUpdate();
            UpdatePlayer();
        }
    }
}
