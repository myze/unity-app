using Assets.Scripts.InMaze.Model;
using Assets.Scripts.StartupScreens.MenuScreen.Controllers;

namespace Assets.Scripts.InMaze.Controllers
{
    public class MenuControllerAdapter : MenuController
    {
        protected override void Awake()
        {
            base.Awake();
            StopAllCoroutines();

            menuCollection = new MenuAdapter(menuCollection.CommonMenuSetting);
            menuCollection.Init();
        }

        public void OpenMenu(string menu, AfterFadeDelegate after = null)
        {
            StartCoroutine(
                menuPanelTvOn(
                    MakeMenu,
                    menuCollection.GetMenu(menu),
                    after
                )
            );
        }

        public void CloseMenu(AfterFadeDelegate after)
        {
            menuFadeOut(after, false);
        }
    }
}
