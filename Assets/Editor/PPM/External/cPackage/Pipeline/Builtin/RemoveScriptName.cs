
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$")]
	public class RemoveScriptName : BaseFeatureProcess
	{
		public override void ProcessFeature(string filePath)
		{
			if (cPackageHelper.IsDebugMode())
				return;

			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);

			foreach(var document in documents)
			{
				((YamlMappingNode)document.RootNode).Remove("ScriptName");
			}

			cPackageHelper.WriteYamlDocuments(filePath, documents);
		}
	}
}
