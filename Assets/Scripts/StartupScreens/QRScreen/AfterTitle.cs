using Assets.Scripts.StartupScreens.InitialScreen;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.InMaze.Networking;
using System.Collections.Generic;
using System.Collections;

using AfterConnected = Assets.Scripts.InMaze.Networking.InternetConn.AfterConnectedWithRefDelegate;
using Assets.Scripts.InMaze.Networking.Jsonify;

namespace Assets.Scripts.StartupScreens.QRScreen
{
	public class AfterTitle : MonoBehaviour
	{
		private Text text;
		private BarcodeCam cam;
		private string prevResult;
		private bool hasResult;
		private GameObject credHint;

		// Start is called just before any of the Update methods is called the first time
		public void Start ()
		{
			// Set LoadSaved disable for future enabling
			credHint = GameObject.Find ("CredHint");
			credHint.SetActive (false);

			text = GameObject.Find ("Text").GetComponent<Text> ();
			cam = GameObject
                .Find ("QRCamera")
                .GetComponent<BarcodeCam> ();
		}

		// Update is called once per frame
		void Update ()
		{
			if (text
                .GetComponent<Blinking> ()
                .isBoundary) {
				if (Essentials.HasInternetConnection ()) {
					if (this.hasResult) {
						// Move to next scene
						SceneManager.LoadScene ("InitialScreen");
					} else {
						// If no valid token discovered
						if (!cam.enabled) {
							// Enable QRCamera
							cam.enabled = true;
							StartCoroutine (LoadAndVerifySavedCred ());
						}

						if (!cam.hasResult) {
							// Change Text
							text.text = "Scan QR code from myze.xyz to login";
						} else {
							// If loaded QR successfully
							if (cam.LastResult != prevResult) {
								text.text = "Verifying QR code";
								// Verify if token is valid or not
								VerifyCameraToken (cam.LastResult);
								// Update previous result record
								prevResult = cam.LastResult;
							}
						}
					}
				} else {
					// No internet connection
					text.text = "No internet connection found";
				}
			}
		}

		IEnumerator LoadAndVerifySavedCred ()
		{
			// Try to load saved credential if any
			string savedCred = PlayerPrefs.GetString ("SavedCred");

			if (savedCred != null && savedCred.Length > 0) {
				// Construct TokenKey with encrypted token
				TransScene.Present.LoginToken = new TokenKey (savedCred, true);
				yield return null;

				InternetConn.RequestMapList (
					TransScene.Present.LoginToken.ToString (), 
                    this,
					(conn) => {
						if (conn.HasResponse) {
							// Show hint
							credHint.SetActive (true);
							// Store response
							new MazeList (conn.Response);
						}
					});
			}
		}

		public void BeforeToNextScene ()
		{
			// Save login credential
			PlayerPrefs.SetString (
				"SavedCred", 
				TransScene.Present.LoginToken.CipherToken
			);
		}

		void VerifyCameraToken (string token)
		{
			InternetConn.RequestMapList (token, this, (conn) => {
				if (conn.HasResponse) {
					hasResult = true;
					// Assign token key
					TransScene.Present.LoginToken = new TokenKey (token);
					// Store response
					new MazeList (conn.Response);

					BeforeToNextScene ();
				} else {
					text.text = "Invalid QR code scanned";
				}
			});
		}
	}
}
