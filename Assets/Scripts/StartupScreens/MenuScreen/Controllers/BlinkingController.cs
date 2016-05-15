using System.Collections;
using Assets.Scripts.StartupScreens.InitialScreen;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.MenuScreen.Controllers
{
    public class BlinkingController : MonoBehaviour
    {
        [SerializeField]
        RectTransform selected;
        [SerializeField]
        [Range(1f, 5f)]
        float deflationSpeed = 3.5f;
        [SerializeField]
        [Range(0f, 0.25f)]
        float spinDelay = 0.15f;
        Image RT, RB, LT, LB;
        bool isSpinning, isDeflating, isInflating;
        MenuController menuController;

        // When inflated
        public delegate void inflatedDelegate();

        // Use this for initialization
        void Start()
        {
            //Binding
            RT = GameObject.Find("RT").GetComponent<Image>();
            RB = GameObject.Find("RB").GetComponent<Image>();
            LT = GameObject.Find("LT").GetComponent<Image>();
            LB = GameObject.Find("LB").GetComponent<Image>();
            menuController = GetComponent<MenuController>();

            setDotsEnabled(false);
            colorEnabled();

            isSpinning = false;
            isDeflating = false;
            isInflating = false;
        }

        void dotsLoc()
        {
            //Change location of dots
            RT.transform.localPosition = new Vector3(
                selected.localPosition.x + selected.sizeDelta.x / 2,
                selected.localPosition.y + selected.sizeDelta.y / 2
                );
            LT.transform.localPosition = new Vector3(
                selected.localPosition.x - selected.sizeDelta.x / 2,
                selected.localPosition.y + selected.sizeDelta.y / 2
                );
            RB.transform.localPosition = new Vector3(
                selected.localPosition.x + selected.sizeDelta.x / 2,
                selected.localPosition.y - selected.sizeDelta.y / 2
                );
            LB.transform.localPosition = new Vector3(
                selected.localPosition.x - selected.sizeDelta.x / 2,
                selected.localPosition.y - selected.sizeDelta.y / 2
                );
        }

        public void stopBlinking()
        {
            LT.GetComponent<Blinking>().stop();
            RT.GetComponent<Blinking>().stop();
            LB.GetComponent<Blinking>().stop();
            RB.GetComponent<Blinking>().stop();
        }

        public void resumeBlinking()
        {
            LT.GetComponent<Blinking>().resume();
            RT.GetComponent<Blinking>().resume();
            LB.GetComponent<Blinking>().resume();
            RB.GetComponent<Blinking>().resume();
        }

        public void sparkOnce()
        {
            StartCoroutine(spark());
        }

        public void colorEnabled()
		{
			/* 
			 * https://www.google.com/design/spec/style/color.html#color-color-palette
			 * palette 200 colors
			 */
			// blue
			LT.GetComponent<Blinking>().changeColor(
				0x80 / (float)0xff,
				0xde / (float)0xff,
				0xea / (float)0xff,
				true);
			// red
			RT.GetComponent<Blinking>().changeColor(
				0xf4 / (float)0xff,
				0x8f / (float)0xff,
				0xb1 / (float)0xff,
				true);
			// yellow
			LB.GetComponent<Blinking>().changeColor(
				0xff / (float)0xff,
				0xe0 / (float)0xff,
				0x82 / (float)0xff,
				true);
			// green
			RB.GetComponent<Blinking>().changeColor(
				0xa5 / (float)0xff,
				0xd6 / (float)0xff,
				0xa7 / (float)0xff,
				true);
        }

        public void colorDisabled()
        {
            colorGrey(LT);
            colorGrey(RT);
            colorGrey(LB);
            colorGrey(RB);
        }

        public void setSelected(RectTransform rect)
        {
            selected = rect;
        }

        public void setDotsEnabled(bool enabled)
        {
            LT.gameObject.SetActive(enabled);
            RT.gameObject.SetActive(enabled);
            LB.gameObject.SetActive(enabled);
            RB.gameObject.SetActive(enabled);

            if (enabled) resumeBlinking();
            else stopBlinking();
        }

        public void deflate()
        {
            isDeflating = true;
            DisableSelectedOption();
            StartCoroutine(deflateCoroutine());
        }

        public void inflate(inflatedDelegate onInflated = null)
        {
            // Set select item for dots
            selected = menuController
                .getSelectedObject()
                .GetComponent<RectTransform>();

            isInflating = true;
            StartCoroutine(inflateCoroutine(onInflated));
        }

        public void startSpinning()
        {
            isSpinning = true;
            StartCoroutine(spin());
        }

        public void stopSpinning()
        {
            isSpinning = false;
        }

        public IEnumerator setDotsEnabledCoroutine(bool enabled)
        {
            float menuFadeSpeed = menuController.menuFadeSpeed;
            RT.gameObject.SetActive(enabled);
            yield return new WaitForSeconds(menuFadeSpeed);
            RB.gameObject.SetActive(enabled);
            yield return new WaitForSeconds(menuFadeSpeed);
            LB.gameObject.SetActive(enabled);
            yield return new WaitForSeconds(menuFadeSpeed);
            LT.gameObject.SetActive(enabled);
            yield return new WaitForSeconds(menuFadeSpeed);

            if (menuController.IsMenuReady)
                if (enabled) resumeBlinking();
                else stopBlinking();
        }

        IEnumerator spark()
        {
            resumeBlinking();
            for (int i = 0; i < 20; i++)
            {
                yield return null;
            }
            stopBlinking();
        }

        void colorGrey(Image image)
        {
            image.GetComponent<Blinking>().changeColor(0.5f, 0.5f, 0.5f, true);
        }

        void DisableSelectedOption()
        {
            GameObject selected = menuController.getSelectedObject();
            // Disable gameObject textcolor
            foreach (Text txt in selected.GetComponents<Text>())
                txt.color = Color.clear;
            // Disable children gameObject textcolor
            foreach (Text txt in selected.GetComponentsInChildren<Text>())
                txt.color = Color.clear;
        }

        void EnableSelectedOption()
        {
            // Similar logic as DisableSelectedOption()
            GameObject selected = menuController.getSelectedObject();
            Option sOption = menuController.getSelectedOption();

            foreach (Text txt in selected.GetComponents<Text>())
                txt.color = sOption.TextColor;
            foreach (Text txt in selected.GetComponentsInChildren<Text>())
                txt.color = sOption.TextColor;
        }

        IEnumerator inflateCoroutine(inflatedDelegate inflated)
        {
            float xT = RT.transform.localPosition.x, yT = RT.transform.localPosition.y;
            float xL = LB.transform.localPosition.x, yL = LB.transform.localPosition.y;
            bool retn = true;

            if (selected != null)
            {
                if (xT < selected.localPosition.x + selected.sizeDelta.x / 2 - 1)
                {
                    xT += deflationSpeed;
                    retn = false;
                }
                else
                    xT = selected.localPosition.x + selected.sizeDelta.x / 2;

                if (yT < selected.localPosition.y + selected.sizeDelta.y / 2 - 1)
                {
                    yT += deflationSpeed;
                    retn = false;
                }
                else
                    yT = selected.localPosition.y + selected.sizeDelta.y / 2;

                if (xL > selected.localPosition.x - selected.sizeDelta.x / 2 + 1)
                {
                    xL -= deflationSpeed;
                    retn = false;
                }
                else
                    xL = selected.localPosition.x - selected.sizeDelta.x / 2;

                if (yL > selected.localPosition.y - selected.sizeDelta.y / 2 + 1)
                {
                    yL -= deflationSpeed;
                    retn = false;
                }
                else
                    yL = selected.localPosition.y - selected.sizeDelta.y / 2;

                RT.transform.localPosition = new Vector3(xT, yT);
                LT.transform.localPosition = new Vector3(xL, yT);
                RB.transform.localPosition = new Vector3(xT, yL);
                LB.transform.localPosition = new Vector3(xL, yL);
            }

            yield return null;

            // Recursive calling until done
            if (!retn || selected == null)
                StartCoroutine(inflateCoroutine(inflated));
            else
            {
                // Indicate end of deflating
                isInflating = false;

                // Enable input
                menuController.IsMenuReady = true;

                resumeBlinking();

                // Reset option's color
                EnableSelectedOption();

                if (inflated != null)
                    inflated();
            }
        }

        IEnumerator deflateCoroutine()
        {
            float xT = RT.transform.localPosition.x, yT = RT.transform.localPosition.y;
            float xL = LB.transform.localPosition.x, yL = LB.transform.localPosition.y;
            bool retn = true;

            if (RT.transform.localPosition.x - selected.localPosition.x > 2)
            {
                xT -= deflationSpeed;
                retn = false;
            }
            else
                xT = 1.5f + selected.localPosition.x;

            if (RT.transform.localPosition.y - selected.localPosition.y > 2)
            {
                yT -= deflationSpeed;
                retn = false;
            }
            else
                yT = 1.5f + selected.localPosition.y;

            if (LB.transform.localPosition.x - selected.localPosition.x < -2)
            {
                xL += deflationSpeed;
                retn = false;
            }
            else
                xL = -1.5f + selected.localPosition.x;

            if (LB.transform.localPosition.y - selected.localPosition.y < -2)
            {
                yL += deflationSpeed;
                retn = false;
            }
            else
                yL = -1.5f + selected.localPosition.y;

            RT.transform.localPosition = new Vector3(xT, yT);
            LT.transform.localPosition = new Vector3(xL, yT);
            RB.transform.localPosition = new Vector3(xT, yL);
            LB.transform.localPosition = new Vector3(xL, yL);

            yield return null;

            // Recursive calling until done
            if (!retn)
                StartCoroutine(deflateCoroutine());
            else
            {
                // Indicate end of deflating
                isDeflating = false;

                // Prevent dotsLoc()
                selected = null;

                // Perform OnDeflated
                menuController
                    .getSelectedOption()
                    .OnDeflated();
            }
        }

        // Spin in Windows loading style
        IEnumerator spin(int mode = 2, int index = 0)
        {
            Image[] dots = new Image[4];
            dots[0] = RT;
            dots[1] = RB;
            dots[2] = LB;
            dots[3] = LT;

            if (mode > 1 || index == 0 && mode == 0)
                dots[(3 + index) % 4].gameObject.SetActive(true);

            for (int i = 0; i < dots.Length; i++)
                if (i == index)
                {
                    dots[i].gameObject.SetActive(mode == 1);
                    break;
                }

            yield return new WaitForSeconds(spinDelay);

            index = (index + 1) % 4;
            // Recursive call for looping
            if (isSpinning)
                StartCoroutine(spin(
                    (index == 0) ? (mode + 1) % 4 : mode,
                    index
                    ));
        }

        // Update is called once per frame
        void Update()
        {
            if (selected != null && !isDeflating && !isInflating)
                dotsLoc();
        }
    }
}
