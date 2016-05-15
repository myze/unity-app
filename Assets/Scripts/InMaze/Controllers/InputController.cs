using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.UI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

namespace Assets.Scripts.InMaze.Controllers
{
    public class InputController : MonoBehaviour
    {
        public bool IsPaused { private set; get; }

        private Popup _popup;
        private MenuControllerAdapter _menuCon;
        private bool _isPauseToggling;

        // Use this for initialization
        void Start()
        {
            // Binding
            _popup = GameObject.Find("SliderHUD").GetComponent<Popup>();
            _menuCon = GameObject.Find("MenuPanel").GetComponent<MenuControllerAdapter>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isPauseToggling)
            {
                if (!IsPaused)
                {
                    // Show map trigger
					if (Input.GetKeyDown(TransScene.Present.Controller.R2))
                        _popup.trigger();
                }
                //Pause Menu
				if (Input.GetKeyDown(TransScene.Present.Controller.Start))
                    TogglePauseMenu(Spectator.Present != null ? "endgame" : "pause");
            }
        }

        // Disable/Enable First Person Controller
        public void ToggleFps()
        {
            FirstPersonController fpc = GameObject.Find("Player")
                .GetComponent<FirstPersonController>();
            fpc.enabled = !fpc.enabled;
        }

        // Available for external pausing
        public void TogglePauseMenu(string menuName = "pause")
        {
            _isPauseToggling = true;
            IsPaused = !IsPaused;
            if (IsPaused)
            {
                ToggleFps();
                _menuCon.OpenMenu(menuName, () => { _isPauseToggling = false; });
            }
            else
            {
                _menuCon.CloseMenu(() =>
                {
                    ToggleFps();
                    _isPauseToggling = false;
                    _menuCon.getCurrentMenu().OnCancel();
                });
            }
        }
    }
}
