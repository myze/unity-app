using System;

namespace Assets.Scripts.InputMethods.Joysticks
{
	public class NimbusForAndroid : GamesirG2
	{
		public NimbusForAndroid () : base ("Nimbus")
		{
			// Invert all axis
			this.InvertedAxis.Add ("Vertical");
			this.InvertedAxis.Add ("Mouse Y");
		}

		public override UnityEngine.KeyCode Start {
			get {
				return UnityEngine.KeyCode.JoystickButton14;
			}
		}
	}
}

