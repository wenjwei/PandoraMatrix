
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using cPackage.Tools;
using cPackage.Package;

namespace cPackage.UI
{
	public class cPackageMenu
	{
		[MenuItem("cPackage/Export cPackage", true)]
		[MenuItem("cPackage/Import cPackage", true)]
		static bool IsSupported()
		{
			return EditorSettings.serializationMode == SerializationMode.ForceText;
		}

		#region Import/Export Menu

		[MenuItem("cPackage/Export cPackage", false, 5)]
		static void ExportcPackage()
		{
			List<string> guidList = new List<string>();
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
			{
				string guidStr = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
				guidList.Add(guidStr);
			}
			guidList.Sort((s1, s2) => AssetDatabase.GUIDToAssetPath(s1).CompareTo(AssetDatabase.GUIDToAssetPath(s2)));

			List<string> dependencyGuidList = new List<string>(guidList);
			dependencyGuidList.AddRange(cPackageHelper.GetDependencies(guidList));
			dependencyGuidList.AddRange(cPackageHelper.GetCSharpScriptGuids());
			dependencyGuidList.Sort((s1, s2) => AssetDatabase.GUIDToAssetPath(s1).CompareTo(AssetDatabase.GUIDToAssetPath(s2)));

			cPackageExportWindow.ShowExportTreeView(guidList.ToArray(), dependencyGuidList.ToArray());
		}

		[MenuItem("cPackage/Import cPackage", false, 6)]
		static void ImportcPackage()
		{
			string fileName = EditorUtility.OpenFilePanel("Import cPackage ...", "", "cpackage");
			if (!string.IsNullOrEmpty(fileName))
			{
				cPackageExporter.ImportCPackage(fileName, null);
			}
		}

		#endregion

		#region Configuration Menu

		[MenuItem("cPackage/Configuration", false, 12)]
		static void Configuration()
		{
			if (cPackageHelper.IsSettingsReady())
			{
				cPackageConfigurationWindow.OpenConfigurationWindow();
			}
		}

		#endregion

		#region Help Menu

		[MenuItem("cPackage/Help", false, 13)]
		static void Help()
		{
			Application.OpenURL("http://km.oa.com/group/34294/articles/show/366981");
		}

		#endregion
	}
}
