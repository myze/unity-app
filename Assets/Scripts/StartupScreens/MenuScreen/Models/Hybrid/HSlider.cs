using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Hybrid
{
    public class HSlider : Hybrid
    {
        public float Max { private set; get; }
        public float Min { private set; get; }
        public float Value { private set; get; }

        public HSlider(GameObject slider, float min, float max)
            : base(slider)
        {
            Max = max;
            Min = min;
        }

        public HSlider SetValue(float value)
        {
            if (value >= Min && value <= Max)
                Value = value;
			Actualize ();
            return this;
        }

        public HSlider AddValue(float increment)
        {
            float oValue = Value;
            Value += increment;
            // Roll back if value exceed
            if (Value > Max)
                Value = oValue;
            Value = Mathf.Round(Value * 10) / 10f;
			Actualize ();
            return this;
        }

        public HSlider MinusValue(float decrement)
        {
            float oValue = Value;
            Value -= decrement;
            // Roll back if value exceed
            if (Value < Min)
                Value = oValue;
            Value = Mathf.Round(Value * 10) / 10f;
			Actualize ();
            return this;
        }

        public override GameObject Actualize()
        {
            Slider slider = base.Actualize().GetComponent<Slider>();
            slider.minValue = Min;
            slider.maxValue = Max;
            slider.value = Value;

            return slider.gameObject;
        }
    }
}
