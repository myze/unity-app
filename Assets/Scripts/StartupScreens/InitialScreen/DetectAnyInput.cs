using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.InputMethods.Joysticks;

namespace Assets.Scripts.StartupScreens.InitialScreen
{
    public class DetectAnyInput : MonoBehaviour
    {
        bool isPressed;
        Text txt;
        Image LT, LB, RT, RB;

        // Use this for initialization
        void Start()
        {
            // Binding
            txt = GameObject.Find("Text").GetComponent<Text>();
            LT = GameObject.Find("LT").GetComponent<Image>();
            RT = GameObject.Find("RT").GetComponent<Image>();
            LB = GameObject.Find("LB").GetComponent<Image>();
            RB = GameObject.Find("RB").GetComponent<Image>();

#if UNITY_STANDALONE
            txt.text = "Using keyboard/mouse to start";
#endif
        }

        // Update is called once per frame
        void Update()
        {
            isPressed = Input.GetKey(KeyCode.JoystickButton0) ||
            Input.GetKey(KeyCode.JoystickButton1) ||
            Input.GetKey(KeyCode.JoystickButton2) ||
            Input.GetKey(KeyCode.JoystickButton3) ||
            Input.GetKey(KeyCode.JoystickButton4) ||
            Input.GetKey(KeyCode.JoystickButton5) ||
            Input.GetKey(KeyCode.JoystickButton6) ||
            Input.GetKey(KeyCode.JoystickButton7) ||
            Input.GetKey(KeyCode.JoystickButton8) ||
            Input.GetKey(KeyCode.JoystickButton9) ||
            Input.GetKey(KeyCode.JoystickButton10) ||
            Input.GetKey(KeyCode.JoystickButton11) ||
            Input.GetKey(KeyCode.JoystickButton12) ||
            Input.GetKey(KeyCode.JoystickButton13) ||
            Input.GetKey(KeyCode.JoystickButton14) ||
            Input.GetKey(KeyCode.JoystickButton15) ||
            Input.GetKey(KeyCode.JoystickButton16) ||
            isPressed;

            if (isPressed)
            {
                LT.GetComponent<Blinking>().fadeOut();
                RT.GetComponent<Blinking>().fadeOut();
                LB.GetComponent<Blinking>().fadeOut();
                RB.GetComponent<Blinking>().fadeOut();
                txt.GetComponent<Blinking>().fadeOut();
            }

            if (txt.GetComponent<Blinking>().isBoundary)
            {
#if UNITY_STANDALONE
                SceneManager.LoadScene("MenuScreen");
#endif
                //Detect if bluetooth device connected
                if (Input.GetJoystickNames().Length > 0 && txt.text != "Press any key on controller to start")
                {

                    // Get joystick key config
                    JoystickController jc = JoystickController
                        .GetJoystick(Input.GetJoystickNames()[0]);
                    TransScene.Present.Controller = jc ?? JoystickController.Default;

                    txt.text = "Press any key on controller to start";

                    LT.GetComponent<Blinking>().wait(10);
                    LT.GetComponent<Blinking>().changeColor(1f, 0.5f, 0.5f);
                    RT.GetComponent<Blinking>().wait(40);
                    RT.GetComponent<Blinking>().changeColor(0.5f, 0.5f, 1f);
                    LB.GetComponent<Blinking>().wait(70);
                    LB.GetComponent<Blinking>().changeColor(0.5f, 1f, 0.5f);
                    RB.GetComponent<Blinking>().wait(100);
                    RB.GetComponent<Blinking>().changeColor(1f, 1f, 0.5f);

                }
                else if (Input.GetJoystickNames().Length == 0)
                {
                    txt.text = "Connect a bluetooth controller to continue";
                }

                // Detect any input from bluetooth controller
                if (isPressed)
                {
                    if (LT.GetComponent<Blinking>().isBoundary)
                    {
                        if (RT.GetComponent<Blinking>().isBoundary)
                        {
                            if (LB.GetComponent<Blinking>().isBoundary)
                            {
                                if (RB.GetComponent<Blinking>().isBoundary)
                                {
                                    SceneManager.LoadScene("MenuScreen");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
