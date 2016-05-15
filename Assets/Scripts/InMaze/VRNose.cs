using UnityEngine;

namespace Assets.Scripts.InMaze
{
    public class VRNose : MonoBehaviour, IRenderSeq
    {
        [SerializeField]
        private float _heightRatio = 0.4f;
        [SerializeField]
        private float _weightRatio = 0.5f;
        [SerializeField]
        private Color _color = Color.white;

        public bool Show;

        // Awake is called when the script instance is being loaded
        public void Awake()
        {
            // Retrieve from TransScene
            Show = TransScene.Present.IsNoseShown;
        }

        // Start is called just before any of the Update methods is called the first time
        public void Start()
        {
            // Add to CardboardPostRender
            GameObject.Find("PostRender")
                .GetComponent<CardboardPostRender>()
                .renderers.AddLast(this);
        }

        public void OnSeqRenderObject()
        {
            if (Show)
            {
                int x = Screen.width / 2;
                int h = (int)(Screen.height * _heightRatio);
                int w = (int)(h * _weightRatio);
                int shiftX = w / 12;
                int shiftY = h / 5;
                int indentX = w / 5;
                int indentY = h / 2;

                // Set color
                Material skin =
                    new Material(Shader.Find("Cardboard/SolidColor"))
                    {
                        color = _color
                    };
                skin.SetPass(0);

                // Left nose
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);
                GL.Begin(GL.QUADS);
                GL.Vertex3(x - w - shiftX, shiftY, 0);
                GL.Vertex3(x - w - shiftX + indentX, h + shiftY - indentY, 0);
                GL.Vertex3(x - shiftX, h + shiftY, 0);
                GL.Vertex3(x - shiftX, shiftY, 0);
                GL.End();
                GL.PopMatrix();

                // Right nose
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);
                GL.Begin(GL.QUADS);
                GL.Vertex3(x + shiftX, shiftY, 0);
                GL.Vertex3(x + shiftX, h + shiftY, 0);
                GL.Vertex3(x + w + shiftX - indentX, h + shiftY - indentY, 0);
                GL.Vertex3(x + w + shiftX, shiftY, 0);
                GL.End();
                GL.PopMatrix();
            }
        }
    }
}