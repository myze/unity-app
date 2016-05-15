using System;
using System.Collections;
using Assets.Scripts.InMaze.Multiplayer;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;

namespace Assets.Scripts.InMaze.GameElements
{
    public class TimeRaceScript : MonoBehaviour
    {
        public string Trial
        {
            get { return FormatTimeString(); }
        }

        [SerializeField]
        private int bestTime;
        [SerializeField]
        private int trialTime;
        [SerializeField]
        private int UnitInMs = 100;

        private DynamicExit dynExit;

        // Use this for initialization
        void Start()
        {
            // Binding
            dynExit = GameObject.Find("MapScripts").GetComponent<DynamicExit>();

            // If not in time racing gamemode
            if (TransScene.Present.SelectedGameMode != TransScene.GameMode.TimeRace)
                Destroy(this);
            else
                StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            // If server provides escape time
            if (AbsClientController.IsPresent)
            {
                // Wait till server provides finish time
                yield return new WaitUntil(() => ServerEscTime.Present != null);
                bestTime = ServerEscTime.Present.BestTime;
            }
            // If such map has best time saved beforehand
            else if (PlayerPrefs.HasKey(TransScene.Present.MapId))
                bestTime = PlayerPrefs.GetInt(TransScene.Present.MapId);

            // Start ticking every second
            StartCoroutine(Tick());
        }

        IEnumerator Tick()
        {
            while (!dynExit.isEscaped)
            {
                trialTime++;
                yield return new WaitForSeconds(0.1f);
            }
            // Escaped
            if ((trialTime < bestTime || bestTime == 0) && ServerEscTime.Present == null)
                // Save trial time as best time
                PlayerPrefs.SetInt(TransScene.Present.MapId, trialTime);
        }

        string FormatTimeString()
        {
            char c = ' ';
            if (bestTime != 0)
                // Faster than saved best time -> '-'
                // Slower than saved best time -> '+'
                c = (bestTime > trialTime) ? '-' : '+';
            TimeSpan t = TimeSpan.FromMilliseconds(Mathf.Abs(bestTime - trialTime) * UnitInMs);
            string tStr = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds / UnitInMs);
            return c + tStr;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
