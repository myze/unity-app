using SimpleJSON;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking.Jsonify
{
    public class PlayerNode : JExtensify, IJsonify
    {
        public static PlayerNode present;
        public int id = -1;

        public Vector3 position, eulerAngles;
        public bool jump;
        public float horizontal, vertical;
        public Color color;

        public PlayerNode() { }

        public PlayerNode(JSONClass json) : base(json) { }

        public static PlayerNode Parse(JSONClass json)
        {
            // To Unity App base
            MazeData.Coordinate coords = new MazeData.Coordinate()
            {
                X = json["Position"]["X"].AsFloat,
                Y = json["Position"]["Z"].AsFloat
            };
            coords.ScaleUp();
            coords.InvertY();

            return new PlayerNode(json)
            {
                id = json["Id"].AsInt,
                position = new Vector3(
                    coords.X,
                    json["Position"]["Y"].AsFloat,
                    coords.Y
                ),
                eulerAngles = new Vector3(
                    json["EulerAngles"]["X"].AsFloat,
                    json["EulerAngles"]["Y"].AsFloat,
                    json["EulerAngles"]["Z"].AsFloat
                ),
                color = (json["Color"] != null) ?
                    Essentials.ParseColor(json["Color"]) : new Color(),
                jump = json["Jump"].AsBool,
                horizontal = json["Horizontal"].AsFloat,
                vertical = json["Vertical"].AsFloat
            };
        }

        public override string ToString()
        {
            JSONClass root = ToJson();
            root.Add("IP", new JSONData(
                Essentials.GetCurrentIP().ToString()
            ));
            return root.ToString();
        }

        public JSONClass ToJson()
        {
            JSONClass root = new JSONClass();
            JSONClass position = new JSONClass();
            JSONClass eulerAngles = new JSONClass();

            // Reset coordinates to Web Editor original base
            MazeData.Coordinate coords = new MazeData.Coordinate()
            {
                X = this.position.x,
                Y = this.position.z
            };
            coords.ScaleDown();
            coords.InvertY();

            position.Add("X", new JSONData(coords.X));
            position.Add("Y", new JSONData(this.position.y));
            position.Add("Z", new JSONData(coords.Y));

            eulerAngles.Add("X", new JSONData(this.eulerAngles.x));
            eulerAngles.Add("Y", new JSONData(this.eulerAngles.y));
            eulerAngles.Add("Z", new JSONData(this.eulerAngles.z));

            root.Add("Id", new JSONData(id));
            root.Add("Position", position);
            root.Add("EulerAngles", eulerAngles);
            root.Add("Jump", new JSONData(this.jump));
            root.Add("Horizontal", new JSONData(this.horizontal));
            root.Add("Vertical", new JSONData(this.vertical));

            AddExtToJson(root);

            return root;
        }
    }
}
