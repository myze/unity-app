using UnityEngine;
using System.Collections;

namespace Assets.Scripts.InputMethods.Joysticks
{
	public class MFIGenericController : JoystickController
	{
		// MFI Controller Name
		public MFIGenericController () : base ("[extended,wireless] joystick 1 by Generic Controller") { }

		public override KeyCode A {
			get {
				return KeyCode.JoystickButton14;
			}
		}

		public override KeyCode B {
			get {
				return KeyCode.JoystickButton13;
			}
		}

		public override KeyCode X {
			get {
				return KeyCode.JoystickButton15;
			}
		}

		public override KeyCode Y {
			get {
				return KeyCode.JoystickButton12;
			}
		}

		public override KeyCode Up {
			get {
				return KeyCode.JoystickButton4;
			}
		}

		public override KeyCode Down {
			get {
				return KeyCode.JoystickButton6;
			}
		}

		public override KeyCode Left {
			get {
				return KeyCode.JoystickButton7;
			}
		}

		public override KeyCode Right {
			get {
				return KeyCode.JoystickButton5;
			}
		}

		public override KeyCode L2 {
			get {
				return KeyCode.JoystickButton10;
			}
		}

		public override KeyCode R2 {
			get {
				return KeyCode.JoystickButton11;
			}
		}

		public override KeyCode L1 {
			get {
				return KeyCode.JoystickButton8;
			}
		}

		public override KeyCode R1 {
			get {
				return KeyCode.JoystickButton9;
			}
		}

		public override KeyCode Start {
			get {
				return KeyCode.JoystickButton0;
			}
		}
	}
}
