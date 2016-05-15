using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
    public class InteractiveOption : Option
    {
        // Event raised when left button is pressed
        protected EventHandler LeftTrigger;
        // Event raised when right button is pressed
        protected EventHandler RightTrigger;

        public InteractiveOption(string label)
            : base(label)
        { }

        public InteractiveOption(string label, Color labelColor)
            : base(label, labelColor)
        { }

        public InteractiveOption(string label, string labelColorString)
            : base(label, labelColorString)
        { }

        public InteractiveOption(Option option) : base(option.Text, option.TextColor)
        {
            SetDeflatedEventHandler((optn) => { option.OnDeflated(); });
            SetSelectEventHandler((optn) => { option.OnSelect(); });
            SetHoverEventHandler((optn) => { option.OnHover(); });
        }

        public void OnLeftTrigger()
        {
            if (LeftTrigger != null) LeftTrigger(this);
        }

        public void OnRightTrigger()
        {
            if (RightTrigger != null) RightTrigger(this);
        }

        public InteractiveOption SetLeftEventHandler(EventHandler eventHandler)
        {
            LeftTrigger = eventHandler;
            return this;
        }

        public InteractiveOption SetRightEventHandler(EventHandler eventHandler)
        {
            RightTrigger = eventHandler;
            return this;
        }
    }
}
