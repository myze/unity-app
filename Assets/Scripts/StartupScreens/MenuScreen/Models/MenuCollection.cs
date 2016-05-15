using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Scripts.InMaze.Controllers;
using Assets.Scripts.InMaze.Multiplayer;
using Assets.Scripts.InMaze.Networking;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.UDP;
using Assets.Scripts.StartupScreens.MenuScreen.Controllers;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Hybrid;
using Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MenuController = Assets.Scripts.StartupScreens.MenuScreen.Controllers.MenuController;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models
{
    [Serializable]
    public class MenuCollection
    {
        public string MenuHeading = "VR Maze Engine";
        public MenuSetting.MenuSetting CommonMenuSetting = new MenuSetting.MenuSetting();
        public MassiveMenuSetting MazeListSetting = new MassiveMenuSetting();
        public GalleryMenuSetting CreditPageSetting = new GalleryMenuSetting();

        // Default events
        public Option.EventHandler
            SelectNext,
            SelectDisabled,
            HoverEnabled,
            HoverDisabled,
            DeflatedEnterMaze;

        protected Dictionary<string, Menu> Menus { get; private set; }

        private Menu currentMenu;
        private readonly Stack<string> prevMenuStack = new Stack<string>();
        // For gamemode pre-request checking
        private delegate bool DoesGameModeMatch(MazeData mazeData);

        protected MenuController menuController { get; private set; }

        protected BlinkingController bkController { get; private set; }

        public virtual void Init()
        {
            // Binding
            menuController = GameObject.Find("MenuPanel").GetComponent<MenuController>();
            bkController = GameObject.Find("MenuPanel").GetComponent<BlinkingController>();

            // Select --> 4 dots indicator deflate animation
            SelectNext = (option) =>
            {
                // Disable input
                menuController.IsMenuReady = false;

                bkController.stopBlinking();
                bkController.deflate();
            };

            // Select --> Show disabled option
            SelectDisabled = (option) =>
            {
                bkController.sparkOnce();
            };

            // Hover --> enabled
            HoverEnabled = (option) =>
            {
                bkController.colorEnabled();
                bkController.resumeBlinking();
            };

            // Hover --> disabled
            HoverDisabled = (option) =>
            {
                bkController.colorDisabled();
                bkController.stopBlinking();
            };

            // Deflated --> enter maze
            DeflatedEnterMaze = (option) =>
            {
                TransitionEffects(() =>
                {
                    // Prepare TransScene parameters
                    TransScene.Present.MazeRenderSize =
                        ((HSlider)
                            ((HybridOption)
                                GetMenu("setting").GetOption(0)).Hybrid).Value;
                    TransScene.Present.MiniMapRenderSize =
                        ((HSlider)
                            ((HybridOption)
                                GetMenu("setting").GetOption(1)).Hybrid).Value;

                    SceneManager.LoadScene("InMaze");
                });
            };

            // Initialize all menus
            Menus = new Dictionary<string, Menu> {
                { "entrance", InitEntranceMenu () },
                { "multiplayer", InitMultiplayerMenu () },
                { "setting", InitSettingMenu () },
                { "help", InitHelpPage () },
                { "credit", InitCreditPage () },
                { "mazeList", InitMazeListMenu () },
                { "gameMode", InitGameModeMenu() }
            };

        }

        public Menu GetCurrentMenu()
        {
            return currentMenu;
        }

        public Menu GetMenu(string menuName)
        {
            if (Menus.ContainsKey(menuName))
            {
                currentMenu = Menus[menuName];
                return currentMenu;
            }
            return null;
        }

        public string GetMenuName(Menu menu)
        {
            if (Menus.ContainsValue(menu))
                return Menus.FirstOrDefault(x => x.Value == menu).Key;
            return null;
        }

        // A set of predefined transitional action for moving to next scene
        void TransitionEffects(TransitionalMove.AfterExitDelegate afterExit)
        {
            // Setup after exit event in TransitionalMove
            GameObject.Find("MenuScripts")
                .GetComponent<TransitionalMove>()
                .setAfterExitEventHandler(afterExit);

            // Menu fade out
            menuController.menuFadeOut(
                GameObject.Find("MenuScripts")
                        .GetComponent<TransitionalMove>()
                        .towardsExit,
                false
            );
        }

        IEnumerator WaitJsonDone(DoesGameModeMatch matcher)
        {
            yield return new WaitUntil(() => MazeData.present != null);

            // End selector dots rotating animation
            bkController.stopSpinning();

            // Check if game mode is supported by maze
            if (matcher == null || matcher(MazeData.present))
            {
                // Load maze data textures
                MazeData.present.LoadTextures(menuController);
                yield return new WaitUntil(() => MazeData.present.textureReady);
                // Enter maze
                DeflatedEnterMaze(null);
            }
            else
            {
                bkController.setDotsEnabled(true);
                bkController.stopBlinking();
                MazeData.present = null;
                // Reset dots indicator to its original position
                bkController.inflate(() =>
                {
                    // Show msgbox for error message
                    MessageBox.Show(
                        menuController, "Not supported gamemode with the maze");
                });
            }
        }

        void EnterMazeFromInternetJson(string mapId, DoesGameModeMatch matcher = null)
        {
            // Initiate Internet Conn object for JSON over server
            InternetConn conn = new InternetConn(InternetConn.MAP + mapId, menuController);

            // Start selector dots rotating animation
            bkController.startSpinning();

            // Perform connection
            conn.Connect((connection) =>
            {
                // If JSON string retrieved from internet successfully
                if (connection.HasResponse)
                {
                    // Remove MazeData.present
                    MazeData.present = null;
                    // Initialize universal JSON object with html from server
                    new Thread(() =>
                    {
                        new MazeData(connection.Response);
                    })
                    .Start();

                    menuController.StartCoroutine(WaitJsonDone(matcher));
                }
                // If internet exception detected
                else
                {
                    // End selector dots rotating animation
                    bkController.stopSpinning();
                    bkController.setDotsEnabled(true);
                    bkController.stopBlinking();

                    // Reset dots indicator to its original position
                    bkController.inflate(() =>
                    {
                        // Show msgbox for error message
                        MessageBox.Show(
                            menuController, "Cannot retrieve maze from main server");
                    });
                }
            });
        }

        PickerMenu PrepareLogo(PickerMenu menu)
        {
            if (CommonMenuSetting.logoSprite != null)
            {
                Logo logo = new Logo(
                                CommonMenuSetting.logoSprite,
                                CommonMenuSetting.logoWidth,
                                CommonMenuSetting.logoHeight
                            );
                menu.SetLogo(logo);
            }
            menu.Title = MenuHeading;
            return menu;
        }

        // Prepare a back option for returning to entrance menu
        Option PrepareBackOption()
        {
            return new Option("< Back", "AAFFAAFF")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Fade out current menu
                    menuController.menuFadeOut(() =>
                    {
                        // Fade in next menu
                        menuController.menuFadeIn(prevMenuStack.Pop());
                    });
                });
        }

        Option.EventHandler PrepareDeflatedNextMenu(string menuString)
        {
            return (option) =>
            {
                // Push to previous menu stack for backing
                if (currentMenu != null)
                    prevMenuStack.Push(GetMenuName(currentMenu));

                menuController.menuFadeOut(() =>
                {
                    menuController.menuFadeIn(menuString);
                });
            };
        }

        PickerMenu InitGameModeMenu()
        {
            /* * * * * * * * * * * * * * *\
             * 	     GameMode Menu       *
            \* * * * * * * * * * * * * * */

            PickerMenu gameModeMenu = PrepareLogo(new PickerMenu());

            gameModeMenu.AddElement(new Label(
                "Please select a game mode",
                Essentials.ParseColor("ADFF2FFF")));

            gameModeMenu.AddElement(new Option("Normal Mode")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Specify gamemode
                    TransScene.Present.SelectedGameMode = TransScene.GameMode.Normal;
                    EnterMazeFromInternetJson(TransScene.Present.MapId);
                })
            );

            gameModeMenu.AddElement(new Option("Time Racing Mode")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Specify gamemode
                    TransScene.Present.SelectedGameMode = TransScene.GameMode.TimeRace;
                    EnterMazeFromInternetJson(
                        TransScene.Present.MapId,
                        (md) => (PlayerPrefs.HasKey(TransScene.Present.MapId) ||
                                !(AbsServerController.IsPresent ||
                                    AbsClientController.IsPresent)) &&
                              md.escape != null
                    );
                })
            );

            Option ctfMode = new Option("Capture the Flag Mode")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Specify gamemode
                    TransScene.Present.SelectedGameMode = TransScene.GameMode.CaptureTheFlag;
                    EnterMazeFromInternetJson(
                        TransScene.Present.MapId,
                        (md) => AbsServerController.IsPresent &&
                                md.escape != null
                        );
                });

            gameModeMenu.AddElement(ctfMode);
            gameModeMenu.SetOnUpdateEventHandler((menu) =>
            {
                // If in server mode
                if (AbsServerController.IsPresent)
                    ctfMode
                        .SetHoverEventHandler(HoverEnabled)
                        .SetSelectEventHandler(SelectNext);
                else
                    ctfMode
                        .SetHoverEventHandler(HoverDisabled)
                        .SetSelectEventHandler((option) =>
                        {
                            MessageBox.Show(menuController,
                                "Only Available in Multiplayer");
                        });
            });

            gameModeMenu.AddCancelOption(PrepareBackOption());

            return gameModeMenu;
        }

        PickerMenu InitMultiplayerMenu()
        {
            /* * * * * * * * * * * * * * *\
             * 	   Multiplayer Menu      *
            \* * * * * * * * * * * * * * */

            PickerMenu multiplayerMenu = PrepareLogo(new PickerMenu());

            multiplayerMenu.AddElement(new Option("Host as Server")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Specify gamemode
                    TransScene.Present.SelectedGameMode = TransScene.GameMode.Normal;
                    // Start both server and client
                    AbsServerController.IsPresent = true;
                    AbsClientController.IsPresent = true;

                    // Select maze from maze list menu
                    PrepareDeflatedNextMenu("mazeList").Invoke(null);
                })
            );

            multiplayerMenu.AddElement(new Option("Join as Client")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Start selector dots rotating animation
                    bkController.startSpinning();

                    // Begin handshaking with possible UServer running
                    UHandshake handshake = new UHandshake(
                        Essentials.GetBroadcastAddress(
                        Essentials.GetCurrentIP(), Essentials.GetSubnetMask())
                        .ToString(),
                        3,
                        1000
                    );
                    new Thread(() =>
                    {
                        handshake.start();
                    }).Start();
                    menuController.StartCoroutine(WaitTillShakedHand(handshake));
                })
            );

            multiplayerMenu.AddCancelOption(PrepareBackOption());

            return multiplayerMenu;
        }

        IEnumerator WaitTillShakedHand(UHandshake handshake)
        {
            // Wait until handshake has result
            yield return new WaitUntil(() => handshake.IsDone);
            // Stop handshake socket
            handshake.stop();

            if (handshake.MapId != null)
            {
                // Specify gamemode
                TransScene.Present.SelectedGameMode = handshake.GameMode;
                // Start client
                AbsClientController.IsPresent = true;
                AbsServerController.IsPresent = false;

                // Update map id reference in TransScene
                TransScene.Present.MapId = handshake.MapId;
                EnterMazeFromInternetJson(TransScene.Present.MapId);
            }
            else
            {
                MessageBox.Show(menuController, "Cannot reach LAN server");
                bkController.stopSpinning();
                bkController.setDotsEnabled(true);
                bkController.stopBlinking();
                bkController.inflate();
            }
        }

        GalleryMenu InitCreditPage()
        {
            /* * * * * * * * * * * * * * *\
             * 		  Credit Page        *
            \* * * * * * * * * * * * * * */

            GalleryMenu creditPage = new GalleryMenu(CreditPageSetting)
            {
                Title = "Special thanks to"
            };

            // Unity Chan 
            // Unity Chan Character Use Guidelines - Version 1.01
            creditPage.addLogo(new Logo(
                Resources.Load<Sprite>("License Logo/UnityChan_Light_Frame")
            ));

            // Icon8
            // Creative Commons Attribution - NoDerivs 3.0 Unported
            creditPage.addLogo(new Logo(
                Resources.Load<Sprite>("License Logo/Icons8_Logo")
            ));

            creditPage.addNewRow();

            // Google Cardboard
            // Apache License version 2.0
            creditPage.addLogo(new Logo(
                Resources.Load<Sprite>("License Logo/Google_Cardboard_logo")
            ));

            // ZXing.Net, a CSharp port of project ZXing
            // Apache License version 2.0
            creditPage.addLogo(new Logo(
                Resources.Load<Sprite>("License Logo/ZXing_logo")
            ));

            creditPage.AddCancelOption(PrepareBackOption());

            return creditPage;
        }

        GalleryMenu InitHelpPage()
        {
            /* * * * * * * * * * * * * * *\
             * 	     Tutorial Page       *
            \* * * * * * * * * * * * * * */

            GalleryMenu helpPage = new GalleryMenu(CreditPageSetting)
            {
                Title = "Controller Layout"
            };

            // Image of controller layout
            helpPage.addLogo(new Logo(
                Resources.Load<Sprite>("Tutorial/full_controller"),
                /* Width */100, /* Width x Ratio */100 * 1529 / 2264f
            ));

            helpPage.AddCancelOption(PrepareBackOption());

            return helpPage;
        }

        PickerMenu InitEntranceMenu()
        {
            /* * * * * * * * * * * * * * *\
		     * 		 Entrance Menu       *
		    \* * * * * * * * * * * * * * */

            PickerMenu entranceMenu = PrepareLogo(new PickerMenu());

            entranceMenu.AddElement(new NetworkOption("Single Player")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    // Specify gamemode
                    TransScene.Present.SelectedGameMode = TransScene.GameMode.Normal;
                    // Neither server nor client will be started
                    AbsServerController.IsPresent = false;
                    AbsClientController.IsPresent = false;

                    // Select maze from maze list menu
                    PrepareDeflatedNextMenu("mazeList").Invoke(null);
                })
            );

            entranceMenu.AddElement(new NetworkOption("Multi Player")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler(PrepareDeflatedNextMenu("multiplayer"))
            );

            entranceMenu.AddElement(new Option("Options")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler(PrepareDeflatedNextMenu("setting"))
            );

            entranceMenu.AddElement(new Option("Help")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler(PrepareDeflatedNextMenu("help"))
            );

            entranceMenu.AddElement(new Option("Exit", "FFAAAAFF")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler((option) =>
                {
                    Application.Quit();
                })
            );

            return entranceMenu;
        }

        MassiveMenu InitMazeListMenu()
        {
            /* * * * * * * * * * * * * * *\
             *       Maze List Menu      *
            \* * * * * * * * * * * * * * */

            // For debugging purpose
            if (MazeList.present == null)
                new MazeList(MazeList.SAMPLE);

            MassiveMenu mazeListMenu = (MassiveMenu)PrepareLogo(
                                           new MassiveMenu(MazeListSetting)
                                       );

            // Set number of items to be displayed per page
            mazeListMenu.ItemsPerPage = MazeListSetting.ItemsPerPage;

            mazeListMenu.AddElement(new Label("Please select a maze"));

            // All options based on fetched maze list
            foreach (MazeList.MazeMeta meta in MazeList.present.List)
            {
                mazeListMenu.AddElement(new ListOption("Title: " + meta.Title)
                    .SetDescription("Timestamp: " + meta.CreateTime)
					.SetReference(meta)
                    .SetLeftEventHandler((option) =>
                    {
                        // Return page
                        if (mazeListMenu.RollBackPage())
                        {
                            // Re-render next page options
                            menuController.ReRenderOptions(
                                mazeListMenu,
                                mazeListMenu.NumPreNonListOption,
                                mazeListMenu.GetElementsCount() -
                                mazeListMenu.NumPostNonListOption,
                                "ListOption"
                            );
                        }
                    })
                    .SetRightEventHandler((option) =>
                    {
                        // Flip page
                        if (mazeListMenu.MoveNextPage())
                        {
                            // Re-render next page options
                            menuController.ReRenderOptions(
                                mazeListMenu,
                                mazeListMenu.NumPreNonListOption,
                                mazeListMenu.GetElementsCount() -
                                mazeListMenu.NumPostNonListOption,
                                "ListOption"
                            );
                        }
                    })
                    .SetHoverEventHandler(HoverEnabled)
                    .SetSelectEventHandler(SelectNext)
                    .SetDeflatedEventHandler((option) =>
                    {
                        // Update map id reference in TransScene
						TransScene.Present.MapId = ((ListOption)option).Reference.Id;
                        // Select game mode
                        PrepareDeflatedNextMenu("gameMode").Invoke(null);
                    })
                );
            }

            Option backOrigin = PrepareBackOption();

            mazeListMenu.AddCancelOption(new InteractiveOption(PrepareBackOption())
                .SetLeftEventHandler((option) =>
                {
                    if (option.Text != "Enter /maps/0")
                    {
                        // Allow to enter test map id 0
                        option.GameText.text = "Enter /maps/0";
                        option.GameText.color = Color.yellow;
                        // Resize
                        option.ResizeRect();
                        option.SetDeflatedEventHandler((optn) =>
                        {
                            // Update map id reference in TransScene
                            TransScene.Present.MapId = "0";
                            // Select game mode
                            PrepareDeflatedNextMenu("gameMode").Invoke(null);
                            // Reset delegate
                            option.SetDeflatedEventHandler((o) => { backOrigin.OnDeflated(); });
                        });
                    }
                })
                .SetRightEventHandler((option) =>
                {
                    if (option.Text != "< Back")
                    {
                        option.GameText.text = backOrigin.Text;
                        option.GameText.color = backOrigin.TextColor;
                        // Resize
                        option.ResizeRect();
                        option.SetDeflatedEventHandler((optn) =>
                        {
                            mazeListMenu.RollBackPage(true);
                            // Original deflated delegation
                            backOrigin.OnDeflated();
                        });
                    }
                })
                .SetDeflatedEventHandler((option) =>
                {
                    mazeListMenu.RollBackPage(true);
                    // Original deflated delegation
                    backOrigin.OnDeflated();
                })
            );

            return mazeListMenu;
        }

        HybridMenu InitSettingMenu()
        {
            /* * * * * * * * * * * * * * *\
             * 		  Option Menu        *
            \* * * * * * * * * * * * * * */
            HybridMenu settingMenu = (HybridMenu)PrepareLogo(new HybridMenu());

            HSlider mazeRenderSize = new HSlider(
                                         Resources.Load<GameObject>("prefab/slider"),
                                         1f,
                                         10f
                                     ).SetValue(
                                         // Default value
                                         TransScene.Present.MazeRenderSize == 0 ? 7f :
                                         TransScene.Present.MazeRenderSize
                                     );
            settingMenu.AddElement(new HybridOption("Render Size:")
                .SetReference(mazeRenderSize)
                .SetLeftEventHandler((option) =>
                {
                    mazeRenderSize.MinusValue(0.5f);
                })
                .SetRightEventHandler((option) =>
                {
                    mazeRenderSize.AddValue(0.5f);
                })
            );

            HSlider miniMapScale = new HSlider(
                                       Resources.Load<GameObject>("prefab/slider"),
                                       1f,
                                       7f
                                   ).SetValue(
                                       // Default value
                                       TransScene.Present.MiniMapRenderSize == 0 ? 2f :
                                       TransScene.Present.MiniMapRenderSize
                                   );
            settingMenu.AddElement(new HybridOption("MiniMap Scale:")
                .SetReference(miniMapScale)
                .SetLeftEventHandler((option) =>
                {
                    miniMapScale.MinusValue(0.5f);
                })
                .SetRightEventHandler((option) =>
                {
                    miniMapScale.AddValue(0.5f);
                })
            );

            settingMenu.AddElement(new Option("Credit")
                .SetSelectEventHandler(SelectNext)
                .SetHoverEventHandler(HoverEnabled)
                .SetDeflatedEventHandler(PrepareDeflatedNextMenu("credit"))
            );

            settingMenu.AddCancelOption(PrepareBackOption());

            return settingMenu;
        }
    }
}

