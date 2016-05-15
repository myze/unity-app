using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Assets.Scripts.InputMethods.Joysticks
{
	public abstract class JoystickController : IButtons
	{
		#region IButtons implementation

		public abstract KeyCode A {
			get;
		}

		public abstract KeyCode B {
			get;
		}

		public abstract KeyCode X {
			get;
		}

		public abstract KeyCode Y {
			get;
		}

		public abstract KeyCode Up {
			get;
		}

		public abstract KeyCode Down {
			get;
		}

		public abstract KeyCode Left {
			get;
		}

		public abstract KeyCode Right {
			get;
		}

		public abstract KeyCode L2 {
			get;
		}

		public abstract KeyCode R2 {
			get;
		}

		public abstract KeyCode L1 {
			get;
		}

		public abstract KeyCode R1 {
			get;
		}

		public abstract KeyCode Start {
			get;
		}

		#endregion

		// Controller list for finding match joystick
		private static ArrayList _controllers = 
			new ArrayList (new JoystickController[] {
				// All joystick layouts should be instantiated here
				new ComputerKeyboard (),
				new MFIGenericController (),
				new GamesirG2 (),
				new GamesirG2u (),
				new NimbusForAndroid ()
			});

		public static JoystickController GetJoystick (string name)
		{
			foreach (JoystickController c in _controllers.ToArray()) {
				if (c.Name == name)
					return c;
			}
			return null;
		}

		public static JoystickController GetJoystick (Type type)
		{
			foreach (JoystickController c in _controllers.ToArray()) {
				if (c.GetType() == type)
					return c;
			}
			return null;
		}

		public static JoystickController Default { 
			get { return GetJoystick (typeof(GamesirG2)); } 
		}

		// For joystick axis inversion
		protected ArrayList InvertedAxis;

		public string Name { private set; get; }

		public JoystickController (string name)
		{
			this.Name = name;
			InvertedAxis = new ArrayList ();
		}

		// Get AxisRaw with inversion
		public float GetAxisRaw (string name)
		{
			return CrossPlatformInputManager.GetAxisRaw (name) * ((InvertedAxis.Contains (name)) ? -1 : 1);
		}

		// Get Axis with inversion
		public float GetAxis (string name)
		{
			return CrossPlatformInputManager.GetAxis (name) * ((InvertedAxis.Contains (name)) ? -1 : 1);
		}
	}
}

