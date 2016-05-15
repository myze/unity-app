using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Assets.Scripts.InMaze.UI
{
    public class Popup : MonoBehaviour
    {
        [SerializeField]
        Vector3 shown = new Vector3(0, 0, 0), hidden = new Vector3(0, 0, 0);
        [SerializeField]
        float increment;

        // Start is called once created
        void Start()
        {
            // Set position to hidden
            transform.localPosition = hidden;
            // Disable all children
            StartCoroutine(SetChildActive(false));
            enabled = false;
        }

        IEnumerator SetChildActive(bool enabled)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(enabled);
                yield return null;
            }
        }

        public void trigger()
        {
            enabled = !enabled;
            if (enabled)
                StartCoroutine(transposition(shown, enabled));
            else
                StartCoroutine(transposition(hidden, enabled));
        }

        float synchronize(float from, float to)
        {
            from = Mathf.Round(from * 1000) / 1000;
            to = Mathf.Round(to * 1000) / 1000;

            if (from > to) return from -= increment;
            else if (from < to) return from += increment;
            else return to;
        }

        IEnumerator transposition(Vector3 target, bool enabled)
        {
            // Enable before showing
            if (enabled)
                StartCoroutine(SetChildActive(true));
            while ((transform.localPosition.x != target.x ||
                    transform.localPosition.y != target.y ||
                    transform.localPosition.z != target.z) &&
                   enabled == this.enabled)
            {
                transform.localPosition = new Vector3(
                    synchronize(transform.localPosition.x, target.x),
                    synchronize(transform.localPosition.y, target.y),
                    synchronize(transform.localPosition.z, target.z)
                    );
                yield return null;
            }
            // Disable after hidding
            if (!enabled)
                StartCoroutine(SetChildActive(false));
        }
    }
}
