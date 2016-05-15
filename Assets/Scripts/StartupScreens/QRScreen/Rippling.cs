using UnityEngine;
using System.Collections;

namespace Assets.Scripts.StartupScreens.QRScreen
{
	public class Rippling : MonoBehaviour
	{
		private RippleController controller;
		private RectTransform rect;
		[SerializeField]
		private bool stationary = true;
		[SerializeField]
		private int rippleCycleIndex;

		// Use this for initialization
		void Start ()
		{
			// Binding
			controller = GameObject.Find ("EventSystem")
                .GetComponent<RippleController> ();
			rect = GetComponent<RectTransform> ();
		}

		IEnumerator Scaling (float boundaryPoint)
		{
			stationary = false;
			// Assume Ripple is in right shape (e.g. width = height)
			if (boundaryPoint > rect.sizeDelta.x)
				// Increasing width
				for (float f = rect.sizeDelta.x; f < boundaryPoint; f++) {
					rect.sizeDelta = new Vector2 (f, f);
					// Animate for 0.1 second
					yield return new WaitForSeconds (controller.rippleAnimateDelay);
				}
			else
				// Decreasing width
				for (float f = rect.sizeDelta.x; f > boundaryPoint; f--) {
					rect.sizeDelta = new Vector2 (f, f);
					// Animate for 0.1 second
					yield return new WaitForSeconds (controller.rippleAnimateDelay);
				}
			stationary = true;
			rippleCycleIndex += 1;
			rippleCycleIndex %= controller.rippleAnimateCycle.Length;
		}

		// Update is called once per frame
		void Update ()
		{
			// If stationary point is met
			if (stationary) {
				StartCoroutine (Scaling (controller.rippleAnimateCycle [rippleCycleIndex]));
			}
		}
	}
}
