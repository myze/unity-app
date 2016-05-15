using Assets.Scripts.InputMethods.Joysticks;

namespace Assets.Scripts
{
    public class TransScene
    {
        private static TransScene _present = new TransScene();

        public static TransScene Present
        {
            get
            {
                return _present;
            }
        }

        // Default controller -- Keyboard
        public JoystickController Controller = JoystickController.GetJoystick("Keyboard");

        public float MazeRenderSize = 0;
        public float MiniMapRenderSize = 0;
        public bool IsNoseShown;

        public TokenKey LoginToken;
        public string MapId;

        public enum GameMode { Normal, CaptureTheFlag, TimeRace }

        // Default gamemode normal
        public GameMode SelectedGameMode = GameMode.Normal;
    }
}
