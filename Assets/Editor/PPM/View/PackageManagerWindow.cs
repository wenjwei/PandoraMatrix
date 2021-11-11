
using UnityEngine;
using UnityEditor;

namespace PPM
{
	using GUI = UnityEngine.GUI;

	//		 __ __ __ __ __ _  Package Manager UI _ __ __ ___ __ _ _
	//		|			  |						   |				|
	//		|			  |						   |				|
	//		|	Menu View |	     Package List	   | Package Detail	|
	//		|			  |						   |				|
	//		|__ __ __ __ _|_ __ __ __ __ __ _ __ __|__ __ __ __ _ __|

	public static class PackageManagerUI
	{
		public const string WindowTitle = "潘多拉插件管理器";
		public const string GUISkinResourceName = "PPMGUISkin";
		public const float WindowMinWidth = 800;
		public const float WindowMinHeight = 450;
		public const float WindowMaxWidth = 800;
		public const float WindowMaxHeight = 450;
		public const float MenuViewWidth = WindowMinWidth * 1 / 4;
		public const float PackageListViewWidth = WindowMinWidth * 1 / 2;
		public const float PackageDetailViewWidth = WindowMinWidth * 1 / 4;
		public static readonly Color MenuViewBackgroundColor = new Color(50 / 255f, 50 / 255f, 50 / 255f);
		public static readonly Color PackageListViewBackgroundColor = new Color(40 / 255f, 40 / 255f, 40 / 255f);
		public static readonly Color PackageDetailViewBackgroundColor = new Color(50 / 255f, 50 / 255f, 50 / 255f);
		public static readonly Rect PackageListViewSortRect = new Rect(MenuViewWidth, 0, PackageListViewWidth, EditorGUIUtility.singleLineHeight * 2f);
		public static readonly Rect PackageDetailViewSearchRect = new Rect(MenuViewWidth + PackageListViewWidth, 0, PackageDetailViewWidth - 2, EditorGUIUtility.singleLineHeight * 1.5f);
	}

	interface IPackageView
	{
		void DrawGUI(Rect uiRect, GUISkin uiSkin);
		void Dispose();
	}

	public class PackageManagerWindow : EditorWindow
	{
		[MenuItem("Window/Pandora Package Manager")]
		private static void OpenPackageManagerWindow()
		{
			if (PackageManager.Instance.InitiateEnvironment() == false)
				return;

			PackageManagerWindow window = GetWindow<PackageManagerWindow>(true);
			window.ShowUtility();
			window.SetWndTitle(string.Format("{0} - v{1}", PackageManagerUI.WindowTitle, PPMHelper.GetCurrentPPMVersion()));
			window.minSize = new Vector2(PackageManagerUI.WindowMinWidth, PackageManagerUI.WindowMinHeight);
			window.maxSize = new Vector2(PackageManagerUI.WindowMaxWidth, PackageManagerUI.WindowMaxHeight);

			window.SetGUISkin(Resources.Load<GUISkin>(PackageManagerUI.GUISkinResourceName));
			PackageManager.Instance.SetLoginCallback(window.UserLoginCallback);
			PackageManager.Instance.GetCookieUID(window.GetUserIDCallback);
			PackageManager.Instance.GetInstalledPackages();

			PPMHelper.InstallSelf();
			PackageManager.Instance.RequestNeedUpdateCount();
		}

		private void SetWndTitle(string title)
		{
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			titleContent = new GUIContent(title);
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			title = title;
#else
			PPMHelper.LogError("this feature is not implemented in current unity");
#endif
		}

		public void UserLoginCallback(string username)
		{
			SetWndTitle(string.Format("{0} - {1} - v{2}", PackageManagerUI.WindowTitle, username, PPMHelper.GetCurrentPPMVersion()));
			PackageManager.Instance.GetInstalledPackages();
		}

		public void GetUserIDCallback(string username)
		{
			if (!string.IsNullOrEmpty(username))
            {
                SetWndTitle(string.Format("{0} - {1} - v{2}", PackageManagerUI.WindowTitle, username, PPMHelper.GetCurrentPPMVersion()));
            }
		}

		public void SetGUISkin(GUISkin guiSkin)
		{
			_pandoraManagerSkin = guiSkin;
		}

		private GUISkin _pandoraManagerSkin;
		private Rect _windowRect;
		private Rect _menuViewRect;
		private Rect _packageListViewRect;
		private Rect _packageDetailViewRect;

		private PackageMenuView _menuView;
		private PackageListView _listView;
		private PackageDetailView _detailView;
        

		private void OnGUI()
		{
			GUI.skin = _pandoraManagerSkin;
			_windowRect = GUILayoutUtility.GetRect(position.width, position.height);

			DrawUI();
		}

		private void DrawUI()
		{
			DrawBackgroundRect();

			_menuView.DrawGUI(_menuViewRect, _pandoraManagerSkin);
			_listView.DrawGUI(_packageListViewRect, _pandoraManagerSkin);
			_detailView.DrawGUI(_packageDetailViewRect, _pandoraManagerSkin);
		}

		private void DrawBackgroundRect()
		{
			if (Event.current.type == EventType.Repaint)
			{
				float startWidth = 0;
				_menuViewRect = DrawRect(startWidth, PackageManagerUI.MenuViewWidth, PackageManagerUI.MenuViewBackgroundColor);
				startWidth += PackageManagerUI.MenuViewWidth;
				_packageListViewRect = DrawRect(startWidth, PackageManagerUI.PackageListViewWidth, PackageManagerUI.PackageListViewBackgroundColor);
				startWidth += PackageManagerUI.PackageListViewWidth;
				_packageDetailViewRect = DrawRect(startWidth, PackageManagerUI.PackageDetailViewWidth, PackageManagerUI.PackageDetailViewBackgroundColor);
			}
		}

		private Rect DrawRect(float startWidth, float viewWidth, Color viewBackgroundColor)
		{
			Rect viewRect = new Rect(_windowRect.xMin + startWidth, _windowRect.yMin, viewWidth, _windowRect.height);
			GUIStyle style = PPMHelper.GenerateStyle(viewRect.width, viewRect.height, viewBackgroundColor);
			GUI.Label(viewRect, GUIContent.none, style);
			return viewRect;
		}

		protected void OnEnable()
		{
			_menuView = new PackageMenuView();
			_listView = new PackageListView();
			_detailView = new PackageDetailView();
		}

		protected void OnDisable()
		{
			_menuView.Dispose();
			_listView.Dispose();
			_detailView.Dispose();

			_menuView = null;
			_listView = null;
			_detailView = null;

			PackageManager.Instance.UnsetLoginCallback();
			PackageManager.Instance = null;
		}
	}
}
