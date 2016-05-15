using SimpleJSON;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Assets.Scripts.InMaze.Networking.Jsonify
{
    // Dummy class for spectator indication
    public class Spectator : IJsonify
    {
        public static Spectator Present;
        private readonly string _ip;

        public Spectator(string ip)
        {
            _ip = ip;
        }

        public JSONClass ToJson()
        {
            return new JSONClass()
            {
                {"Spectator", new JSONData("null")},
                {"IP", new JSONData(_ip)}
            };
        }

        public override string ToString()
        {
            return ToJson().ToString();
        }

        // Return if successfully spectate
        public static bool Spectate()
        {
            // Binding
            FirstPersonController fpc = GameObject.Find("Player")
                .GetComponent<FirstPersonController>();
            CapsuleCollider collider = fpc.gameObject
                .GetComponent<CapsuleCollider>();

            if (collider != null)
            {
                // Enter spectator
                fpc.m_WalkSpeed = 20f;
                fpc.m_RunSpeed = 20f;
                fpc.m_JumpSpeed = 0;
                fpc.m_UseFovKick = false;
                fpc.m_UseHeadBob = false;

                return true;
            }
            return false;
        }
    }
}
