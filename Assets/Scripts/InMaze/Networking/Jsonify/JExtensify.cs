using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SimpleJSON;

namespace Assets.Scripts.InMaze.Networking.Jsonify
{
    // Extension node for JExtensified JSON
    public abstract class JExtNode : IJsonify
    {
        public int TargetId { private get; set; }

        protected JExtNode() { TargetId = -1; }

        public bool ShouldAdd(int id)
        {
            return TargetId == -1 || id == TargetId;
        }

        public abstract JSONClass ToJson();
        // Use to initialize all parameters
        public abstract void Init(JSONClass json);

        public override string ToString()
        {
            return ToJson().ToString();
        }
    }

    // For extending the behavior of JSON to support custom gamemodes
    public class JExtensify
    {
        private readonly LinkedList<JExtNode> _ext =
            new LinkedList<JExtNode>();

        public JExtNode[] Ext { get { return _ext.ToArray(); } }

        public JExtensify() { }

        public JExtensify(JSONClass json)
        {
            if (json["Ext"] != null)
            {
                for (int i = 0; i < json["Ext"].Count; i++)
                {
                    JExtNode node = (JExtNode)Activator.CreateInstance(
                        // Class type
                        null,
                        GetType().Namespace + ".Extension." +
                        json["Ext"][i]["Name"]
                    ).Unwrap();
                    // Initialize all parameters
                    node.Init((JSONClass)json["Ext"][i]);
                    _ext.AddLast(node);
                }
            }
        }

        public void AddExt(JExtNode extension)
        {
            _ext.AddLast(extension);
        }

        public bool RemoveExt(int index)
        {
            return _ext.Remove(Ext[index]);
        }

        public bool ContainsExt(Type type)
        {
            return Ext.Any(ij => ij.GetType() == type);
        }

        public bool ContainsExt(JExtNode jsonNode)
        {
            return Ext.Any(ij => ij == jsonNode);
        }

        protected void AddExtToJson(JSONClass root, int id = -1)
        {
            JSONArray jArr = new JSONArray();
            foreach (JExtNode node in Ext)
                if (node.ShouldAdd(id))
                    jArr.Add(node.ToJson());
            if (jArr.Count > 0)
                root.Add("Ext", jArr);
        }
    }
}
