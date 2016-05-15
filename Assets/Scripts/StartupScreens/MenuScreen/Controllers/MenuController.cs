using System.Collections;
using System.Threading;
using Assets.Scripts.StartupScreens.MenuScreen.Models;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Hybrid;
using Assets.Scripts.StartupScreens.MenuScreen.Models.MenuSetting;
using Assets.Scripts.StartupScreens.MenuScreen.Models.Primitive;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

namespace Assets.Scripts.StartupScreens.MenuScreen.Controllers
{
	public class MenuController : MonoBehaviour
	{
		[SerializeField]
		int fontSize = 10, selectedIndex;
		[SerializeField]
		[Range (1, 10)]
		int panelFadeSpeed = 10;
		[SerializeField]
		Font fontFace;
		[SerializeField]
		[Range (0.01f, 0.15f)]
		float _menuFadeSpeed = 0.5f;
		[SerializeField]
		[Range (0.5f, 3f)]
		float panelSize = 1.25f;
		[SerializeField]
		protected MenuCollection menuCollection = new MenuCollection ();

		BlinkingController bkController;

		public float menuFadeSpeed {
			get { return _menuFadeSpeed; }
		}

		// Is menu ready for accepting input from controller?
		public bool IsMenuReady {
			get { return mainCoroutineDone && subCoroutineDone; }
			set { mainCoroutineDone = subCoroutineDone = value; }
		}

		private bool mainCoroutineDone = true, subCoroutineDone = true;

		// For internal method passing
		public delegate IEnumerator MakeMenuDelegate (Menu menu, AfterFadeDelegate after);
		// For menuFadeIn right after menuFadeOut
		public delegate void AfterFadeDelegate ();

		// Use this for initialization
		protected virtual void Awake ()
		{
			// Screen never timeout
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			IsMenuReady = false;
			bkController = GetComponent<BlinkingController> ();

			menuCollection.Init ();
			StartCoroutine (
				menuPanelTvOn (
					MakeMenu,
					menuCollection.GetMenu ("entrance")
				)
			);
		}

		// For non-MonoBehaviour class
		public void menuFadeOut (AfterFadeDelegate after, bool isPanelLeftover = true)
		{
			StartCoroutine (
				menuFadeOutCoroutine (after, isPanelLeftover)
			);
		}

		// Constructing entrance menu from external classes
		public void menuFadeIn (string menuName)
		{
			Menu menu = menuCollection.GetMenu (menuName);

			StartCoroutine (MakeMenu (menu));
			CallSpecificMakeMethod (menu);
		}

		public GameObject getSelectedObject ()
		{
			return GameObject.Find ("Option" + selectedIndex);
		}

		public Option getSelectedOption ()
		{
			return menuCollection
                .GetCurrentMenu ()
                .GetOption (selectedIndex);
		}

		public Menu getCurrentMenu ()
		{
			return menuCollection.GetCurrentMenu ();
		}

		public void selectNext ()
		{
			selectedIndex = (selectedIndex + 1) %
			menuCollection.GetCurrentMenu ().GetElements ().Length;

			// Recursion if not an option
			if (GameObject.Find ("Option" + selectedIndex) == null)
				selectNext ();

			updateSelectedRef ();
		}

		public void selectPrev ()
		{
			int c = menuCollection.GetCurrentMenu ().GetElements ().Length;
			selectedIndex = (selectedIndex + c - 1) % c;

			// Recursion if not an option
			if (GameObject.Find ("Option" + selectedIndex) == null)
				selectPrev ();

			updateSelectedRef ();
		}

