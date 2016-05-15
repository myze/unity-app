
using UnityEngine;

namespace Assets.Scripts.InMaze
{
    public class RotateWithPlayer : MonoBehaviour
    {
        private Transform _camera;

        public void Start()
        {
            // Binding
            _camera = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {
            transform.localEulerAngles = new Vector3(
                0,
                _camera.eulerAngles.y,
                0
            );
        }
    }
}
