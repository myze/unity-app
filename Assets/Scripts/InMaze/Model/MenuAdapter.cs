using System;
using UnityEngine;
using Assets.Scripts.StartupScreens.MenuScreen.Models;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using Assets.Scripts.InMaze.Controllers;
using Assets.Scripts.InMaze.Multiplayer;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using Assets.Scripts.InMaze.UI.Mapify;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Hybrid;
using Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.InMaze.Model
{
    [Serializable]
    public class MenuAdapter : MenuCollection
    {
        readonly InputController input = GameObject
            .Find("MapScripts")
            .GetComponent<InputController>();

        public MenuAdapter(MenuSetting commonMenuSetting)
        {
            this.CommonMenuSetting = commonMenuSetting;
        }

        public override void Init()
        {
            // Call base Init method
            base.Init();
            // Clear base menu dictionary
            Menus.Clear();
            Menus.Add("pause", InitPauseMenu());
            Menus.Add("endgame", InitEndGameMenu());
            Menus.Add("setting", InitSettingMenu());
        }

        Option GetMainMenuOption()
        {
            return new Option("Main Menu")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
				.SetDeflatedEventHandler((option) =>
                {
                    SceneManager.LoadScene("MenuScreen");
                });
        }

        PickerMenu InitEndGameMenu()
        {
            /* * * * * * * * * * * * * * *\
             * 		 End Game Menu       *
            \* * * * * * * * * * * * * * */

            PickerMenu endGameMenu = new PickerMenu();
            Label label = new Label(
                null,
                Color.green
            );
            endGameMenu.AddElement(label);

            // If in Multiplayer mode
            if (AbsServerController.IsPresent || AbsClientController.IsPresent)
            {
                label.Text = "Congrats!\n" +
                             "...\n" +
                             "Finished";
                endGameMenu.SetOnUpdateEventHandler((menu) =>
                {
                    // Check if Place is contained in Title
                    if (!label.Text.Contains("#"))
                        if (EscapeSeq.Present != null && menuController.IsMenuReady)
                        {
                            // Update Place
                            label.Text = label.Text.Replace(
                                "...",
                                "#" + EscapeSeq.Present.Place
                            );
                            // Update actualized Text
                            if (label.GameText != null)
                                label.GameText.text = label.Text;
                        }
                });

                endGameMenu.AddElement(new Option("Spectate")
                    .SetHoverEventHandler(HoverEnabled)
                    .SetSelectEventHandler(SelectNext)
					.SetDeflatedEventHandler((option) =>
                    {
                        // Re-enable input controller
                        input.enabled = true;

                        // Enter spectator mode
                        if (Spectator.Spectate())
                            new MessageBox(
                                menuController,
                                "Spectating",
                                MessageBox.DURATION_FOREVER, MessageBox.Y_LOW
                            ).ShowInstantly();

                        // End pausing
                        if (input.IsPaused)
                            input.TogglePauseMenu();
                    })
                );
            }
            else
            {
                label.Text = "Congrats!\n" +
                             "Finished";
            }

            endGameMenu.AddElement(GetMainMenuOption());
            return endGameMenu;
        }

        HybridMenu InitSettingMenu()
        {
            /* * * * * * * * * * * * * * *\
		     * 		  Setting Menu       *
		    \* * * * * * * * * * * * * * */

            // Binding
            MiniMap miniMap = GameObject
                .Find("SliderHUD/MapUI")
                .GetComponent<MiniMap>();

            HybridMenu settingMenu = new HybridMenu();

            HSlider miniMapScale = new HSlider(
                Resources.Load<GameObject>("prefab/slider"),
                1f,
                7f
            ).SetValue(TransScene.Present.MiniMapRenderSize == 0 ?
                2f : TransScene.Present.MiniMapRenderSize);

            settingMenu.AddElement(new HybridOption("MapScale:")
                .SetReference(miniMapScale)
				.SetLeftEventHandler((option) => { miniMapScale.MinusValue(0.5f); })
				.SetRightEventHandler((option) => { miniMapScale.AddValue(0.5f); })
            );

            VRNose vrnose = GameObject.Find("Nose").GetComponent<VRNose>();
            Option nose = new Option((vrnose.Show ? "Hide" : "Show") + " Nose");
            nose.SetHoverEventHandler(HoverEnabled)
				.SetSelectEventHandler((option) =>
                {
                    vrnose.Show = !vrnose.Show;
                    nose.GameText.text = (vrnose.Show ? "Hide" : "Show") + " Nose";
                    // Update TransScene
                    TransScene.Present.IsNoseShown = vrnose.Show;
                });
            settingMenu.AddElement(nose);

            settingMenu.AddCancelOption(new Option("Apply", Color.green)
                .SetHoverEventHandler(HoverEnabled)
                .SetSelectEventHandler(SelectNext)
				.SetDeflatedEventHandler((option) =>
                {
                    // Iff updated
                    if (miniMapScale.Value != miniMap.mapRenderSize)
                    {
                        // Update TransScene obj
                        TransScene.Present.MiniMapRenderSize = miniMapScale.Value;
                        // Update actual representing value
                        miniMap.mapRenderSize = miniMapScale.Value;
                        // Reset script
                        miniMap.Reset();
                    }

                    if (input.IsPaused)
                        // Back to pause menu
                        menuController.menuFadeOut(() =>
                        {
                            menuController.menuFadeIn("pause");
                        });
                })
            );

            return settingMenu;
        }

        PickerMenu InitPauseMenu()
        {
            /* * * * * * * * * * * * * * *\
		     * 		   Pause Menu        *
		    \* * * * * * * * * * * * * * */

            PickerMenu pauseMenu = new PickerMenu();

            pauseMenu.AddCancelOption(new Option("Resume")
                .SetHoverEventHandler(HoverEnabled)
                .SetSelectEventHandler(SelectNext)
				.SetDeflatedEventHandler((option) =>
                {
                    // End pausing
                    if (input.IsPaused)
                        input.TogglePauseMenu();
                })
            );


            pauseMenu.AddElement(new Option("Main Menu")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
				.SetDeflatedEventHandler((option) =>
                {
                    SceneManager.LoadScene("MenuScreen");
                })
            );

            pauseMenu.AddElement(new Option("Settings")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
				.SetDeflatedEventHandler((option) =>
                {
                    menuController.menuFadeOut(() =>
                    {
                        menuController.menuFadeIn("setting");
                    });
                })
            );

            return pauseMenu;
        }

    }
}
