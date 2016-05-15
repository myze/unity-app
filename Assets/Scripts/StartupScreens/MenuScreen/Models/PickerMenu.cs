
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models
{
    public class PickerMenu : Menu
    {
        private Logo logo;

        public Logo GetLogo()
        {
            return logo;
        }

        public void SetLogo(Logo logo)
        {
            this.logo = logo;
        }
    }
}
