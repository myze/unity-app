using UnityEngine;
using System.Collections;

namespace Assets.Scripts.InputMethods.Touch
{
	public class TouchObject
	{
		private const float POS_Z = 602.5f;

		public Vector2 Position;

		public static int TouchCount {
			get {
#if UNITY_ANDROID || UNITY_IOS
				return Input.touchCount;
#else
				// Is mouse left button clicked
				return Input.GetKey (KeyCode.Mouse0) ? 1 : 0;
#endif
			}
		}

		public static TouchObject GetTouch (int index)
		{

#if UNITY_ANDROID || UNITY_IOS
			Vector3 pos = new Vector3 (
				                   Input.GetTouch (index).position.x,
				                   Input.GetTouch (index).position.y,
				                   POS_Z
			                   );
#else
			Vector3 pos = new Vector3 (
				              Input.mousePosition.x,
				              Input.mousePosition.y,
				              POS_Z
			              );
#endif
			// Get Touch info
			return new TouchObject () { 
				Position = Camera.main.ScreenToWorldPoint (pos)
			};
		}
	}
}