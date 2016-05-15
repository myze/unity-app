using UnityEngine;

namespace Assets.Scripts.InMaze
{
    public class MoveWithPlayer : MonoBehaviour {

        // Use this for initialization
        void Start () {
	
        }
	
        // Update is called once per frame
        void Update () {
            transform.localPosition = GameObject.Find("Player").transform.position;
        }
    }
}
