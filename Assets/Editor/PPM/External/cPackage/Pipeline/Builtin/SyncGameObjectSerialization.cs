
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$")]
	public class SyncGameObjectSerialization : BaseFeatureProcess
	{
		public override void ProcessFeature(string filePath)
		{
			string serializedVersionStr = cPackageHelper.GetGameObjectSerializedVersion();

			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			bool notChanged = false;
			foreach(var document in documents)
			{
				if (document.RootNode["GameObject"] != null)
				{
					if (serializedVersionStr == document.RootNode["GameObject"]["serializedVersion"].ToString())
					{
						notChanged = true;
						break;
					}

					if (serializedVersionStr.Equals("4"))
						TransferSerializedVersion4(document.RootNode["GameObject"], documents);
					else if (serializedVersionStr.Equals("5"))
						TransferSerializedVersion5(document.RootNode["GameObject"], documents);
					else if (serializedVersionStr.Equals("6"))
						TransferSerializedVersion6(document.RootNode["GameObject"], documents);
					else
						cPackageHelper.LogError("unsupported gameObject Serialized Version:" + serializedVersionStr);
				}
			}

			if (!notChanged)
			{
				cPackageHelper.WriteYamlDocuments(filePath, documents);
			}
		}

		private void TransferSerializedVersion4(YamlNode node, List<YamlDocument> refDocuments)
		{
			// serializedVersion: 4
			// m_Component:
			// - 4: { fileID: 4504073673795910}
			// - 114: { fileID: 114776184175215732}

			YamlSequenceNode oldNode = node["m_Component"] as YamlSequenceNode;
			YamlSequenceNode newNode = new YamlSequenceNode();

			for (int i = 0; i < oldNode.Children.Count; i++)
			{
				YamlMappingNode fileIdMapping = new YamlMappingNode(0);
				fileIdMapping.Style = YamlDotNet.Core.Events.MappingStyle.Flow;
				fileIdMapping.Add("fileID", oldNode[i][0]["fileID"]);

				YamlDocument anchorDocument = cPackageHelper.GetYamlDocumentByAnchorId(refDocuments, oldNode[i][0]["fileID"].ToString());

				YamlMappingNode sequenceMapping = new YamlMappingNode(0);
				sequenceMapping.Add(anchorDocument.RootNode.Tag.Replace("tag:unity3d.com,2011:", ""), fileIdMapping);

				newNode.Add(sequenceMapping);
			}

			YamlMappingNode mappingNode = node as YamlMappingNode;
			mappingNode.Update("m_Component", newNode);
			mappingNode.Update("serializedVersion", "4");
		}

		private void TransferSerializedVersion5(YamlNode node, List<YamlDocument> refDocuments)
		{
			// serializedVersion: 5
			// m_Component:
			// - component: { fileID: 4504073673795910}
			// - component: { fileID: 114776184175215732}

			YamlSequenceNode oldNode = node["m_Component"] as YamlSequenceNode;
			YamlSequenceNode newNode = new YamlSequenceNode();

			for (int i = 0; i < oldNode.Children.Count; i++)
			{
				YamlMappingNode fileIdMapping = new YamlMappingNode(0);
				fileIdMapping.Style = YamlDotNet.Core.Events.MappingStyle.Flow;
				fileIdMapping.Add("fileID", oldNode[i][0]["fileID"]);

				YamlMappingNode sequenceMapping = new YamlMappingNode(0);
				sequenceMapping.Add("component", fileIdMapping);

				newNode.Add(sequenceMapping);
			}

			YamlMappingNode mappingNode = node as YamlMappingNode;
			mappingNode.Update("m_Component", newNode);
			mappingNode.Update("serializedVersion", "5");
		}

		private void TransferSerializedVersion6(YamlNode node, List<YamlDocument> refDocuments)
		{
			// serializedVersion: 6
			// m_Component:
			// - component: { fileID: 4504073673795910}
			// - component: { fileID: 114776184175215732}

			YamlSequenceNode oldNode = node["m_Component"] as YamlSequenceNode;
			YamlSequenceNode newNode = new YamlSequenceNode();

			for (int i = 0; i < oldNode.Children.Count; i++)
			{
				YamlMappingNode fileIdMapping = new YamlMappingNode(0);
				fileIdMapping.Style = YamlDotNet.Core.Events.MappingStyle.Flow;
				fileIdMapping.Add("fileID", oldNode[i][0]["fileID"]);

				YamlMappingNode sequenceMapping = new YamlMappingNode(0);
				sequenceMapping.Add("component", fileIdMapping);

				newNode.Add(sequenceMapping);
			}

			YamlMappingNode mappingNode = node as YamlMappingNode;
			mappingNode.Update("m_Component", newNode);
			mappingNode.Update("serializedVersion", "6");
		}
	}
}
