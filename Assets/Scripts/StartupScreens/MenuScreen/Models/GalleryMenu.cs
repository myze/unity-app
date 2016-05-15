using System.Collections;
using Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models
{
    public class GalleryMenu : Menu, IEmbeddedSetting
    {
        private ArrayList rowsOfLogos;
        private ArrayList logos;
        private MenuSetting.MenuSetting setting;

        public int ColCount { get { return logos.Count; } }
        public int RowCount { get { return rowsOfLogos.Count; } }

        public GalleryMenu(MenuSetting.MenuSetting setting) : base()
        {
            rowsOfLogos = new ArrayList();
            addNewRow();
            this.setting = setting;
        }

        public void addLogo(Logo logo)
        {
            logos.Add(logo);
        }

        public void addNewRow()
        {
            logos = new ArrayList();
            rowsOfLogos.Add(logos);
        }

        public Logo getLogo(int rowIndex, int colIndex)
        {
            if (rowIndex >= 0 && rowIndex < RowCount)
                if (colIndex >= 0 && colIndex < ColCount)
                    return (Logo)((ArrayList)rowsOfLogos[rowIndex])[colIndex];
            return null;
        }

        public MenuSetting.MenuSetting getMenuSetting()
        {
            return setting;
        }
    }
}
