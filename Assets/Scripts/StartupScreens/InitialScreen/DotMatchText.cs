using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.InitialScreen
{
    public class DotMatchText : MonoBehaviour
    {
        private Text _txt;
        private Image _lt, _rt, _lb, _rb;

        // Use this for initialization
        void Start()
        {
            //Binding
            _txt = GameObject.Find("Text").GetComponent<Text>();
            _lt = GameObject.Find("LT").GetComponent<Image>();
            _rt = GameObject.Find("RT").GetComponent<Image>();
            _lb = GameObject.Find("LB").GetComponent<Image>();
            _rb = GameObject.Find("RB").GetComponent<Image>();

            DotsLoc();
        }

        // Update is called every frame, if the MonoBehaviour is enabled
        void Update()
        {
            DotsLoc();
        }

        void DotsLoc()
        {
            //Change location of dots
            _rt.transform.localPosition = new Vector3(
                Essentials.GetStringWidth(_txt.text, _txt.fontSize, _txt.font) + 50,
                _lt.transform.localPosition.y
                );
            _lt.transform.localPosition = new Vector3(
                -(Essentials.GetStringWidth(_txt.text, _txt.fontSize, _txt.font) + 50),
                _rt.transform.localPosition.y
                );
            _rb.transform.localPosition = new Vector3(
                Essentials.GetStringWidth(_txt.text, _txt.fontSize, _txt.font) + 50,
                _lb.transform.localPosition.y
                );
            _lb.transform.localPosition = new Vector3(
                -(Essentials.GetStringWidth(_txt.text, _txt.fontSize, _txt.font) + 50),
                _rb.transform.localPosition.y
                );
        }
    }
}
