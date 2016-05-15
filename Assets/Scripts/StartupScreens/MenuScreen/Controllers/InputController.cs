using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Assets.Scripts.StartupScreens.MenuScreen.Controllers
{
    public class InputController : MonoBehaviour
    {
        [SerializeField]
        int verticalTimeOut = 10, horizontalTimeOut = 1;
        MenuController _menuController;
        int _up = -1, _down = -1, _left = -1, _right = -1;

        // Use this for initialization
        void Start()
        {
            _menuController = GameObject.Find("MenuPanel").GetComponent<MenuController>();
        }

        void FixedUpdate()
        {
            if (_menuController.IsMenuReady)
            {
                VerticalSelect();
                HorizontalSelect();
            }
        }

        void Update()
        {
            if (_menuController.IsMenuReady)
            {
				if (Input.GetKeyDown(TransScene.Present.Controller.A))
                    _menuController.getSelectedOption().OnSelect();
				if (Input.GetKeyDown(TransScene.Present.Controller.B))
                    _menuController.getCurrentMenu().OnCancel();
            }
        }

        void HorizontalSelect()
        {
			float horizontal = TransScene.Present.Controller.GetAxisRaw("Horizontal");
            InteractiveOption option = _menuController.getSelectedOption()
                as InteractiveOption;

            if (horizontal == 1 && _right == -1 && option != null)
            {
                option.OnRightTrigger();
                // Start a timer to prevent being called again
                _right = 0;
            }
            else if (horizontal == -1 && _left == -1 && option != null)
            {
                option.OnLeftTrigger();
                // Start a timer to prevent being called again
                _left = 0;
            }

            _left = (_left == -1) ? -1 : _left + 1;
            _right = (_right == -1) ? -1 : _right + 1;

            if ((_right == horizontalTimeOut || _left == horizontalTimeOut) ||
                (_right != -1 && horizontal == -1) ||
                (_left != -1 && horizontal == 1))
            {
                // Re-enable input
                _left = _right = -1;
            }
        }

        void VerticalSelect()
        {
			float vertical = TransScene.Present.Controller.GetAxisRaw("Vertical");

            if (vertical == 1 && _up == -1)
            {
                _menuController.selectPrev();
                // Start a timer to prevent being called again
                _up = 0;
            }
            else if (vertical == -1 && _down == -1)
            {
                _menuController.selectNext();
                // Start a timer to prevent being called again
                _down = 0;
            }

            // Option hovering event
            if (_up == 0 || _down == 0)
            {
                _menuController.getSelectedOption().OnHover();
            }

            _up = (_up == -1) ? -1 : _up + 1;
            _down = (_down == -1) ? -1 : _down + 1;

            if ((_up == verticalTimeOut || _down == verticalTimeOut) ||
                (_up != -1 && vertical == -1) ||
                (_down != -1 && vertical == 1))
            {
                // Re-enable input
                _up = _down = -1;
            }
        }
    }
}
