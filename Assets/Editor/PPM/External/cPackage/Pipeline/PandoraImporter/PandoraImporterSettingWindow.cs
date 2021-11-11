
using System;
using UnityEngine;
using UnityEditor;

namespace cPackage.Pipeline.CSharp
{
	public class PandoraImporterSettingWindow : EditorWindow
	{
		public static void OpenSettingWindow()
		{
			PandoraImporterSettingWindow window = (PandoraImporterSettingWindow)EditorWindow.GetWindow(typeof(PandoraImporterSettingWindow), true, "Pandora Import Setting");
			window.ShowUtility();
			window.InitConfiguration();
		}

		private void InitConfiguration()
		{
			_nGUISelected = Convert.ToBoolean(PlayerPrefs.GetString("cPackage.Pipeline.CSharp.nGUI", "true"));
			_uGUISelected = Convert.ToBoolean(PlayerPrefs.GetString("cPackage.Pipeline.CSharp.uGUI", "true"));
		}

		private bool _nGUISelected;
		private bool _uGUISelected;

		private void OnGUI()
		{
			GUILayout.Space(10);
			EditorGUI.BeginChangeCheck();
			_nGUISelected = GUILayout.Toggle(_nGUISelected, new GUIContent(" use nGUI", ""));
			if (EditorGUI.EndChangeCheck())
			{
				PlayerPrefs.SetString("cPackage.Pipeline.CSharp.nGUI", _nGUISelected.ToString());
				PlayerPrefs.Save();
			}

			EditorGUI.BeginChangeCheck();
			_uGUISelected = GUILayout.Toggle(_uGUISelected, new GUIContent(" use uGUI", ""));
			if (EditorGUI.EndChangeCheck())
			{
				PlayerPrefs.SetString("cPackage.Pipeline.CSharp.uGUI", _uGUISelected.ToString());
				PlayerPrefs.Save();
			}
		}
	}
}