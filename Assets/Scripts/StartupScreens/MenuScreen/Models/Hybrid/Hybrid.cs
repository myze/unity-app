using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Hybrid
{
    public class Hybrid : IHybridation
    {
        private readonly GameObject _originator;
        private GameObject _clone;

        public Hybrid(GameObject hostGameObject)
        {
            _originator = hostGameObject;
        }

        // Actualize will be used for updating binded gameobject
        // Or
        // Create a gameobject refering _originator
        public virtual GameObject Actualize()
        {
            // Clone if null
            if (_clone == null)
                _clone = Object.Instantiate(_originator);
            return _clone;
        }
    }
}