		public void ReRenderOptions (Menu menu, int optionStart, int optionEnd, string tagToRemove = null, bool callMakeMethod = true)
		{
			// Set menu NOT ready
			subCoroutineDone = false;

			// Kill all coroutines
			StopAllCoroutines ();

			// Delete all options within range
			for (int i = optionStart; i < optionEnd; i++)
				if (GameObject.Find ("Option" + i))
					Destroy (GameObject.Find ("Option" + i));
				else if (GameObject.Find ("Label" + i))
					Destroy (GameObject.Find ("Label" + i));

			if (tagToRemove != null) {
				// Delete all options tagged
				GameObject[] gameObjects = GameObject.FindGameObjectsWithTag (tagToRemove);
				foreach (GameObject obj in gameObjects)
					Destroy (obj);
			}

			MenuSetting setting = menuCollection.CommonMenuSetting;
			// Get settings from specific
			if (menu is IEmbeddedSetting)
				setting = ((IEmbeddedSetting)menu).getMenuSetting ();

			// Prepare shunken ratio
			float shunkenRatio = 1 / Mathf.Log (
				                     9 + menu.GetElementsCount (),
				                     setting.shunkenBase
			                     );

			StartCoroutine (RenderOptions (
				menu, shunkenRatio, setting, optionStart, optionEnd));
			if (callMakeMethod)
				CallSpecificMakeMethod (menu);
			subCoroutineDone = !callMakeMethod;
		}

		void CallSpecificMakeMethod (Menu menu)
		{
			// Get Method based on type - convention with leading "Make"
			// E.g. GetGalleryMenu(Menu menu), GetHybridMenu(Menu menu)
			MethodInfo method = GetType ().GetMethod (
				                    "Make" + menu.GetType ().Name,
				                    BindingFlags.Instance | BindingFlags.NonPublic,
				                    null,
				                    new[] { typeof(Menu) },
				                    null
			                    );
			// Call method if exists
			if (method != null)
				StartCoroutine ((IEnumerator)method.Invoke (this, new object[] { menu }));
			else
				subCoroutineDone = true;
		}

		protected IEnumerator MakeHybridMenu (Menu menu)
		{
			HybridMenu hybridMenu = (HybridMenu)menu;

			for (int i = 0; i < hybridMenu.GetElementsCount (); i++) {
				HybridOption option = hybridMenu.GetOption (i) as HybridOption;
				if (option != null) {
					// Actualize hybrid control 
					RectTransform hybrid = option.Hybrid.Actualize ()
                        .GetComponent<RectTransform> ();
					Vector3 hybridScale = hybrid.localScale;
					hybrid.gameObject.name = "Hybrid" + i;

					// Loop till sibling is created
					while (GameObject.Find ("Option" + i) == null)
						yield return null;
					// Get parent option
					RectTransform parent = GameObject.Find ("Option" + i)
                        .GetComponent<RectTransform> ();
					// Increase text left padding
					parent.GetComponent<Text> ().text =
                        "  " + parent.GetComponent<Text> ().text;
					// Set parent to option
					hybrid.transform.SetParent (parent);

					// Set position
					makeRectTransform (
						hybrid,
						parent.sizeDelta.x / 2 + 2.5f,
						0,
						0
					);
					makeRectTransform (hybrid, hybridScale);

					// Alter parent
					// Set text alignment
					parent.GetComponent<Text> ().alignment = TextAnchor.MiddleLeft;
					// Set Width Height
					makeRectTransform (
						parent,
						parent.sizeDelta.x + hybrid.sizeDelta.x * hybrid.localScale.x + 10,
						parent.sizeDelta.y > hybrid.sizeDelta.y * hybrid.localScale.y ?
                        parent.sizeDelta.y : hybrid.sizeDelta.y * hybrid.localScale.y
					);

					yield return new WaitForSeconds (menuFadeSpeed);
				}
			}

			subCoroutineDone = true;
		}

