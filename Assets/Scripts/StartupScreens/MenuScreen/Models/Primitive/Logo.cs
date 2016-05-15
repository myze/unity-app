using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
    public class Logo
    {
        private Sprite logoSprite;
        private float width, height;

        public Logo(Sprite sprite)
        {
            logoSprite = sprite;
        }

        public Logo(Sprite sprite, float width, float height) : this(sprite)
        {
            this.width = width;
            this.height = height;
        }

        public float getWidth()
        {
            return width;
        }

        public float getHeight()
        {
            return height;
        }

        public Sprite getLogoSprite()
        {
            return logoSprite;
        }
    }
}
