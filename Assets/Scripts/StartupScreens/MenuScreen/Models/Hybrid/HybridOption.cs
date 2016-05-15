using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Hybrid
{
    public class HybridOption : InteractiveOption
    {
        public IHybridation Hybrid { private set; get; }

        public HybridOption(string label)
            : base(label)
        { }

        public HybridOption(string label, Color labelColor)
            : base(label, labelColor)
        { }

        public HybridOption(string label, string labelColorString)
            : base(label, labelColorString)
        { }

        public HybridOption SetReference(IHybridation hybrid)
        {
            Hybrid = hybrid;
            return this;
        }
    }
}
