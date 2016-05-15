using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
    public class NetworkOption : Option
    {
        private EventHandler savedSelect, savedHover;

        public NetworkOption(string label) : base(label) { }
        public NetworkOption(string label, string labelColorString) : base(label, labelColorString) { }
        public NetworkOption(string label, Color labelColor) : base(label, labelColor) { }

        public bool isAvailable()
        {
            return Essentials.HasInternetConnection();
        }

        public void pushSelectEventHandler()
        {
            if (savedSelect == null)
                savedSelect = Select;
        }

        public void pushHoverEventHandler()
        {
            if (savedHover == null)
                savedHover = Hover;
        }

        public void popSelectEventHandler()
        {
            if (isSelectPushed())
            {
                SetSelectEventHandler(savedSelect);
                savedSelect = null;
            }
        }

        public void popHoverEventHandler()
        {
            if (isHoverPushed())
            {
                SetHoverEventHandler(savedHover);
                savedHover = null;
            }
        }

        public bool isHoverPushed()
        {
            return savedHover != null;
        }

        public bool isSelectPushed()
        {
            return savedSelect != null;
        }
    }
}