		protected IEnumerator MakeMassiveMenu (Menu menu)
		{
			MassiveMenu massive = (MassiveMenu)menu;

			int _from = massive.NumPreNonListOption;
			int _to = massive.GetElementsCount () - massive.NumPostNonListOption;

			string idxTxt = (massive.PageIndex + 1) + "/" +
			                (massive.NumListOption / massive.ItemsPerPage + 1);
			if (!GameObject.Find ("PageIndex"))
                // If page index not yet created
                MakePageIndex (idxTxt, "PageIndex", 7, new Vector3 (
					47f * panelSize - Essentials.GetStringWidth (idxTxt, 7, fontFace) / 2,
					-44f * panelSize,
					0));
			else
                // Update page index
                GameObject.Find ("PageIndex").GetComponent<Text> ().text = idxTxt;

			yield return null;

			if (massive.PageIndex != 0) {
				if (!GameObject.Find ("LeftPageIndicator"))
					MakePageIndex ("‹", "LeftPageIndicator", 14, new Vector3 (
						-47f * panelSize + Essentials.GetStringWidth (idxTxt, 14, fontFace) / 3,
						0,
						0));
			} else
				Destroy (GameObject.Find ("LeftPageIndicator"));

			if (massive.PageIndex != massive.NumListOption / massive.ItemsPerPage) {
				if (!GameObject.Find ("RightPageIndicator"))
					MakePageIndex ("›", "RightPageIndicator", 14, new Vector3 (
						47f * panelSize - Essentials.GetStringWidth (idxTxt, 14, fontFace) / 3,
						0,
						0));
			} else
				Destroy (GameObject.Find ("RightPageIndicator"));

			for (int i = _from; i < _to; i++) {
				// Loop till sibling is created
				yield return new WaitUntil (() => GameObject.Find ("Option" + i) != null);

				// Get parent option
				RectTransform parent = GameObject.Find ("Option" + i)
                    .GetComponent<RectTransform> ();

				// Add tag "ListOption"
				parent.tag = "ListOption";

				// Prepare ListOption
				RectTransform title = MakeListOptionText (
					                      parent,
					                      "Title",
					                      ((ListOption)massive.GetOption (i)).Title,
					                      massive.GetOption (i),
					                      new Vector3 (0, 3)
				                      ).GetComponent<RectTransform> ();

				RectTransform description = MakeListOptionText (
					                            parent,
					                            "Description",
					                            ((ListOption)massive.GetOption (i)).Description,
					                            massive.GetOption (i),
					                            new Vector3 (0, -3)
				                            ).GetComponent<RectTransform> ();

				// Update parent width height
				parent.sizeDelta = new Vector2 (
                    // Add 3f for better looking
					((title.sizeDelta.x > description.sizeDelta.x) ? title.sizeDelta.x :
                        description.sizeDelta.x) + 3f,
					title.sizeDelta.y + description.sizeDelta.y
				);

				// Remove parent Text
				Destroy (parent.GetComponent<Text> ());

				yield return null;
			}

			if (getSelectedObject () == null && mainCoroutineDone)
				selectPrev ();

			subCoroutineDone = true;
		}

		void MakePageIndex (string indexText, string name, int fontSize, Vector3 location)
		{
			// Create page indicator
			GameObject pageIndex = new GameObject (name);
			pageIndex.transform.SetParent (transform);
			pageIndex.AddComponent<Text> ();
			// Set text
			makeText (
				pageIndex.GetComponent<Text> (),
				indexText,
				Color.white,
				fontSize
			);
			// Set position
			makeRectTransform (
				pageIndex.GetComponent<RectTransform> (),
				location.x,
				location.y,
				location.z
			);
		}

