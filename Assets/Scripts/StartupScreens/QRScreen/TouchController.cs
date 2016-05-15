using UnityEngine;
using System.Collections;
using Assets.Scripts.InputMethods.Touch;
using UnityEngine.SceneManagement;
using Assets.Scripts.StartupScreens.InitialScreen;
using System;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.QRScreen
{
	public class TouchController : MonoBehaviour
	{
		AfterTitle afterTitle;
		Coroutine longPress, pressTwice;
		[SerializeField]
		float delayBase = 1;
		[SerializeField]
		int longPressWait;
		[SerializeField]
		int pressTwice1stUp;
		[SerializeField]
		int pressTwiceInterval;

		// Default empty action
		Action WhenBoundary;

		// Use this for initialization
		void Start ()
		{
			// Binding
			afterTitle = GameObject.Find ("EventSystem").GetComponent<AfterTitle> ();
		}
	
		// Update is called once per frame
		void Update ()
		{
			// If touched
			if (TouchObject.TouchCount > 0) {
				if (longPress == null)
					longPress = StartCoroutine (LongPressing ());
				if (pressTwice == null)
					pressTwice = StartCoroutine (PressingTwice ());
			}
			if (GetComponent<Blinking> ().isBoundary) {
				if (WhenBoundary != null)
					WhenBoundary ();
			}
		}

		// Handling Long Press event
		IEnumerator LongPressing ()
		{
			bool isSuccess = true;

			for (int i = 0; i < longPressWait; i++) {
				// Event failed in execution
				if (TouchObject.TouchCount == 0) {
					isSuccess = false;
					break;
				}
				yield return new WaitForSeconds (delayBase);
			}

			// Event executed successfully
			if (isSuccess) {
				// Remove saved credential
				PlayerPrefs.DeleteKey ("SavedCred");
				WhenBoundary = () => {
					// Change display text
					GetComponent<Text> ().text = "Credential removed";
					WhenBoundary = () => {
						// Hide self
						gameObject.SetActive (false);
					};
				};
			}

			// Reset event
			yield return new WaitUntil (() => TouchObject.TouchCount == 0);
			longPress = null;
		}

		// Handling Press Twice event
		IEnumerator PressingTwice ()
		{
			bool isSuccess = false;
			float fuc = 0;

			// Wait till first release
			yield return new WaitUntil (() => {
				fuc += delayBase;
				return TouchObject.TouchCount == 0 || fuc >= pressTwice1stUp;
			});

			if (fuc < pressTwice1stUp) {
				for (int i = 0; i < pressTwiceInterval; i++) {
					// Event executed successfully
					if (TouchObject.TouchCount > 0) {
						isSuccess = true;
						break;
					}
					yield return new WaitForSeconds (delayBase);
				}
			}

			// Event executed successfully
			if (isSuccess) {
				afterTitle.BeforeToNextScene ();
				// Move to next Scene
				SceneManager.LoadScene ("InitialScreen");
			}

			// Reset event
			yield return new WaitUntil (() => TouchObject.TouchCount == 0);
			pressTwice = null;
		}
	}
}
