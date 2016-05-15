using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;

namespace Assets.Scripts.InMaze.Multiplayer
{
    public class Costar : MonoBehaviour
    {
        // Id for this Costar
        public int id = -1;
        // Animator for UnityChan
        private Animator anim;

        // Use this for initialization
        void Start()
        {
            anim = GetComponent<Animator>();
            updatePosition();
        }

        public void Update()
        {
            // If spectating
            if (Spectator.Present != null)
            {
                // Disable capsule collider for player penetration
                gameObject
                    .GetComponent<CapsuleCollider>()
                    .enabled = false;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            updatePosition();
        }

        void updatePosition()
        {
            if (PlayerNodes.present.GetPlayer(id) != null)
            {
                // Actual update for position
                // Update position
                transform.localPosition = new Vector3(
                    PlayerNodes.present.GetPlayer(id).position.x,
                    PlayerNodes.present.GetPlayer(id).position.y,
                    PlayerNodes.present.GetPlayer(id).position.z
                    );
                // Update rotation
                transform.localEulerAngles = new Vector3(
                    0,
                    PlayerNodes.present.GetPlayer(id).eulerAngles.y
                    );
                // Trigger jump animation
                anim.SetBool(
                    "Jump",
                    PlayerNodes.present.GetPlayer(id).jump
                    );
                // Trigger move animations
                anim.SetFloat(
                    "Speed",
                    PlayerNodes.present.GetPlayer(id).vertical
                    );
                anim.SetFloat(
                    "Direction",
                    PlayerNodes.present.GetPlayer(id).horizontal
                    );
            }
        }
    }
}
