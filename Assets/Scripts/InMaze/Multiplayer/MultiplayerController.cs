using System;
using System.Linq;
using System.Reflection;
using Assets.Scripts.InMaze.Controllers;
using Assets.Scripts.InMaze.Multiplayer.Normal;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.InMaze.Networking.Jsonify.Extension;
using UnityEngine;

namespace Assets.Scripts.InMaze.Multiplayer
{
    public class MultiplayerController : MonoBehaviour
    {
        void Awake()
        {
            // If in single player mode
            if (!AbsServerController.IsPresent && !AbsClientController.IsPresent)
                // Remove MultiplayerScripts instantly before drop of frame
                DestroyImmediate(gameObject);
            else
            {
                // Reset Jsonified references
                PlayerNode.present = null;
                PlayerNodes.present = null;
                Spectator.Present = null;
                EscapeSeq.Present = null;

                if (AbsServerController.IsPresent)
                    AddRoleController("ServerController");
                if (AbsClientController.IsPresent)
                    AddRoleController("ClientController");

                // Remove MultiplayerController
                Destroy(this);
            }
        }

        // Add specific gamemode server/client controller
        void AddRoleController(string className)
        {
            string _namespace = "Assets.Scripts.InMaze.Multiplayer." +
                TransScene.Present.SelectedGameMode;
            Type type = (from t in Assembly.GetExecutingAssembly().GetTypes()
                          where t.IsClass &&
                                t.Namespace == _namespace &&
                                t.Name == className
                          select t).ToList().First();
            gameObject.AddComponent(type);
        }
    }
}
