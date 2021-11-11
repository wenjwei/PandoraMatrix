
using System.IO;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$")]
	public class UGUIFeatureChecker : BaseFeatureProcess
	{
		public override void ProcessFeature(string filePath)
		{
			if (cPackageHelper.IsUGUISupported())
				return;

			if (IsUsingUGUI(filePath))
			{
				string uguiNotSupportPrefabPath = cPackageHelper.GetPrefabGenerateDirectory() + "uGUI not support.prefab";
				if (File.Exists(uguiNotSupportPrefabPath))
				{
					File.Copy(uguiNotSupportPrefabPath, filePath, true);
				}

				string pathName = cPackageHelper.ReadStringFromFile(filePath.Replace("/asset", "/pathname"));
				cPackageHelper.LogError("Import not support uGUI prefab:" + pathName);
			}
		}

		private bool IsUsingUGUI(string filePath)
		{
			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			foreach(var document in documents)
			{
				if (document.RootNode["RectTransform"] != null)
					return true;
			}

			return false;
		}
	}
}
