using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
    public class Option : Label
    {
		public delegate void EventHandler(Option option);

        // Raised when action of option is being performed
        protected EventHandler Deflated;
        // Raised when selected with 4 dots indicator
        protected EventHandler Hover;
        // Raised when confirm button is pressed
        protected EventHandler Select;

        public Option(string label) : 
            base(label) { }

        public Option(string label, Color labelColor) : 
            base(label, labelColor) { }

        public Option(string label, string labelColorString) : 
            this(label, Essentials.ParseColor(labelColorString)) { }

        public void OnDeflated()
        {
            if (Deflated != null) { Deflated(this); }
        }

        public void OnHover()
        {
            if (Hover != null) { Hover(this); }
        }

        public void OnSelect()
        {
            if (Select != null) { Select(this); }
        }

        public Option SetDeflatedEventHandler(EventHandler eventHandler)
        {
            Deflated = eventHandler;
            return this;
        }

        public Option SetHoverEventHandler(EventHandler eventHandler)
        {
            Hover = eventHandler;
            return this;
        }

        public Option SetSelectEventHandler(EventHandler eventHandler)
        {
            Select = eventHandler;
            return this;
        }
    }
}
