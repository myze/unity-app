using Assets.Scripts.InMaze.Networking.Jsonify;
using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
	public class ListOption : InteractiveOption
	{
		public string Description { private set; get; }

		public string Title { private set; get; }

		public MazeList.MazeMeta Reference { private set; get; }

		public Sprite Thumbnail { private set; get; }

		public ListOption (string title) : base ("")
		{ 
			Title = title;
		}

		public ListOption SetDescription (string description)
		{
			Description = description;
			return this;
		}

		public ListOption SetThumbnail (Sprite sprite)
		{
			Thumbnail = sprite;
			return this;
		}

		public ListOption SetReference (MazeList.MazeMeta meta) {
			Reference = meta;
			return this;
		}
	}
}
