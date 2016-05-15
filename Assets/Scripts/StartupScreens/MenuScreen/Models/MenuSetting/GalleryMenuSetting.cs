using System;
using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting
{
    [Serializable]
    public class GalleryMenuSetting : MenuSetting
    {
        [Range(0, 1)]
        public float logoShunkenBase = 1f;
        public int logoPadding = 10;
    }
}
