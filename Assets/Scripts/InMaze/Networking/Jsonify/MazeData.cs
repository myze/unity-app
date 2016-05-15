using System;
using lz_string_csharp;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.InMaze.Networking.Jsonify
{
	public class MazeData
	{
		public class Coordinate
		{
			// Default no scale
			public static float Scale = 1f;

			public float X;
			public float Y;

			public void ScaleUp ()
			{
				X *= Scale;
				Y *= Scale;
			}

			public void ScaleDown ()
			{
				X /= Scale;
				Y /= Scale;
			}

			public void InvertY ()
			{
				Y *= -1;
			}
		}

		public class CoordinateDesc : Coordinate
		{
			public string Description;
		}

		//Universal Json
		public static MazeData present;

		//Will be removed with server JSON extraction
		public const string EMPTY =
			"{" +
			"\"config\":" +
			"{" +
			"\"lighting\":8," +
			"\"spawn\":[0,0]," +
			"\"escape\":[0,-100]," +
			"\"facing\":0" +
			"}," +
			"\"texture\":" +
			"{" +
			"\"ground\":null," +
			"\"wall\":null," +
			"\"ceiling\":null" +
			"}," +
			"\"signs\":[]," +
			"\"coordinates\":[]," +
			"\"timestamp\":0000000000" +
			"}";

		private readonly JSONClass rootObject;
		private MonoBehaviour mono;

		// Getters / Setters
		public Coordinate[][] coordinates { private set; get; }

		public int lighting { private set; get; }

		public Coordinate spawn { private set; get; }

		public Coordinate escape { private set; get; }

		public int facing { private set; get; }

		private Dictionary<string, Texture> textures;
		private readonly string[] TEXTURE_NAME = { "ground", "wall", "ceiling" };

		public Texture groundTexture { get { return textures [TEXTURE_NAME [0]]; } }

		public Texture wallTexture { get { return textures [TEXTURE_NAME [1]]; } }

		public Texture ceilingTexture { get { return textures [TEXTURE_NAME [2]]; } }

		public bool textureReady {
			get {
				bool retn = true;
				for (int i = 0; i < TEXTURE_NAME.Length && retn; i++)
					retn = textures.ContainsKey (TEXTURE_NAME [i]);
				return retn;
			}
		}

		public CoordinateDesc[] signs { private set; get; }

		public int timestamp { private set; get; }

		public MazeData (string encodedJSON)
		{
			textures = new Dictionary<string, Texture> ();
			// Decode compressed json
			string json = LZString.decompressFromUTF16 (encodedJSON);
			// If not encoded
			if (JSON.Parse (json) == null)
				json = encodedJSON;
			// Case of invalid maze
			if (!UDP.UDP.LOG_MUTE) {
				if (json == null || JSON.Parse (json) == null) {
					json = EMPTY;
					Debug.LogWarning ("MazeData: Using empty maze");
				}
				Debug.Log ("MazeData: " + json);
			}
			rootObject = JSON.Parse (json).AsObject;

			// Coordinate related settings, default scale 1f
			SetCoordsScale ();

			// Config
			lighting = GetLighting (GetConfig (rootObject));
			facing = GetFacing (GetConfig (rootObject));
			// Timestamp
			timestamp = GetTimestamp (rootObject);

			present = this;
		}

		// Since InternetConn can only be called in main thread
		// A seperated method for loading textures is created
		public void LoadTextures (MonoBehaviour mono)
		{
			this.mono = mono;
			// Texture
			foreach (string s in TEXTURE_NAME)
				GetTextureData (GetTexture (rootObject), s);
		}

		public void SetCoordsScale (float scale)
		{
			Coordinate.Scale = scale;
			SetCoordsScale ();
		}

		public void SetCoordsScale ()
		{
			coordinates = GetCoordinates (rootObject);
			spawn = GetSpawn (GetConfig (rootObject));
			escape = GetEscape (GetConfig (rootObject));
			signs = GetSigns (rootObject);
		}

		private CoordinateDesc[] GetSigns (JSONClass jsonObject)
		{
			LinkedList<CoordinateDesc> signs = new LinkedList<CoordinateDesc> ();
			JSONArray jSigns = jsonObject ["signs"].AsArray;
			foreach (JSONArray node in jSigns) {
				CoordinateDesc c = new CoordinateDesc () {
					X = node [0].AsInt,
					Y = node [1].AsInt,
					Description = node [2]
				};
				c.ScaleUp ();
				c.InvertY ();
				signs.AddLast (c);
			}
			return signs.ToArray ();
		}

		private JSONClass GetConfig (JSONClass jsonObject)
		{
			return jsonObject ["config"].AsObject;
		}

		private int GetLighting (JSONClass config)
		{
			return config ["lighting"].AsInt;
		}

		private Coordinate GetSpawn (JSONClass config)
		{
			Coordinate spawn = new Coordinate () {
				X = config ["spawn"].AsArray [0].AsInt,
				Y = config ["spawn"].AsArray [1].AsInt
			};
			spawn.ScaleUp ();
			spawn.InvertY ();
			return spawn;
		}

		private Coordinate GetEscape (JSONClass config)
		{
			if (config ["escape"].ToString ().ToLower () != "\"null\"") {
				Coordinate escape = new Coordinate () {
					X = config ["escape"].AsArray [0].AsInt,
					Y = config ["escape"].AsArray [1].AsInt
				};
				escape.ScaleUp ();
				escape.InvertY ();
				return escape;
			}
			return null;
		}

		private int GetFacing (JSONClass config)
		{
			return config ["facing"].AsInt;
		}

		private Coordinate[][] GetCoordinates (JSONClass jsonObject)
		{
			LinkedList<Coordinate[]> coords = new LinkedList<Coordinate[]> ();
			JSONArray preCoords = jsonObject ["coordinates"].AsArray;

			for (int i = 0; i < preCoords.Count; i++) {
				for (int j = 0; j < preCoords [i].Count - 1; j++) {
					Coordinate[] coordsLv2 = new Coordinate[2];

					for (int k = 0; k < 2; k++) {
						Coordinate coordsLv3 = new Coordinate () {
							X = preCoords [i] [j + k] [0].AsInt,
							Y = preCoords [i] [j + k] [1].AsInt
						};
						coordsLv3.ScaleUp ();
						coordsLv3.InvertY ();
						coordsLv2 [k] = coordsLv3;
					}

					coords.AddLast (coordsLv2);
				}
			}

			return coords.ToArray ();
		}

		private JSONClass GetTexture (JSONClass jsonObject)
		{
			return jsonObject ["texture"].AsObject;
		}

		private void GetTextureData (JSONClass jsonObject, string key)
		{
			if (jsonObject [key].ToString ().ToLower () == "\"null\"") {
				textures.Add (key, null);
				return;
			}
			// Get Texture behind
			InternetConn conn =
				new InternetConn (InternetConn.WEBSITE + jsonObject [key], mono);
			// Update dictionary
			conn.Connect (() => {
				textures.Add (key, conn.Texture);
			});
		}

		private int GetTimestamp (JSONClass jsonObject)
		{
			return jsonObject ["timestamp"].AsInt;
		}
	}
}
