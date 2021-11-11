
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using cPackage.Package;
using cPackage.Tools;
using Newtonsoft.Json;

namespace cPackage.UI
{
	public class cPackageExportWindow : EditorWindow
	{
		private static cPackageExportTree _tree;
		private static cPackageExportTreeView _treeView;
		private Vector2 _scrollPosition;
		private static bool _includeDependencies;

		private static string[] _guids;
		private static string[] _dependencyGuids;

		public static void ShowExportTreeView(string[] guids, string[] dependencyGuids)
		{
			_guids = guids;
			_dependencyGuids = dependencyGuids;

			InitTreeView(_includeDependencies ? dependencyGuids : _guids);
			ShowWindow();
		}

		private static void InitTreeView(string[] guids)
		{
			_tree = new cPackageExportTree();
			_treeView = new cPackageExportTreeView(_tree.GetRoot());

			foreach (var guid in guids)
			{
				_tree.AddExportAsset(guid);
			}
		}

		private static void ShowWindow()
		{
			cPackageExportWindow window = EditorWindow.GetWindow<cPackageExportWindow>(true);
			window.ShowUtility();
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent("Exporting cPackage");
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = "Exporting cPackage";
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
			if (_tree.GetRoot().GetChildCount() == 0)
				NothingExportOnGUI();
			else
				ExportOnGUI();
		}

		void ExportOnGUI()
		{
			TopArea();
			float bottomAreaHeight = cPackageHelper.IsAdvancedMode() ? 120.0f : 85.0f;
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(position.height - bottomAreaHeight));
			_treeView.Display();
			EditorGUILayout.EndScrollView();

			if (cPackageHelper.IsAdvancedMode())
				ExportConfigurationArea();
			BottomArea();
		}

		void NothingExportOnGUI()
		{
			GUILayout.Space(20f);
#if !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
			GUILayout.BeginVertical(EditorStyles.helpBox);
#endif
			GUILayout.Label("Nothing to export!", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("OK"))
			{
				Close();
				GUIUtility.ExitGUI();
			}
			GUILayout.EndHorizontal();

#if !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
			GUILayout.EndVertical();
#endif
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
			GUI.Label(titleRect, "Items to Export", titleStyle);
		}

		void ExportConfigurationArea()
		{
			GUILayout.BeginVertical("ProjectBrowserBottomBarBg");
			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			if (GUILayout.Button("Save Config", GUILayout.Width(150)))
			{
				string savePath = EditorUtility.SaveFilePanel("file to save", Application.dataPath, "export", "cconf");
				if (!string.IsNullOrEmpty(savePath))
				{
					List<string> selectedNodesConfigList = _tree.GetSelectedGuids();
					string configStr = JsonConvert.SerializeObject(selectedNodesConfigList);
					File.WriteAllText(savePath, configStr);
				}
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Read Config", GUILayout.Width(150)))
			{
				string selectedPath = EditorUtility.OpenFilePanel("file to read", Application.dataPath, "cconf");
				if (!string.IsNullOrEmpty(selectedPath))
				{
					string configStr = File.ReadAllText(selectedPath);
					_tree.DeselectAllNodes();
					_tree.SelectNodes(JsonConvert.DeserializeObject<List<string>>(configStr));
				}
			}

			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.EndVertical();
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
			EditorGUI.BeginChangeCheck();
			_includeDependencies = GUILayout.Toggle(_includeDependencies, "Include Dependencies");
			if (EditorGUI.EndChangeCheck())
			{
				InitTreeView(_includeDependencies ? _dependencyGuids : _guids);
			}
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(new GUIContent("Export...")))
			{
				string fileName = EditorUtility.SaveFilePanel("Export cPackage ...", "", "", "cpackage");
				if (!string.IsNullOrEmpty(fileName))
				{
					List<string> guidList = _tree.GetSelectedGuids();
					cPackageExporter.ExportCPackage(guidList, fileName);

					Close();
					GUIUtility.ExitGUI();
				}
			}
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.EndVertical();
		}
	}
}
