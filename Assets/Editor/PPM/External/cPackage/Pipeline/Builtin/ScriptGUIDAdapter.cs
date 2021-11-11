
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$")]
	public class ScriptGUIDAdapter : BaseFeatureProcess
	{
        public ScriptGUIDAdapter()
        {
            ExecutePriority = 51;
        }

        public override string GetProgressBarTitle()
		{
			return "cPackage scriptGUIDAdapter";
		}

		public override void ProcessFeature(string filePath)
		{
			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			List<string> errorMsgs = new List<string>();
			int updateDocumentCount = 0;

			float totalCount = documents.Count;
			for (int i = 0; i < totalCount; i++)
			{
				YamlDocument document = documents[i];

				YamlNode node = document.RootNode["ScriptName"];
				if (node == null)
					continue;

				if (document.RootNode[0]["m_Script"] == null)
					continue;

				string assemblyQualifiedName = node.ToString();
				if (!string.IsNullOrEmpty(assemblyQualifiedName))
				{
					Type type = Type.GetType(assemblyQualifiedName);
					DisplayProgressBar(assemblyQualifiedName, i / totalCount);
					YamlNode newNode = cPackageHelper.GetMScriptNodeByType(type);
					if (newNode != null)
					{
						(document.RootNode[0] as YamlMappingNode).Update("m_Script", newNode);
						updateDocumentCount++;
					}
					else
					{
						string errorMsg = string.Format("can not find type: {0}", assemblyQualifiedName);
						if (!errorMsgs.Contains(errorMsg))
							errorMsgs.Add(errorMsg);
					}
				}
			}

			if (updateDocumentCount > 0)
				cPackageHelper.WriteYamlDocuments(filePath, documents);

			foreach (var errorMsg in errorMsgs)
				cPackageHelper.LogWarning(errorMsg);
		}
	}
}
