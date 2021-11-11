using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Text;

using cPackage.Tools;
using Semver;

namespace PPM
{
	public class PackageChangeLogWindow : EditorWindow
	{
        Rect _background = new Rect(0, 0, 1024, 720);
        Color _backgroundColor = new Color(45 / 255f, 45 / 255f, 48 / 255f);

		private GUIStyle _textFieldStyle;
		private GUIStyle _labelStyle;
		private GUIStyle _toggleStyle;

		private static string _title = "版本日志";

		public static void OpenWindow(PPMPackageInfo packageInfo)
		{
			PackageChangeLogWindow window = GetWindow<PackageChangeLogWindow>(true);
			window.minSize = new Vector2(300, 280);
			window.Init(packageInfo);
			window.ShowUtility();
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent(_title);
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = _title;
#else
			PPMHelper.LogError("this feature is not implemented in current unity");
#endif
		}

		private PPMPackageInfo _packageInfo;
		private int _selectedIndex;
		private List<string> _displayOptions;

		private void Init(PPMPackageInfo packageInfo)
		{
			InitGUIStyle();

			_packageInfo = packageInfo;
			_selectedIndex = _packageInfo.releases.Count - 1;
			_displayOptions = new List<string>();
			foreach(var package in _packageInfo.releases)
			{
				_displayOptions.Add(package.version);
			}
		}

		private void InitGUIStyle()
		{
			Color _guiColorBlack = new Color(86 / 255f, 86 / 255f, 90 / 255f);

			_labelStyle = new GUIStyle(PPMHelper.IsProSkin() ? EditorStyles.label : EditorStyles.whiteLabel)
			{
				fontSize = 12
			};

			_textFieldStyle = new GUIStyle(EditorStyles.textField);
			_textFieldStyle.normal.background = PPMHelper.MakeTex(10, 10, _guiColorBlack);
			_textFieldStyle.hover.background = PPMHelper.MakeTex(10, 10, _guiColorBlack);
			_textFieldStyle.focused.background = PPMHelper.MakeTex(10, 10, _guiColorBlack);
			if (!PPMHelper.IsProSkin())
			{
				_textFieldStyle.normal.textColor = Color.white;
				_textFieldStyle.hover.textColor = Color.white;
				_textFieldStyle.focused.textColor = Color.white;
			}
			_textFieldStyle.fontSize = 12;
			_textFieldStyle.wordWrap = true;

			_toggleStyle = new GUIStyle(EditorStyles.toggle);
			if (!PPMHelper.IsProSkin())
			{
				_toggleStyle.normal.textColor = Color.white;
				_toggleStyle.focused.textColor = Color.white;
				_toggleStyle.hover.textColor = Color.white;
				_toggleStyle.onHover.textColor = Color.white;
				_toggleStyle.onNormal.textColor = Color.white;
				_toggleStyle.onFocused.textColor = Color.white;
				_toggleStyle.onActive.textColor = Color.white;
			}
		}

		void OnGUI()
		{
			EditorGUI.DrawRect(_background, _backgroundColor);
			DrawUI();
		}

		private void DrawUI()
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("版本:", _labelStyle);
			_selectedIndex = EditorGUILayout.Popup(_selectedIndex, _displayOptions.ToArray());
			GUILayout.EndHorizontal();

			GUILayout.Space(15);
			TextField(GetChangeLogByVersion(_displayOptions[_selectedIndex]), _textFieldStyle);
			GUILayout.Space(10);
		}

		private string GetChangeLogByVersion(string version)
		{
			foreach (var package in _packageInfo.releases)
			{
				if (package.version.Equals(version))
					return package.changeLog;
			}

			return "";
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

		public static string TextField(string value, GUIStyle style, params GUILayoutOption[] options)
		{
			int textFieldID = GUIUtility.GetControlID("TextArea".GetHashCode(), FocusType.Keyboard) + 1;
			if (textFieldID == 0)
				return value;

			// Handle custom copy-paste
			value = HandleCopyPaste(textFieldID) ?? value;

			return GUILayout.TextField(value, style, options);
		}
	}
}