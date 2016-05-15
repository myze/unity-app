using System;
using Assets.Scripts.InMaze.GameElements;
using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI
{
    public class DistanceFromExit : StackableUi
    {
        private Text _text;
        private Transform _player;
        private MazeData.Coordinate _exit;
        private DynamicMaze dynMaze;

        // Use this for initialization
        void Start()
        {
            // Set next node
            LowerNode = GameObject.Find("FlagUI/Distance")
                .GetComponent<DistanceFromFlag>();

            // Binding
            _text = GetComponent<Text>();
            _player = GameObject.Find("Player").transform;
            _exit = MazeData.present.escape;
            dynMaze = GameObject.Find("MapScripts").GetComponent<DynamicMaze>();

			// Self destruction if maze data doesn't contain exit
			if (_exit == null)
			{
				Destroy(transform.parent.gameObject);
			}
        }

        // Update is called once per frame
        void Update()
        {
            // Calculate distance between exit and player
            _text.text = 
                string.Format("{0:F1}", Mathf.Round(
                (Mathf.Sqrt(
                Mathf.Pow(_player.localPosition.x - _exit.X, 2) +
                Mathf.Pow(_player.localPosition.z - _exit.Y, 2)) 
                - 1.125f /* Radius of exit capsule collider + 1/4 of radius of player */) 
                * dynMaze.playerPerWall * 10) / 10);
        }
    }
}
