using System.Collections;
using System.Linq;
using SimpleJSON;

namespace Assets.Scripts.InMaze.Networking.Jsonify
{
    public class PlayerNodes : JExtensify, IJsonify
    {
        public static PlayerNodes present;
        public static PlayerNodes serverSide;

        private readonly ArrayList players = new ArrayList();

        public int Count
        {
            get { return players.Count; }
        }

        public PlayerNodes() { }

        public PlayerNodes(JSONClass json) : base(json) { }

        public void AddPlayer(PlayerNode player)
        {
            players.Add(player);
        }

        public void RemovePlayer(int id)
        {
            foreach (PlayerNode p in players)
                if (p.id == id)
                {
                    players.Remove(p);
                    break;
                }
        }

        public void AlterPlayer(int id, PlayerNode altered)
        {
            RemovePlayer(id);
            players.Add(altered);
        }

        public PlayerNode GetPlayer(int id)
        {
            return players.Cast<PlayerNode>().FirstOrDefault(p => p.id == id);
        }

        public PlayerNode GetFirstPlayer()
        {
            return players.Cast<PlayerNode>().FirstOrDefault();
        }

        public PlayerNode[] GetPlayers()
        {
			return players.Cast<PlayerNode> ().ToArray ();
        }

        public override string ToString()
        {
            return ToString(-1);
        }

        public string ToString(int id)
        {
            JSONClass root = new JSONClass();
            JSONArray array = new JSONArray();
            object[] players = this.players.ToArray();
            
            foreach (object t in players)
            {
                JSONClass player = ((PlayerNode) t).ToJson();
                // Erase child Ext
                player.Remove("Ext");
                array.Add(player);
            }

            root.Add("Players", array);

            AddExtToJson(root, id);

            return root.ToString();
        }

        public JSONClass ToJson()
        {
            return (JSONClass)JSON.Parse(ToString());
        }

        public static PlayerNodes Parse(JSONClass json)
        {
            PlayerNodes players = new PlayerNodes(json);
            JSONArray array = json["Players"].AsArray;

            for (int i = 0; i < array.Count; i++)
                players.AddPlayer(
                    PlayerNode.Parse((JSONClass)array[i])
                );

            return players;
        }
    }
}
