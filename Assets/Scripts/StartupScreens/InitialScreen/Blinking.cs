using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.InitialScreen
{
    public class Blinking : MonoBehaviour
    {
        [SerializeField]
        bool transpFlag = false, isStopped = false;
        [SerializeField]
        int delay = 0;
        bool isDecade = false;
        Color color;
        MaskableGraphic obj;
        public bool isBoundary = false;
        public int speed = 5;

        public void wait(int delay)
        {
            this.delay = delay;
        }

        public void fadeOut()
        {
            isDecade = true;
            speed = 10;
        }

        public void stop()
        {
            freeze();
            obj.color = color;
        }

        public void freeze()
        {
            isStopped = true;
        }

        public void resume()
        {
            isStopped = false;
        }

        public void changeColor(float r, float g, float b, bool instant = false)
        {
            color = new Color(r, g, b);
            if (instant)
            {
                obj.color = new Color(r, g, b, obj.color.a);
            }
        }

        float closer(float from, float to)
        {
            if (from > to)
            {
                from -= 0.002f;
            }
            else if (from < to)
            {
                from += 0.002f;
            }
            return from;
        }

        // Use this for initialization
        void Awake()
        {
            if (GetComponent<Text>() != null)
            {
                obj = GetComponent<Text>();
                color = new Color(1f, 1f, 1f);
                obj.color = color;
            }
            else {
                obj = GetComponent<Image>();
                color = new Color(0.8f, 0.8f, 0.8f);
                obj.color = color;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isStopped)
            {
                if (delay == 0 || !isBoundary)
                {
                    obj.color = new Color(
                        closer(obj.color.r, color.r),
                        closer(obj.color.g, color.g),
                        closer(obj.color.b, color.b),
                        obj.color.a + ((transpFlag) ? speed / 255f : -speed / 255f)
                        );

                    if (isDecade || obj.color.a >= 1)
                    {
                        transpFlag = false;
                    }
                    else if (obj.color.a <= 0)
                    {
                        transpFlag = true;
                    }

                    isBoundary = obj.color.a <= 0;
                }
                else {
                    delay--;
                }
            }
        }
    }
}
