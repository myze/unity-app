using Assets.Scripts.InMaze.Networking;
using Assets.Scripts.InMaze.Networking.Jsonify;

namespace Assets.Scripts.InMaze.Multiplayer
{
    public interface IMultiplayer
    {
        // Trigger when a new player joined
        void Join(PlayerNode p);
        // Trigger when a player quitted
        void Quit(PlayerNode p);
    }
}
