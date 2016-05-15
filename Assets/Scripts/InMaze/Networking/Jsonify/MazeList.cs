using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Xml;

namespace Assets.Scripts.InMaze.Networking.Jsonify
{
    public class MazeList
    {
        public class MazeMeta
        {
            public string Id { private set; get; }

            public string Title { private set; get; }

            public DateTime CreateTime { private set; get; }

            internal MazeMeta(JSONClass meta)
            {
                Id = GetId(meta);
                Title = GetTitle(meta);
                CreateTime = GetCreateTime(meta);
            }

            private string GetId(JSONClass node)
            {
                return node["id"];
            }

            private string GetTitle(JSONClass node)
            {
                return node["title"];
            }

            private DateTime GetCreateTime(JSONClass node)
            {
                return XmlConvert.ToDateTime(node["date"],
                    XmlDateTimeSerializationMode.Utc);
            }
        }

        public static MazeList present;
        public const string EMPTY = "[]";
        public const string SAMPLE = "[" +
                                         "{\"id\":\"404\",\"title\":\"壹\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"貳\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"仨\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"肆\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"伍\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"陸\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"柒\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"捌\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"玖\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"拾\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"11\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"12\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"13\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"14\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"15\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"16\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"17\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"18\",\"date\":\"2016-02-18T11:59:58.000Z\"}," +
                                         "{\"id\":\"404\",\"title\":\"19\",\"date\":\"2016-02-18T11:59:58.000Z\"}" +
                                     "]";

        private readonly JSONArray rootArray;

        public MazeMeta[] List { private set; get; }

        public MazeList(string list)
        {
			if (!UDP.UDP.LOG_MUTE) {
				// Case of invalid list
				if (list == null || JSON.Parse (list) == null) {
					list = EMPTY;
					Debug.LogWarning ("MazeList: Using empty list");
				}
				Debug.Log ("MazeList: " + list);
			}
            rootArray = JSON.Parse(list).AsArray;

            List = GetList(rootArray);

            present = this;
        }

        private MazeMeta[] GetList(JSONArray jsonArray)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < jsonArray.Count; i++)
            {
                list.Add(new MazeMeta(jsonArray[i].AsObject));
            }
            return (MazeMeta[])list.ToArray(typeof(MazeMeta));
        }
    }
}
