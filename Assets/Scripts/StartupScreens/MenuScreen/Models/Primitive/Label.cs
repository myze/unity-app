using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
	public class Label
	{
		public Color TextColor;
		public string Text;

		public Text GameText { private set; get; }

		public Label ()
		{
		}

		public Label (string text)
		{
			Text = text;
			TextColor = Color.white;
		}

		public Label (string text, Color textColor) : this (text)
		{
			TextColor = textColor;
		}

		public void ResizeRect ()
		{
			if (GameText != null) {
				RectTransform rt = GameText.GetComponent<RectTransform> ();
				rt.sizeDelta = new Vector2 (
					Essentials.GetStringWidth (
						GameText.text,
						GameText.fontSize,
						GameText.font,
						GameText.fontStyle
					) + 5f,
					Essentials.GetStringHeight (
						GameText.text, 
						GameText.fontSize
					)
				);
			}
		}

		public void SetGameText (Text text)
		{
			GameText = text;
		}
	}
}
