using System.Collections;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive
{
	public class Menu
	{
		private readonly ArrayList elements;
		private OnUpdateEvent updateEvent;
		public string Title;

		// For frame updating
		public delegate void OnUpdateEvent (Menu menu);

		// Click when cancel button is pressed on menu
		protected Option CancelOption;

		public Menu ()
		{
			elements = new ArrayList ();
			updateEvent = (menu) => {
			};
		}

		public void OnCancel ()
		{
			if (CancelOption != null)
				CancelOption.OnDeflated ();
		}

		public void OnUpdate ()
		{
			updateEvent (this);
		}

		public void AddCancelOption (Option option)
		{
			AddElement (option);
			CancelOption = option;
		}

		public virtual void AddElement (Label label)
		{
			elements.Add (label);
		}

		public virtual Option GetOption (int index)
		{
			Label element = _GetElement (index);
			var option = element as Option;
			return option;
		}

		private Label _GetElement (int index)
		{
			if (index >= 0 && index < elements.Count)
				return ((Label)elements [index]);
			return null;
		}

		public virtual Label GetElement (int index)
		{
			return _GetElement (index);
		}

		public Label[] GetElements ()
		{
			return (Label[])elements.ToArray (typeof(Label));
		}

		public virtual int GetElementsCount ()
		{
			return elements.Count;
		}

		public virtual Label RemoveElement (int index)
		{
			Label retn = GetElement (index);
			if (retn != null) {
				elements.RemoveAt (index);
				if (retn == CancelOption)
					CancelOption = null;
			}
			return retn;
		}

		public Menu SetOnUpdateEventHandler (OnUpdateEvent e)
		{
			updateEvent = e;
			return this;
		}
	}
}
