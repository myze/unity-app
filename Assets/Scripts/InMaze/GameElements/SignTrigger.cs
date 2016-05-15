using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InMaze.GameElements
{
    public class SignTrigger : MonoBehaviour
    {
        private DynamicSigns dynSigns;

        // Start is called just before any of the Update methods is called the first time
        public void Start()
        {
            // Binding
            dynSigns = GameObject.Find("MapScripts").GetComponent<DynamicSigns>();
        }

        // OnTriggerEnter is called when the Collider other enters the trigger
        public void OnTriggerEnter(Collider other)
        {
            // Fade in
            StartCoroutine(Fade(true));
        }

        public void OnTriggerExit(Collider other)
        {
            // Fade out
            StartCoroutine(Fade(false));
        }

        IEnumerator Fade(bool isIn)
        {
            Text text = transform.Find("Canvas/Text").GetComponent<Text>();
            text.color = new Color(
                text.color.r,
                text.color.g,
                text.color.b,
                isIn ? 0 : 1
            );
            while (text.color.a <= 1 && text.color.a >= 0)
            {
                text.color = new Color(
                    text.color.r,
                    text.color.g,
                    text.color.b,
                    text.color.a + 0.1f * (isIn ? 1 : -1));
                yield return new WaitForSeconds(dynSigns.FadeInSpeed);
            }
        }
    }
}
