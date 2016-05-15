using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking
{
	public class InternetConn
	{
		public const string WEBSITE = "https://myze.xyz";
		public const string MAP_LIST = WEBSITE + "/maps";
		public const string MAP = MAP_LIST + "/";

		public string Response { get; private set; }

		public Texture Texture { get; private set; }

		public bool HasResponse {
			get { return Response != null && Response.Length > 0; }
		}

		public bool HasTexture {
			get { return Texture != null; }
		}

		readonly string url;
		int timeout;
		readonly MonoBehaviour mono;
		Coroutine countDownRoute;

		byte[] formData;
		Dictionary<string, string> headers;
		WWW www;

		public delegate void AfterConnectedDelegate ();

		public delegate void AfterConnectedWithRefDelegate (InternetConn conn);

        public static void RequestMapList(string token, MonoBehaviour mono, AfterConnectedWithRefDelegate after)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers.Add("X-Myze-Sec-Token", token);

            InternetConn conn = new InternetConn(InternetConn.MAP_LIST, mono, form.data, headers);
            conn.Connect(after);
        }

        public InternetConn (string url, MonoBehaviour mono, int timeout = 500)
		{
			this.mono = mono;
			this.url = url;
			this.timeout = timeout;
		}

		public InternetConn (string url, MonoBehaviour mono,
		                     byte[] formData, Dictionary<string, string> headers, int timeout = 500)
			: this (url, mono, timeout)
		{
			// Prevent error when creating request
			// POST request with a zero-sized post buffer is not supported
			if (formData.Length != 0)
				this.formData = formData;
			else
				this.formData = new byte[]{ 0 };
			this.headers = headers;
		}

		public void Connect (AfterConnectedWithRefDelegate after)
		{
			mono.StartCoroutine (ConnectEnumerator (after));
			countDownRoute = mono.StartCoroutine (CountDown ());
		}

		public void Connect (AfterConnectedDelegate after)
		{
			Connect ((conn) => {
				after ();
			});
		}

		IEnumerator CountDown ()
		{
			while (timeout > 0) {
				timeout -= 1;
				yield return new WaitForSeconds (0.001f);
			}
			www.Dispose ();
		}

		IEnumerator ConnectEnumerator (AfterConnectedWithRefDelegate after)
		{
			if (formData != null && headers != null)
				www = new WWW (url, formData, headers);
			else
				www = new WWW (url);
			yield return www;

			// Stop counting down
			mono.StopCoroutine (countDownRoute);

			try {
				if (www.error == null) {
					Response = www.text;
					if (www.texture.width != 8 || www.texture.height != 8)
						Texture = www.texture;
				}
			} catch (NullReferenceException) {
				// Exception thrown due to asynchronous www.Dispose() in countDown()
			}

			after (this);
		}
	}
}