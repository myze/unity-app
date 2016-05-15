using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.MenuScreen
{
    public class SliderText : MonoBehaviour
    {
        // Bind text with slider value
        public void OnValueChanged()
        {
            GetComponent<Text>().text =
                transform.parent.GetComponent<Slider>().value.ToString();
        }
    }
}
