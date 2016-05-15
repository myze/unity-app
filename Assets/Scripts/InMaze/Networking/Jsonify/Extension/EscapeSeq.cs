using SimpleJSON;

namespace Assets.Scripts.InMaze.Networking.Jsonify.Extension
{
    public class EscapeSeq : JExtNode
    {
        // For reserving
        public static EscapeSeq Present;

        public int Place;

        public override JSONClass ToJson()
        {
            JSONClass root = new JSONClass()
            {
                { "Name", new JSONData("EscapeSeq") },
                { "Place", new JSONData(Place) }
            };
            return root;
        }

        public override void Init(JSONClass json)
        {
            Place = json["Place"].AsInt;
            Present = this;
        }
    }
}
