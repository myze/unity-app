using SimpleJSON;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking.Jsonify.Extension
{
    public class ServerEscTime : JExtNode
    {
        // For reserving
        public static ServerEscTime Present;

        public int BestTime;

        public override JSONClass ToJson()
        {
            return new JSONClass()
            {
                { "Name", new JSONData("ServerEscTime") },
                { "BestTime", new JSONData(BestTime) }
            };
        }

        public override void Init(JSONClass json)
        {
            BestTime = json["BestTime"].AsInt;
            Present = this;
        }
    }
}
