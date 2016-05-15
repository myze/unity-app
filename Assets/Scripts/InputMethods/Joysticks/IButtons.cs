using System;
using UnityEngine;

namespace Assets.Scripts.InputMethods.Joysticks
{
	public interface IButtons
	{
		KeyCode A { get; }
		KeyCode B { get; }
		KeyCode X { get; }
		KeyCode Y { get; }

		KeyCode Up { get; }
		KeyCode Down { get; }
		KeyCode Left { get; }
		KeyCode Right { get; }

		KeyCode L2 { get; }
		KeyCode R2 { get; }
		KeyCode L1 { get; }
		KeyCode R1 { get; }

		KeyCode Start { get; }
	}
}

