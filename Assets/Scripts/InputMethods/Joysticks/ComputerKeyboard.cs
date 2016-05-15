using System;
using UnityEngine;

namespace Assets.Scripts.InputMethods.Joysticks
{
	public class ComputerKeyboard : JoystickController
	{
		public ComputerKeyboard () : base ("Keyboard") {}

		#region IButtons implementation

		public override KeyCode A {
			get {
				return KeyCode.Space;
			}
		}

		public override KeyCode B {
			get {
				return KeyCode.Escape;
			}
		}

		public override KeyCode X {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode Y {
			get {
				throw new NotImplementedException ();
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
				return KeyCode.LeftShift;
			}
		}

		public override KeyCode R2 {
			get {
				return KeyCode.M;
			}
		}

		public override KeyCode L1 {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode R1 {
			get {
				throw new NotImplementedException ();
			}
		}

		public override KeyCode Start {
			get {
				return KeyCode.Equals;
			}
		}

		#endregion
	}
}

