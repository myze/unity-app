using System;
using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting
{
    [Serializable]
    public class MenuSetting
    {
        public float logoWidth = 40f, logoHeight = 40f;
        public float logoPosition = 10f;
        public Sprite logoSprite;
        public Color logoColor = Color.white;
        public float optionPadding = 15f;
        public float optionBeginPos = 0f;

        [Range(1f, 10f)]
        public float shunkenBase = 10f;

        public int titlePos = 45;
        public Color titleColor = Color.white;
        [Range(5, 36)]
        public int titleFontSize = 12;
    }
}
