using UnityEngine;
using BaseClient = Assets.Scripts.InMaze.Multiplayer.Normal.ClientController;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using Assets.Scripts.InMaze.Networking.Jsonify;

namespace Assets.Scripts.InMaze.Multiplayer.CaptureTheFlag
{
	public class ClientController : BaseClient
	{
		private bool hasFlag;

		// Exactly the same as normal clientcontroller
		protected override void Update ()
		{
			base.Update ();

			if (FlagHolder.Present != null) {
				// Not you
				if (FlagHolder.Present.Id != PlayerNode.present.id) {
					if (hasFlag)
						MessageBox.Show (this, "You just lost the flag");
					hasFlag = false;
				} else {
					if (!hasFlag)
						MessageBox.Show (this, "You have gotten the flag");
					hasFlag = true;
				}
			}
		}
	}
}