		GameObject MakeListOptionText (RectTransform parent, string name, string value, Option option, Vector3 position)
		{
			// GameObject Title
			GameObject gameText = new GameObject (name);
			gameText.transform.parent = parent;
			gameText.transform.localPosition = new Vector3 (0, 0, 0);
			gameText.AddComponent<Text> ();

			Text txt = gameText.GetComponent<Text> ();
			Text parentTxt = parent.GetComponent<Text> ();

			txt.text = value;
			// Copy attributes
			txt.font = parentTxt.font;
			txt.color = option.TextColor;
			txt.fontSize = parentTxt.fontSize;
			txt.lineSpacing = parentTxt.lineSpacing;
			txt.alignment = parentTxt.alignment;

			// Make RectTransform
			RectTransform rt = gameText.GetComponent<RectTransform> ();
			makeRectTransform (rt, Vector3.one);
			makeRectTransform (
				rt,
				Essentials.GetStringWidth (txt.text, txt.fontSize, txt.font) + 5f,
				Essentials.GetStringHeight (txt.text, txt.fontSize)
			);
			makeRectTransform (rt, position.x, position.y, position.z);

			return gameText;
		}

		protected IEnumerator MakeGalleryMenu (Menu menu)
		{
			GalleryMenu gallery = (GalleryMenu)menu;
			GalleryMenuSetting setting =
				(GalleryMenuSetting)gallery.getMenuSetting ();

			for (int i = 0; i < gallery.RowCount; i++)
				for (int j = 0; j < gallery.ColCount; j++) {
					//Create logo
					GameObject logoImage = new GameObject ("GalleryLogo" + j);
					logoImage.transform.SetParent (transform);
					logoImage.AddComponent<Image> ();

					float width = gallery.getLogo (i, j).getWidth ();
					float height = gallery.getLogo (i, j).getHeight ();
					// Default setting logo width / height
					width = (width == 0) ? setting.logoWidth : width;
					height = (height == 0) ? setting.logoHeight : height;
					// Apply logoShunkenBase iff more than one item in galleryMenu
					if (gallery.ColCount > 1) {
						width /= setting.logoShunkenBase * gallery.ColCount * gallery.RowCount;
						height /= setting.logoShunkenBase * gallery.ColCount * gallery.RowCount;
					}

					makeRectTransform (
						logoImage.GetComponent<RectTransform> (),
						width,
						height
					);

					// Set position
					makeRectTransform (
						logoImage.GetComponent<RectTransform> (),
                        // X axis calculation
						-((width + setting.logoPadding) / 2) * (gallery.ColCount - 1)
						+ j * (width + setting.logoPadding),
                        // Starting from setting logo position
						setting.logoPosition +
                        // Y axis calculation
						((height + setting.logoPadding) / 2) * (gallery.RowCount - 1)
						- i * (height + setting.logoPadding),
						0
					);

					logoImage.GetComponent<Image> ().sprite = gallery.getLogo (i, j).getLogoSprite ();

					yield return new WaitForSeconds (menuFadeSpeed);
				}

			subCoroutineDone = true;
		}

		IEnumerator menuFadeOutCoroutine (AfterFadeDelegate after, bool isPanelLeftOver = true)
		{
			IsMenuReady = false;

			// Fade out after delay for better visual effect
			yield return new WaitForSeconds (0.1f);

			for (int i = 1; i < transform.childCount; i++) {
				if (transform.GetChild (i).gameObject.activeSelf) {
					transform.GetChild (i).gameObject.SetActive (false);
					yield return new WaitForSeconds (menuFadeSpeed);
				} else
					yield return null;
			}

			// Destroy Options
			for (int i = 5; i < transform.childCount; i++)
				Destroy (transform.GetChild (i).gameObject);

			if (isPanelLeftOver)
                // Action after fade out
                after ();
			else
                // Panel fade out first, then after()
                StartCoroutine (menuPanelTvOff (after));
		}

		public IEnumerator menuPanelTvOn (MakeMenuDelegate makeMenu, Menu menu, AfterFadeDelegate after = null)
		{
			// Fade out after delay for better visual effect
			yield return new WaitForSeconds (0.5f);

			RectTransform panel = transform.Find ("Panel").GetComponent<RectTransform> ();

			for (float i = 0; i <= panelSize; i += panelSize / panelFadeSpeed) {
				panel.localScale = new Vector3 (0.005f, i);
				yield return null;
			}

			for (float i = 0.005f; i <= panelSize; i += (panelSize - 0.005f) / panelFadeSpeed) {
				panel.localScale = new Vector3 (i, panelSize);
				yield return null;
			}

			menuFadeIn (menuCollection.GetMenuName (menu));
		}

