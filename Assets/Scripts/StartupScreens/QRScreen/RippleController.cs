using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.InputMethods.Touch;

namespace Assets.Scripts.StartupScreens.QRScreen
{
    public class RippleController : MonoBehaviour
    {
        private GameObject canvas;
        [SerializeField]
        private Sprite rippleSprite;
        [SerializeField]
        private Color[] rippleColors;
        [SerializeField]
        private Vector2 rippleSize;

		public float[] rippleAnimateCycle;
		public float rippleAnimateDelay = 0.01f;

        // Use this for initialization
        void Start()
        {
            // Binding
            canvas = GameObject.Find("Canvas");
        }

        // Update is called once per frame
        void Update()
        {
            if (TouchObject.TouchCount > 0)
            {
                Vector2 touchPos = TouchObject.GetTouch(0).Position;
                Transform ripple;

                if (!GameObject.Find("Ripple0"))
                {
                    // Show Touch Indicator
                    ripple = CreateRipple(0, canvas.transform).transform;
                    // Set top most
					ripple.SetSiblingIndex(ripple.parent.childCount-1);
                }
                else {
                    ripple = GameObject.Find("Ripple0").transform;
                }
                // Set position
                ripple.localPosition = touchPos;
            }
            else {
                // Clear all ripple
                GameObject[] objs = GameObject.FindGameObjectsWithTag("Ripple");
                foreach (GameObject obj in objs)
                    Destroy(obj);
            }
        }

        GameObject CreateRipple(int id, Transform parent)
        {
            GameObject ripple = new GameObject("Ripple" + id)
            {
                tag = "Ripple"
            };
            // Make child of parent
            ripple.transform.SetParent(parent);

            RectTransform rt = ripple.AddComponent<RectTransform>();
            Image img = ripple.AddComponent<Image>();
            img.sprite = rippleSprite;
            img.color = rippleColors[id];
            rt.localScale = Vector3.one;
            rt.sizeDelta = rippleSize;

            // Add rippling behavior
            ripple.AddComponent<Rippling>();

            return ripple;
        }
    }
}