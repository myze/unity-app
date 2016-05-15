using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.UI
{
    public class UIClock : MonoBehaviour
    {
        Text text;

        // Use this for initialization
        void Start()
        {
            text = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            text.text = string.Format("{0:hh:mm tt}", DateTime.Now);
        }
    }
}
