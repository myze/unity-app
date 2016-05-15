using System;
using System.Collections;
using Assets.Scripts.InMaze.GameElements;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI
{
    public class Counting : StackableUi
    {
        private Text text;
        private TimeRaceScript timeScript;

        [SerializeField]
        private Color warningColor;
        [SerializeField]
        private Color positiveColor;
        private Color defaultColor;

        // Use this for initialization
        void Start()
        {
            UpperNode = GameObject.Find("PlayerNumUI/PlayerNum")
                .GetComponent<PlayerNumber>();

            // If not in time racing gamemode
            if (TransScene.Present.SelectedGameMode != TransScene.GameMode.TimeRace)
                Destroy(transform.parent.gameObject);

            // Binding
            text = GetComponent<Text>();
            timeScript = GameObject.Find("MapScripts")
                .GetComponent<TimeRaceScript>();

            defaultColor = text.color;
        }

        // Update is called once per frame
        void Update()
        {
            // Update time from time script trial time string
            text.text = timeScript.Trial;
            if (text.text.Contains("+"))
                text.color = warningColor;
            else if (!text.text.Contains("-"))
                text.color = positiveColor;
            else
                text.color = defaultColor;
        }
    }
}
