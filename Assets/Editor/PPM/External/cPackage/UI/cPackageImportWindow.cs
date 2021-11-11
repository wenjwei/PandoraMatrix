
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using cPackage.Package;
using cPackage.Tools;

namespace cPackage.UI
{
	public class cPackageImportWindow : EditorWindow
	{
		private static cPackageImportTree _tree;
		private static cPackageImportTreeView _treeView;
		private Vector2 _scrollPosition;
		private static Action<bool> _importRetCallback;

		public static void ShowImportTreeView(List<ImportItem> itemList, Action<bool> importRetCallback)
		{
			_importRetCallback = importRetCallback;
			InitTreeView(itemList);
			ShowWindow();
		}

		private static void InitTreeView(List<ImportItem> itemList)
		{
			_tree = new cPackageImportTree();
			_treeView = new cPackageImportTreeView(_tree.GetRoot());

			foreach (var item in itemList)
			{
				_tree.AddImportAsset(item);
			}
		}

		private static void ShowWindow()
		{
			cPackageImportWindow window = EditorWindow.GetWindow<cPackageImportWindow>(true);
			window.ShowUtility();

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent("Importing cPackage");
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = "Importing cPackage";
#else
			cPackageHelper.LogError("this feature is not implemented in current unity");
#endif

			window.position = new Rect(100, 100, 400, 300);
			window.minSize = new Vector2(350, 350);
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}

		void OnGUI()
		{
			if (_tree.HasItemToImport())
				ImportOnGUI();
			else
				NothingImportOnGUI();
		}

		void ImportOnGUI()
		{
			TopArea();
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(position.height - 85f));
			_treeView.Display();
			EditorGUILayout.EndScrollView();
			BottomArea();
		}

		void NothingImportOnGUI()
		{
			GUILayout.Label("Nothing to import!", EditorStyles.boldLabel);
			GUILayout.Label("All assets from this package are already in your project.", "WordWrappedLabel");

			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("ProjectBrowserBottomBarBg");
			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("OK"))
			{
                if (_importRetCallback != null)
                    _importRetCallback(false);

                Close();
				GUIUtility.ExitGUI();
			}
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.EndVertical();
		}

		void TopArea()
		{
			float totalTopHeight = 53f;
			Rect r = GUILayoutUtility.GetRect(position.width, totalTopHeight);

			GUIStyle topBarBgStyle = new GUIStyle("ProjectBrowserHeaderBgTop");
			topBarBgStyle.fixedHeight = 0;
			topBarBgStyle.border.top = topBarBgStyle.border.bottom = 2;
			GUI.Label(r, GUIContent.none, topBarBgStyle);

			Rect titleRect = new Rect(r.x + 5f, r.yMin, r.width, r.height);
			GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
			titleStyle.fontStyle = FontStyle.Bold;
			titleStyle.alignment = TextAnchor.MiddleLeft;
			GUI.Label(titleRect, "Items to Import", titleStyle);
		}

		void BottomArea()
		{
			GUILayout.BeginVertical("ProjectBrowserBottomBarBg");
			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			if (GUILayout.Button("All", GUILayout.Width(50)))
			{
				_tree.SelectAllNodes();
			}

			if (GUILayout.Button("None", GUILayout.Width(50)))
			{
				_tree.DeselectAllNodes();
			}
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(new GUIContent("Cancel")))
			{
                if (_importRetCallback != null)
                    _importRetCallback(false);

                Close();
				GUIUtility.ExitGUI();
			}

			if (GUILayout.Button(new GUIContent("Import")))
			{
				cPackageExporter.ImportItemList(_tree.GetSelectedImportItems());

                if (_importRetCallback != null)
                    _importRetCallback(true);

                Close();
				GUIUtility.ExitGUI();
			}
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.EndVertical();
		}

		void OnDestroy()
		{
			cPackageExporter.DestroyTempCPackageFolder();
		}
	}
}
