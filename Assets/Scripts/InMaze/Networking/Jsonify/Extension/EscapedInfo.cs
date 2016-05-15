using System;
using SimpleJSON;

namespace Assets.Scripts.InMaze.Networking.Jsonify.Extension
{
    public class EscapedInfo : JExtNode
    {
        public override JSONClass ToJson()
        {
            JSONClass root = new JSONClass
            {
                {"Name", new JSONData("EscapedInfo")},
                {"Time", new JSONData(
                    Essentials.GetTimestamp()
                )}
            };
            return root;
        }

        public override void Init(JSONClass json) { }
    }
}
