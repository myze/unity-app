using SimpleJSON;

namespace Assets.Scripts.InMaze.Networking.Jsonify.Extension
{
    public class FlagHolder : JExtNode
    {
        public static FlagHolder Present;

        public int Id;

        public override JSONClass ToJson()
        {
            return new JSONClass()
            {
                { "Name", new JSONData("FlagHolder") },
                { "Id", new JSONData(Id) }
            };
        }

        public override void Init(JSONClass json)
        {
            Id = json["Id"].AsInt;
            Present = this;
        }
    }
}
