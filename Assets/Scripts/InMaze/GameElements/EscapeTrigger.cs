using Assets.Scripts.InMaze.Controllers;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;

namespace Assets.Scripts.InMaze.GameElements
{
    public class EscapeTrigger : MonoBehaviour
    {
        private DynamicExit dynExit;

        public void Start()
        {
            // Binding
            dynExit = GameObject.Find("MapScripts").GetComponent<DynamicExit>();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (TransScene.Present.SelectedGameMode != TransScene.GameMode.CaptureTheFlag ||
                FlagHolder.Present.Id == PlayerNode.present.id)
            {
                Escape();
            }
            else if (TransScene.Present.SelectedGameMode == TransScene.GameMode.CaptureTheFlag)
            {
                MessageBox.Show(this, "Get the Flag to Escape");
            }
        }

        private void Escape()
        {
            // Once only
            if (!dynExit.isEscaped)
            {
                dynExit.isEscaped = true;

                InputController input = GameObject.Find("MapScripts")
                    .GetComponent<InputController>();
                // Freeze User
                input.ToggleFps();
                // Ignore all input
                input.enabled = false;

                new MessageBox(this, "Escaped!", 50, MessageBox.Y_LOW)
                    .SetFadedEventHandler(() =>
                    {
                        // Toggle menu
                        input.ToggleFps();
                        input.TogglePauseMenu("endgame");
                    })
                    .Show();
            }
        }
    }
}
