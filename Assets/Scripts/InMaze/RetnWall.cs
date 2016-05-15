using UnityEngine;

namespace Assets.Scripts.InMaze
{
    public class RetnWall{
        public Vector3 localScale;
        public Vector3 position;

        private bool _hasRotation = false;
        public bool hasRotation() { return _hasRotation; }

        private Quaternion _rotation;
        public Quaternion rotation {
            get { return this._rotation; }
            set {
                this._rotation = value;
                _hasRotation = true;
            }
        }
    }
}
