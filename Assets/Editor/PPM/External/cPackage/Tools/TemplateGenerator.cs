
using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace cPackage.Tools
{
	public class TemplateGenerator
	{
		[UnityEditor.Callbacks.DidReloadScripts]
		public static void OnScriptsReloaded()
		{
			if (!cPackageHelper.IsSettingsReady())
				return;

			if (!TemplateIsReady())
				GenerateTemplate();

#if !UNITY_2018_3_OR_NEWER
			cPackageHelper.GetGameObjectSerializedVersion();
#endif
		}

		private static string _uguiNotSupportPrefabPath = cPackageHelper.GetPrefabGenerateDirectory() + "uGUI not support.prefab";

		private static bool TemplateIsReady()
		{
			if (cPackageHelper.IsUGUISupported())
				return true;
			return File.Exists(_uguiNotSupportPrefabPath);
		}

		private static void GenerateTemplate()
		{
			GenerateUGUINotSupportPrefab();
		}

		private static void GenerateUGUINotSupportPrefab()
		{
			if (cPackageHelper.IsUGUISupported())
				return;

			if (File.Exists(_uguiNotSupportPrefabPath))
				File.Delete(_uguiNotSupportPrefabPath);

			GameObject gameObject = new GameObject("uGUI not support");
			GameObject childGameObject = new GameObject("uGUI not support");
			childGameObject.transform.parent = gameObject.transform;
			
#if UNITY_2018_3_OR_NEWER
			// https://docs.unity3d.com/2018.3/Documentation/Manual/UpgradeGuide20183.html
			PrefabUtility.SaveAsPrefabAsset(gameObject, cPackageHelper.SystemPathToUnityPath(_uguiNotSupportPrefabPath));
#else
			PrefabUtility.CreatePrefab(cPackageHelper.SystemPathToUnityPath(_uguiNotSupportPrefabPath), gameObject);
#endif

			GameObject.DestroyImmediate(gameObject);
			cPackageHelper.Fix45And46EditorBug(_uguiNotSupportPrefabPath);
		}
	}
}
