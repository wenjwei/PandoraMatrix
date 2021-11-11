
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using cPackage.Tools;
using cPackage.Package;

namespace cPackage.UI
{
	public class cPackageConfigurationWindow : EditorWindow
	{
		public static void OpenConfigurationWindow()
		{
			cPackageConfigurationWindow window = GetWindow<cPackageConfigurationWindow>(true);
			window.ShowUtility();
			string title = string.Format("cPackage-v{0}", _version);
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent(title);
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = title;
#else
			cPackageHelper.LogError("this feature is not implemented in current unity");
#endif
			window.minSize = new Vector2(220, 200);
		}

		private const string _version = cPackageHelper.cPackageVersion;

		private Configuration _configuration;

		void Awake()
		{
			_configuration = cPackageHelper.GetConfiguration();
		}

		private Vector2 _scrollPosition;

		void OnGUI()
		{
			GUILayout.Space(10);

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			GUILayout.BeginHorizontal();
			GUILayout.Label(GetEditorContent("DebugMode"));
			GUILayout.FlexibleSpace();
			_configuration.DebugMode = GUILayout.Toggle(_configuration.DebugMode, "");
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label(GetEditorContent("IncludeCSharp"));
			GUILayout.FlexibleSpace();
			_configuration.IncludeCSharp = GUILayout.Toggle(_configuration.IncludeCSharp, "");
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("uGUI Support:");
			GUILayout.FlexibleSpace();
			GUILayout.Label(cPackageHelper.IsUGUISupported().ToString());
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label(GetEditorContent("GameObjectSerializedVersion"));
			GUILayout.FlexibleSpace();
			GUILayout.Label(cPackageHelper.GetGameObjectSerializedVersion());
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label(GetEditorContent("AdvancedMode"));
			GUILayout.FlexibleSpace();
			_configuration.AdvancedMode = GUILayout.Toggle(_configuration.AdvancedMode, "");
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("useProSkin");
			GUILayout.FlexibleSpace();
			GUILayout.Label(cPackageHelper.IsProSkin().ToString());
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			ShowConfigurationExtensions();

			GUILayout.EndScrollView();
		}

		void ShowConfigurationExtensions()
		{
			List<FieldInfo> fieldInfos = cPackageHelper.GetClassFieldsWithCustomAttribute<ConfigurationExtensionFieldAttribute>(typeof(Configuration));
			if (fieldInfos.Count > 0)
				GUILayout.Label("Pipeline Feature Extensions:");
			foreach (var fieldInfo in fieldInfos)
			{
				GUILayout.BeginHorizontal();

				// draw extension name
				GUILayout.Label(GetEditorContent(fieldInfo.Name));
				GUILayout.FlexibleSpace();

				// draw setting button
				string settingCls = GetEditorSettingCls(fieldInfo.Name);
				if (!string.IsNullOrEmpty(settingCls))
				{
					if (GUILayout.Button("setting"))
					{
						Type settingClsType = Assembly.GetExecutingAssembly().GetType(settingCls, true);
						MethodInfo mi = settingClsType.GetMethod("OpenSettingWindow", BindingFlags.Static | BindingFlags.Public);
						mi.Invoke(null, null);
					}
				}

				// draw switch toggle
				bool ret = GUILayout.Toggle((bool)fieldInfo.GetValue(_configuration), "");
				fieldInfo.SetValue(_configuration, ret);
				GUILayout.Space(10);
				GUILayout.EndHorizontal();
			}
		}

		GUIContent GetEditorContent(string fieldName)
		{
			return new GUIContent()
			{
				text = GetEditorLabel(fieldName),
				tooltip = GetEditorTips(fieldName)
			};
		}

		string GetEditorLabel(string fieldName)
		{
			ConfigurationFieldAttribute attr = cPackageHelper.GetClassFieldAttribute<ConfigurationFieldAttribute>(typeof(Configuration), fieldName);
			return attr.EditorLabel;
		}

		string GetEditorTips(string fieldName)
		{
			ConfigurationFieldAttribute attr = cPackageHelper.GetClassFieldAttribute<ConfigurationFieldAttribute>(typeof(Configuration), fieldName);
			return attr.Tips;
		}

		string GetEditorSettingCls(string fieldName)
		{
			ConfigurationFieldAttribute attr = cPackageHelper.GetClassFieldAttribute<ConfigurationFieldAttribute>(typeof(Configuration), fieldName);
			return attr.SettingCls;
		}

		void OnDestroy()
		{
			_configuration = null;
			cPackageHelper.SaveConfiguration();
			AssetDatabase.SaveAssets();
		}
	}
}