		IEnumerator menuPanelTvOff (AfterFadeDelegate after)
		{
			RectTransform panel = transform.Find ("Panel").GetComponent<RectTransform> ();

			for (float i = panelSize; i > 0; i -= (panelSize - 0.005f) / panelFadeSpeed) {
				panel.localScale = new Vector3 (i, panel.localScale.y);
				yield return null;
			}

			for (float i = panelSize; i >= -0.001f; i -= panelSize / panelFadeSpeed) {
				panel.localScale = new Vector3 (panel.localScale.x, i);
				yield return null;
			}

			after ();
		}

		IEnumerator RenderOptions (Menu menu, float shunkenRatio, MenuSetting setting, int optionStart, int optionEnd)
		{
			bool isMainCoroutine = mainCoroutineDone;

			// Create options
			float posComplement = setting.optionBeginPos;

			for (int i = optionStart; i < optionEnd; i++) {
				// GameObject following their type
				GameObject option = menu.GetOption (i) != null ?
                    new GameObject ("Option" + i) : new GameObject ("Label" + i);

				option.transform.SetParent (transform);
				option.AddComponent<Text> ();

				// Cross binding
				menu.GetElement (i).SetGameText (option.GetComponent<Text> ());

				// Update posComplement
				posComplement = setting.optionBeginPos -
				(setting.optionPadding * shunkenRatio + Essentials.GetStringHeight (
					option.GetComponent<Text> ().text,
					option.GetComponent<Text> ().fontSize,
					0
				) * 0.2f) * i;

				makeText (option.GetComponent<Text> (), i, shunkenRatio, menu);
				// Set position
				makeRectTransform (
					option.GetComponent<RectTransform> (),
					0,
					posComplement,
					0
				);
				// Set width height
				makeRectTransform (
					option.GetComponent<RectTransform> (),
					Essentials.GetStringWidth (
						option.GetComponent<Text> ().text,
						option.GetComponent<Text> ().fontSize,
						fontFace
					) + 5,
					Essentials.GetStringHeight (
						option.GetComponent<Text> ().text,
						option.GetComponent<Text> ().fontSize
					)
				);

				yield return new WaitForSeconds (menuFadeSpeed);
			}

			yield return new WaitUntil (() => subCoroutineDone);
			updateSelectedRef ();

			if (isMainCoroutine)
				mainCoroutineDone = true;
		}

