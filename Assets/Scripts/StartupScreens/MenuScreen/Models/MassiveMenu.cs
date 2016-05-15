using System.Collections;
using Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using UnityEngine;

namespace Assets.Scripts.StartupScreens.MenuScreen.Models
{
	public class MassiveMenu : PickerMenu, IEmbeddedSetting
	{
		public int NumPreNonListOption, NumPostNonListOption;
		public int NumListOption;
		private readonly MassiveMenuSetting menuSetting;

		public int ItemsPerPage;

		public int PageIndex { private set; get; }

		public MassiveMenu (MassiveMenuSetting menuSetting)
		{
			this.menuSetting = menuSetting;
		}

		private void CalcDistribution ()
		{
			// Reset variables
			NumPostNonListOption = NumPreNonListOption = NumListOption = 0;

			bool isPre = true;
			for (int i = 0; i < base.GetElementsCount (); i++) {
				// Count the total number of non-ListOption / ListOption
				if (!(base.GetElement (i) is ListOption)) {
					// Count number of options before ListOption(s)
					if (isPre)
						NumPreNonListOption++;
                    // Count number of options after ListOption(s)
                    else
						NumPostNonListOption++;
				} else {
					// Count ListOption
					NumListOption++;
					// Flip non-ListOption
					isPre = false;
				}
			}
		}

		public bool MoveNextPage (bool toLastPage = false)
		{
			if (!toLastPage) {
				if ((PageIndex + 1) * ItemsPerPage < NumListOption) {
					PageIndex++;
					return true;
				}
				return false;
			}
			PageIndex = NumListOption / ItemsPerPage;
			return true;
		}

		public bool RollBackPage (bool toFirstPage = false)
		{
			if (!toFirstPage) {
				if (PageIndex > 0) {
					PageIndex--;
					return true;
				}
				return false;
			}
			PageIndex = 0;
			return true;
		}

		public MenuSetting.MenuSetting getMenuSetting ()
		{
			return menuSetting;
		}

		public override void AddElement (Label label)
		{
			base.AddElement (label);
			CalcDistribution ();
		}

		public override Label RemoveElement (int index)
		{
			Label label = base.RemoveElement (index);
			CalcDistribution ();
			return label;
		}

		public override Option GetOption (int index)
		{
			// For Non-listOption case
			if (index < NumPreNonListOption)
				return base.GetOption (index);
			if (index >= GetElementsCount () - NumPostNonListOption) {
				// Correction of wrong postfix options
				if (index >= GetElementsCount ()) {
					index -= ItemsPerPage - NumListOption % ItemsPerPage;
				}
				return base.GetOption (
					NumPreNonListOption +
					NumListOption +
					NumPostNonListOption - (GetElementsCount () - index));
			}
			// For ListOption case
			return base.GetOption (
				NumPreNonListOption - 1 + index + PageIndex * ItemsPerPage);
		}

		public override Label GetElement (int index)
		{
			// For Non-listOption case
			if (index < NumPreNonListOption)
				return base.GetElement (index);
			if (index >= GetElementsCount () - NumPostNonListOption) {
				// Correction of wrong postfix options
				if (index >= GetElementsCount ()) {
					index -= ItemsPerPage - NumListOption % ItemsPerPage;
				}
				return base.GetElement (
					NumPreNonListOption +
					NumListOption +
					NumPostNonListOption - (GetElementsCount () - index));
			}
			// For ListOption case
			return base.GetElement (
				NumPreNonListOption - 1 + index * (PageIndex + 1));
		}

		public override int GetElementsCount ()
		{
			int numNonListOption = NumPostNonListOption + NumPreNonListOption;
			// Check if last page
			if ((PageIndex + 1) * ItemsPerPage > NumListOption)
				return numNonListOption + NumListOption - PageIndex * ItemsPerPage;
			return numNonListOption + ItemsPerPage;
		}
	}
}
