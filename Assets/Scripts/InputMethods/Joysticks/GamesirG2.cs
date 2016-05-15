using System;
using UnityEngine;

namespace Assets.Scripts.InputMethods.Joysticks
{
	public class GamesirG2 : JoystickController
	{
		public GamesirG2 () : base ("Gamesir-G2")
		{
		}

		public GamesirG2 (string name) : base (name)
		{
			// For inheritance
		}

		#region implemented abstract members of JoystickController

		public override KeyCode A {
			get {
				return KeyCode.JoystickButton0;
			}
		}

		public override KeyCode B {
			get {
				return KeyCode.JoystickButton1;
			}
		}

		public override KeyCode X {
			get {
				return KeyCode.JoystickButton2;
			}
		}

		public override KeyCode Y {
			get {
				return KeyCode.JoystickButton3;
			}
		}

		public override KeyCode Up {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode Down {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode Left {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode Right {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode L2 {
			get {
				return KeyCode.JoystickButton4;
			}
		}

		public override KeyCode R2 {
			get {
				return KeyCode.JoystickButton5;
			}
		}

		public override KeyCode L1 {
			get {
				return KeyCode.JoystickButton6;
			}
		}

		public override KeyCode R1 {
			get {
				return KeyCode.JoystickButton7;
			}
		}

		public override KeyCode Start {
			get {
				return KeyCode.JoystickButton10;
			}
		}

		#endregion
	}
}