		public IEnumerator MakeMenu (Menu menu, AfterFadeDelegate after = null)
		{
			MenuSetting setting = menuCollection.CommonMenuSetting;
			// Get settings from specific
			if (menu is IEmbeddedSetting) {
				setting = ((IEmbeddedSetting)menu).getMenuSetting ();
			}
			float shunkenRatio = 1 / Mathf.Log (
				                     9 + menu.GetElementsCount (),
				                     setting.shunkenBase
			                     );

			if (menu is PickerMenu && ((PickerMenu)menu).GetLogo () != null) {
				//Create logo
				GameObject logoImage = new GameObject ("Logo");
				logoImage.transform.SetParent (transform);
				logoImage.AddComponent<Image> ();
				// Set width height
				makeRectTransform (
					logoImage.GetComponent<RectTransform> (),
					setting.logoWidth,
					setting.logoHeight
				);
				// Set position
				makeRectTransform (
					logoImage.GetComponent<RectTransform> (),
					0,
					setting.logoPosition,
					0
				);
				Image img = logoImage.GetComponent<Image> ();
				img.sprite = ((PickerMenu)menu).GetLogo ().getLogoSprite ();
				img.color = setting.logoColor;

				yield return new WaitForSeconds (menuFadeSpeed);
			}

			if (menu.Title != null) {
				// Create Title message on the topmost of panel
				GameObject title = new GameObject ("Title");
				title.transform.SetParent (transform);
				title.AddComponent<Text> ();

				makeText (
					title.GetComponent<Text> (),
					menu.Title,
					setting.titleColor,
					setting.titleFontSize
				);

				makeRectTransform (
					title.GetComponent<RectTransform> (),
					0,
					setting.titlePos,
					0
				);
				yield return new WaitForSeconds (menuFadeSpeed);
			}

			// Prepare options / labels
			StartCoroutine (RenderOptions (
				menu, shunkenRatio, setting, 0, menu.GetElementsCount ()));

			// Create page indicator
			GameObject bread = new GameObject ("Breadcrumb");
			bread.transform.SetParent (transform);
			bread.AddComponent<Text> ();
			// Set text
			makeText (
				bread.GetComponent<Text> (),
				Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase (
					menuCollection.GetMenuName (menu)
				),
				Color.grey,
				5
			);
			// Set position
			makeRectTransform (
				bread.GetComponent<RectTransform> (),
				47f * panelSize
				- Essentials.GetStringWidth (menuCollection.GetMenuName (menu), 5, fontFace) / 2,
				44f * panelSize,
				0
			);
			yield return new WaitForSeconds (menuFadeSpeed);

			// Set Blinking to option 1
			selectedIndex = menu.GetElements ().Length - 1;
			selectNext ();
			updateSelectedRef ();

			StartCoroutine (
				bkController.setDotsEnabledCoroutine (true)
			);

			mainCoroutineDone = true;

			if (after != null)
				after ();
		}

		void updateSelectedRef ()
		{
			bkController.setSelected (
				getSelectedObject ().GetComponent<RectTransform> ()
			);
		}

		void makeText (Text text, string message, Color color, int fontSize)
		{
			text.font = fontFace;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = message;
			text.color = color;
			text.fontSize = fontSize;
			text.lineSpacing = 1.2f;
		}

		void makeText (Text text, int position, float shunkenRatio, Menu menu)
		{
			makeText (
				text,
				menu.GetElement (position).Text,
				menu.GetElement (position).TextColor,
				Mathf.RoundToInt (fontSize * shunkenRatio)
			);
		}

		void makeRectTransform (RectTransform rectTransform)
		{
			rectTransform.localRotation = Quaternion.Euler (0, 0, 0);
			rectTransform.localScale = new Vector3 (1f, 1f, 0);
		}

		void makeRectTransform (RectTransform rectTransform, float width, float height)
		{
			makeRectTransform (rectTransform);
			rectTransform.sizeDelta = new Vector2 (width, height);
		}

		void makeRectTransform (RectTransform rectTransform, float x, float y, float z)
		{
			makeRectTransform (rectTransform);
			rectTransform.localPosition = new Vector3 (x, y, z);
		}

		void makeRectTransform (RectTransform rectTransform, Vector3 scale)
		{
			makeRectTransform (rectTransform);
			rectTransform.localScale = scale;
		}

		void Update ()
		{
			Menu m = menuCollection.GetCurrentMenu ();

			if (m != null) {
				// Update event delegate
				m.OnUpdate ();

				// Disable options that need internet connection when none
				for (int i = 0; i < m.GetElementsCount (); i++)
					if (m.GetOption (i) is NetworkOption) {
						NetworkOption n = (NetworkOption)m.GetOption (i);
						if (!n.isAvailable ()) {
							n.pushHoverEventHandler ();
							n.pushSelectEventHandler ();

							n.SetHoverEventHandler (menuCollection.HoverDisabled);
							n.SetSelectEventHandler (menuCollection.SelectDisabled);
						} else {
							n.popHoverEventHandler ();
							n.popSelectEventHandler ();
						}
					}
			}
		}
	}
}
