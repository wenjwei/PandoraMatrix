using UnityEngine;
using cPackage.Tools;
using UnityEditor;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using Newtonsoft.Json;

namespace cPackage.Package
{
	public partial class Configuration
	{
		[ConfigurationExtensionField]
		[ConfigurationField("Replace ScriptName:", "replace scriptName for prefab solving missing script reference problem.", "cPackage.Pipeline.Prefab.ReplaceScriptNameSettingWindow")]
		public bool ReplaceScriptName;
	}
}

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$", "ReplaceScriptName")]
	public class ReplaceScriptName : BaseFeatureProcess
	{
		public ReplaceScriptName()
		{
			ExecutePriority = 50;
		}

		public override void ProcessFeature(string filePath)
		{
			ReplaceData data = GetReplaceData();

			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			int updateDocumentCount = 0;

			float totalCount = documents.Count;
			for (int i = 0; i < totalCount; i++)
			{
				YamlDocument document = documents[i];

				YamlNode node = document.RootNode["ScriptName"];
				if (node == null)
					continue;

				string assemblyQualifiedName = node.ToString();
				bool toReplace = ToReplaceStr(ref assemblyQualifiedName, data);
				if (toReplace)
				{
					updateDocumentCount++;
					((YamlMappingNode)document.RootNode).Update("ScriptName", assemblyQualifiedName);
				}
			}

			if (updateDocumentCount > 0)
				cPackageHelper.WriteYamlDocuments(filePath, documents);
		}

		private bool ToReplaceStr(ref string scriptName, ReplaceData data)
		{
			foreach(var set in data.data)
			{
				if (scriptName.Contains(set.ToFindStr))
				{
					scriptName = scriptName.Replace(set.ToFindStr, set.ToReplaceStr);
					return true;
				}
			}

			return false;
		}

		private static ReplaceData GetReplaceData()
		{
			string saveKey = "cPackage.Pipeline.Prefab.ReplaceData";
			if (!PlayerPrefs.HasKey(saveKey))
				return new ReplaceData();

			string saveValue = PlayerPrefs.GetString(saveKey);
			return JsonConvert.DeserializeObject<ReplaceData>(saveValue);
		}
	}

	public class ReplaceSet
	{
		public string ToFindStr;
		public string ToReplaceStr;
	}

	public class ReplaceData
	{
		public List<ReplaceSet> data = new List<ReplaceSet>();
	}

	public class ReplaceScriptNameSettingWindow : EditorWindow
	{
		private static GUISkin _skin;
		private static ReplaceData _data;
		private const string SaveKey = "cPackage.Pipeline.Prefab.ReplaceData";

		public static void OpenSettingWindow()
		{
			ReplaceScriptNameSettingWindow window = EditorWindow.GetWindow<ReplaceScriptNameSettingWindow>(true);
			window.ShowUtility();
			string title = string.Format("脚本名替换设置");
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent(title);
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = title;
#else
			cPackageHelper.LogError("this feature is not implemented in current unity");
#endif
			window.minSize = new Vector2(400, 300);
			window.maxSize = new Vector2(400, 300);
			InitSkin();
			InitData(SaveKey);
		}

		private static void InitSkin()
		{
			_skin = AssetDatabase.LoadAssetAtPath("Assets/Editor/cPackage/Pipeline/Prefab/ReplaceScriptName/ReplaceScriptName.guiskin", typeof(GUISkin)) as GUISkin;
		}

		private static void InitData(string saveKey)
		{
			if (!PlayerPrefs.HasKey(saveKey))
			{
				_data = new ReplaceData();
			}
			else
			{
				string saveValue = PlayerPrefs.GetString(saveKey);
				_data = JsonConvert.DeserializeObject<ReplaceData>(saveValue);
			}	
		}

		private static void SaveData(string saveKey)
		{
			string saveValue = JsonConvert.SerializeObject(_data);
			PlayerPrefs.SetString(saveKey, saveValue);
			PlayerPrefs.Save();
		}

		private Vector2 _scrollPosition;
		private void OnGUI()
		{
			GUI.skin = _skin;

			GUILayout.BeginHorizontal();
			GUILayout.Label("目标脚本名");
			GUILayout.Label("-->");
			GUILayout.Label("替换脚本名");
			GUILayout.Space(20);
			GUILayout.EndHorizontal();

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			List<ReplaceSet> toDeleteList = new List<ReplaceSet>();
			foreach(var set in _data.data)
			{
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();

				EditorGUI.BeginChangeCheck();
				set.ToFindStr = TextField(set.ToFindStr);
				GUILayout.Label("-->");
				set.ToReplaceStr = TextField(set.ToReplaceStr);
				if (EditorGUI.EndChangeCheck())
					SaveData(SaveKey);
				if (GUILayout.Button("x", GUILayout.Width(20)))
				{
					toDeleteList.Add(set);
				}

				GUILayout.EndHorizontal();
			}

			foreach(var set in toDeleteList)
			{
				_data.data.Remove(set);
			}
			if (toDeleteList.Count > 0)
			{
				SaveData(SaveKey);
			}

			GUILayout.Space(30);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("+"))
			{
				_data.data.Add(new ReplaceSet { ToFindStr = "ToFindStr", ToReplaceStr = "ToReplaceStr" });
				SaveData(SaveKey);
			}
			if (GUILayout.Button("-"))
			{
				_data.data.RemoveAt(_data.data.Count - 1);
				SaveData(SaveKey);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndScrollView();
		}

		public static string HandleCopyPaste(int controlID)
		{
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			if (controlID == GUIUtility.keyboardControl)
			{
				if (Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
				{
					if (Event.current.keyCode == KeyCode.C)
					{
						Event.current.Use();
						TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						editor.Copy();
					}
					else if (Event.current.keyCode == KeyCode.V)
					{
						Event.current.Use();
						TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						editor.Paste();
						return editor.text;
					}
				}
			}
#endif
			return null;
		}

		public static string TextField(string value, params GUILayoutOption[] options)
		{
			int textFieldID = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;
			if (textFieldID == 0)
				return value;

			// Handle custom copy-paste
			value = HandleCopyPaste(textFieldID) ?? value;

			return GUILayout.TextField(value);
		}
	}
}
