
using UnityEngine;
using System;

namespace PPM
{
	public class PackageMenuView : IPackageView
	{
		private MenuTreeIMGUI _menuTreeGUI;

		private const int IconWidth = 30;
		private const int IconHeight = 30;
	
		public PackageMenuView()
		{
			InitMenuTreeIMGUI(PackageManager.Instance.GetMenuTree());
			PackageManager.Instance.OnMenuViewRefreshed += OnViewRefreshed;
		}

		public void OnViewRefreshed()
		{
			InitMenuTreeIMGUI(PackageManager.Instance.GetMenuTree());
		}

		private void OnMenuSelected(MenuData menuData)
		{
			PackageManager.Instance.SelectMenu(menuData);
		}

		private void InitMenuTreeIMGUI(MenuTree menuTree)
		{
			_menuTreeGUI = new MenuTreeIMGUI(menuTree.Root);
			_menuTreeGUI.NodeSelectCallback += OnMenuSelected;
			ThreadUtils.ExecuteOnNextFrame(() => _menuTreeGUI.SelectNode(menuTree.Root[0][0]));
		}

		public void DrawGUI(Rect uiRect, GUISkin uiSkin)
		{ 
			_menuTreeGUI.DrawTreeLayout(uiRect);
            DrawIcon(new Rect(uiRect.x, uiRect.y + uiRect.height - IconHeight, IconWidth, IconHeight),
				new GUIContent("", null, "更多操作"),
				"settingButton",
				() => { PackageMangerMoreOperate.OpenPackageMangerMoreOperateWindow(); });
		}

		private void DrawIcon(Rect rect, GUIContent content, GUIStyle style, Action cb)
		{
			if (UnityEngine.GUI.Button(rect, content, style))
				if (cb != null) cb();
		}

		public void Dispose()
		{
			PackageManager.Instance.OnMenuViewRefreshed -= OnViewRefreshed;
		}
	}
}
