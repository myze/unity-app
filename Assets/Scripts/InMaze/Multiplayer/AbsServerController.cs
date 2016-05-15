using System.Threading;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.UDP;
using UnityEngine;

namespace Assets.Scripts.InMaze.Multiplayer
{
    public abstract class AbsServerController : MonoBehaviour
    {
        // Should be instantiated for this class
        public static bool IsPresent;
        protected UDP Server, Spec;

        // Use this for initialization
        protected virtual void Start()
        {
            Server = new UServer()
                .SetOnPlayerUpdatedEventHandler(PlayerUpdate);
            Spec = new USpectator(USpectator.DEFAULT_URL);

            new Thread(() => { Server.start(); }).Start();
            new Thread(() => { Spec.start(); }).Start();
        }

        // Leave scene
        protected virtual void OnDestroy()
        {
            if (Server != null) Server.stop();
            if (Spec != null) Spec.stop();
            IsPresent = false;
            StopAllCoroutines();
        }

        // When player finished updating in UServer
        protected abstract void PlayerUpdate(PlayerNode player);
    }
}
