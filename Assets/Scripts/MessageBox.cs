using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MessageBox
    {
        public string message { get; private set; }
        public int duration { get; private set; }
        public static bool HasMessageBoxOnScreen
        {
            get { return current != null; }
        }

        static MessageBox current;

        public delegate void OnFaded();

        static readonly Font FONT_FACE = Resources.Load<Font>("Juction-Light");
        static readonly Sprite BACKGROUND = Resources.Load<Sprite>("Background");
        const int FONT_SIZE = 9;
        const float BOX_HEIGHT = 25f, BACKDROP_MAX_ALPHA = 0.8f;
        const float FADE_RANGE = 0.03f, FADE_INCREMENT = 0.01f;

        public const int DURATION_SHORT = 100, DURATION_LONG = 300, DURATION_FOREVER = -1;
        public const float Y_LOW = -0.15f, Y_UP = 0.15f;

        readonly MonoBehaviour mono;
        float y;
        Text text;
        Image image;
        RectTransform canvas;
        OnFaded faded;

        public static void Show(MonoBehaviour mono)
        {
            Show(mono, " ");
        }

        public static void Show(MonoBehaviour mono, string message)
        {
            Show(mono, message, DURATION_SHORT);
        }

        public static void Show(MonoBehaviour mono, string message, int duration)
        {
            Show(mono, message, duration, 0);
        }

        public static void Show(MonoBehaviour mono, string message, int duration, float y)
        {
            new MessageBox(mono, message, duration, y).Show();
        }

        public static void Clear()
        {
            // Clear current
            current = null;
        }

        public MessageBox(MonoBehaviour mono) : this(mono, " ") { }

        public MessageBox(MonoBehaviour mono, string message) : this(mono, message, DURATION_SHORT) { }

        public MessageBox(MonoBehaviour mono, string message, int duration) : this(mono, message, duration, 0) { }

        public MessageBox(MonoBehaviour mono, string message, int duration, float y)
        {
            this.mono = mono;
            this.message = message;
            this.duration = duration;
            this.y = y;
        }

        public void Show()
        {
            mono.StartCoroutine(ShowCoroutine());
        }

        public void ShowInstantly()
        {
            mono.StartCoroutine(ShowCoroutine(true));
        }

        public void ShowOnlyAvailable()
        {
            // Show only if no message on screen
            if (!HasMessageBoxOnScreen)
                Show();
        }

        public void Dispose()
        {
            mono.StartCoroutine(DisposeCoroutine());
        }

        public MessageBox SetFadedEventHandler(OnFaded faded)
        {
            this.faded = faded;
            return this;
        }

        void Showing(bool instantly = false)
        {
            // Update object
            if (!instantly)
                current = this;

            // Create canvas
            GameObject canvas = new GameObject("MessageBox");
            canvas.transform.SetParent(Camera.main.transform);
            canvas.AddComponent<Canvas>();
            canvas.AddComponent<CanvasScaler>();

            // Width & height
            Vector2 v2 = new Vector2(
                Essentials.GetStringWidth(message, FONT_SIZE, FONT_FACE) + 15,
                BOX_HEIGHT
                );

            MakeRectTransform(
                canvas.GetComponent<RectTransform>(),
                v2
                );
            MakeRectTransform(
                canvas.GetComponent<RectTransform>(),
                new Vector3(0, y, 0.4f)
                );
            MakeRectTransform(
                canvas.GetComponent<RectTransform>(),
                0.002f,
                0.002f
                );
            MakeCanvas(
                canvas.GetComponent<Canvas>()
                );
            MakeCanvasScaler(
                canvas.GetComponent<CanvasScaler>()
                );

            // Create Image
            GameObject image = new GameObject("MbImage");
            image.transform.SetParent(canvas.transform);
            image.AddComponent<Image>();

            MakeRectTransform(
                image.GetComponent<RectTransform>(),
                v2
                );
            MakeImage(image.GetComponent<Image>());

            // Create Text
            GameObject text = new GameObject("MbText");
            text.transform.SetParent(canvas.transform);
            text.AddComponent<Text>();

            MakeRectTransform(
                text.GetComponent<RectTransform>(),
                v2
                );
            MakeText(text.GetComponent<Text>(), message);

            // Blinding
            this.canvas = canvas.GetComponent<RectTransform>();
            this.text = text.GetComponent<Text>();
            this.image = image.GetComponent<Image>();
        }

        void Disposing()
        {
            GameObject msgBox = GameObject.Find("MessageBox");
            if (msgBox != null)
                Object.Destroy(msgBox);

            // Update current object
            current = null;

            // Call faded event
            if (faded != null)
                faded();
        }

        void Fading(float y, float z, float a)
        {
            canvas.localPosition = new Vector3(
                canvas.localPosition.x,
                y,
                z
                );

            text.color = new Color(
                text.color.r,
                text.color.g,
                text.color.b,
                a
                );

            a *= BACKDROP_MAX_ALPHA;

            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                a
                );
        }

        IEnumerator Fade(float from, float to, bool isFadingOut, OnFaded faded)
        {
            float y = canvas.localPosition.y, z = canvas.localPosition.z;
            for (float f = from; f > to; f -= FADE_INCREMENT)
            {
                Fading(
                    y + f,
                    z + f * ((isFadingOut) ? -1 : 1),
                    Mathf.Abs(f - FADE_RANGE) / FADE_RANGE
                    );
                yield return null;
            }
            // Set to absolute target values
            Fading(
                y + to,
                z + to * ((isFadingOut) ? 1 : -1),
                Mathf.Abs(to - FADE_RANGE) / FADE_RANGE
                );
            faded();
        }

        IEnumerator ShowCoroutine(bool instantly = false)
        {
            // Loop until no MessageBox is displaying
            while (current != null && !instantly)
                yield return null;

            Showing(instantly);
            mono.StartCoroutine(Fade(FADE_RANGE, 0, false, Dispose));
        }

        IEnumerator DisposeCoroutine()
        {
            // Loop until time out
            int i = duration;
            // Negative number won't be disposed
            if (i >= 0)
            {
                while (i-- > 0)
                    yield return null;

                mono.StartCoroutine(Fade(0, -FADE_RANGE, true, Disposing));
            }
        }

        void MakeImage(Image image)
        {
            image.sprite = BACKGROUND;
            image.color = new Color(
                0x22 / (float)0xff,
                0x2c / (float)0xff,
                0x35 / (float)0xff,
                .75f
                );
            image.type = Image.Type.Sliced;
        }

        void MakeCanvas(Canvas canvas)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }

        void MakeCanvasScaler(CanvasScaler canvasScaler)
        {
            canvasScaler.dynamicPixelsPerUnit = 5;
            canvasScaler.referencePixelsPerUnit = 100;
        }

        void MakeRectTransform(RectTransform rectTransform, Vector2 sizeDelta)
        {
            sizeDelta = new Vector2(
                sizeDelta.x <= 15 ? 15 : sizeDelta.x,
                sizeDelta.y
                );

            rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = sizeDelta;
        }

        void MakeRectTransform(RectTransform rectTransform, float scaleX, float scaleY)
        {
            rectTransform.localScale = new Vector3(scaleX, scaleY);
        }

        void MakeRectTransform(RectTransform rectTransform, Vector3 localPosition)
        {
            rectTransform.localPosition = localPosition;
        }

        void MakeText(Text text, string message)
        {
            text.text = message;
            text.color = new Color(1, 1, 1, 0);
            text.font = FONT_FACE;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = FONT_SIZE;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
    }
}
