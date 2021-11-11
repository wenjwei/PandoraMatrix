
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using YamlDotNet.RepresentationModel;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
	[ExportPrePipelineFeature("\\.prefab$")]
	public class AddScriptName : BaseFeatureProcess
	{
		public override void ProcessFeature(string filePath)
		{
			// Get the file Path in Project
			string filePathInProject = cPackageHelper.ReadStringFromFile(filePath.Replace("/asset", "/pathname"));
			GameObject gameObject = null;
			gameObject = AssetDatabase.LoadAssetAtPath(filePathInProject, typeof(GameObject)) as GameObject;

			anchorDocumentDict.Clear();
			transformYamlDocumentDict.Clear();

			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			foreach (var document in documents)
			{
				anchorDocumentDict.Add(document.RootNode.Anchor, document);
			}

#if UNITY_2018_1_OR_NEWER
			YamlDocument rootTransformDocument = null;
			foreach (var document in anchorDocumentDict)
			{
				YamlNode node = document.Value.RootNode["Transform"] ?? document.Value.RootNode["RectTransform"];
				if (node != null && node["m_Father"]["fileID"].ToString() == "0")
				{
					rootTransformDocument = document.Value;
					break;
				}
			}
#else
			YamlDocument prefabDocument = cPackageHelper.GetYamlDocumentByKeyName(documents, "Prefab");
			string rootGameObjectAnchor = prefabDocument.RootNode["Prefab"]["m_RootGameObject"]["fileID"].ToString();
			YamlDocument rootGameObjectDocument = anchorDocumentDict[rootGameObjectAnchor];
			string rootTransformAnchor = rootGameObjectDocument.RootNode["GameObject"]["m_Component"][0][0]["fileID"].ToString();
			YamlDocument rootTransformDocument = anchorDocumentDict[rootTransformAnchor];
#endif

			MakeTransformConnectYamlDocument(gameObject.transform, rootTransformDocument);

			TraverseAllComponentsAndYamlDocuments(transformYamlDocumentDict);
			TraverseAllGameObjectYamlDocuments(anchorDocumentDict);

			WriteAnchorDocumentDictToFile(filePath);
		}

		static void WriteAnchorDocumentDictToFile(string filePath)
		{
			List<YamlDocument> writeDocuments = new List<YamlDocument>();
			foreach (var keyValuePair in anchorDocumentDict)
			{
				writeDocuments.Add(keyValuePair.Value);
			}

			cPackageHelper.WriteYamlDocuments(filePath, writeDocuments);
		}

		static void TraverseAllGameObjectYamlDocuments(Dictionary<string, YamlDocument> traverseDict)
		{
			string gameObjectFullName = typeof(GameObject).AssemblyQualifiedName;
			foreach (var tr in traverseDict)
			{
				YamlDocument curDocument = tr.Value;

				YamlNode node = curDocument.RootNode["GameObject"];
				if (node != null)
				{
					YamlMappingNode rootNode = curDocument.RootNode as YamlMappingNode;
					rootNode.Add("ScriptName", gameObjectFullName);
				}
			}
		}

		static void TraverseAllComponentsAndYamlDocuments(Dictionary<Transform, YamlDocument> traverseDict)
		{
			foreach (var tr in traverseDict)
			{
				GameObject curGameObject = tr.Key.gameObject;
				YamlDocument curDocument = tr.Value;

				YamlNode node = curDocument.RootNode["RectTransform"] ?? curDocument.RootNode["Transform"];
				string gameObjectAnchor = node["m_GameObject"]["fileID"].ToString();
				YamlSequenceNode sequenceNode = anchorDocumentDict[gameObjectAnchor].RootNode["GameObject"]["m_Component"] as YamlSequenceNode;

				Component[] components = curGameObject.GetComponents(typeof(Component));
				for (int idx = 0; idx < components.Length; idx++)
				{
					YamlDocument componentDocument = anchorDocumentDict[sequenceNode[idx][0]["fileID"].ToString()];
					YamlMappingNode rootNode = componentDocument.RootNode as YamlMappingNode;
					if (components[idx] != null)
					{
						rootNode.Add("ScriptName", components[idx].GetType().AssemblyQualifiedName);
					}
					else
					{
						rootNode.Add("ScriptName", "Missing");
					}
				}
			}
		}

		static Dictionary<string, YamlDocument> anchorDocumentDict = new Dictionary<string, YamlDocument>();

		static Dictionary<Transform, YamlDocument> transformYamlDocumentDict = new Dictionary<Transform, YamlDocument>();
		static void MakeTransformConnectYamlDocument(Transform rootTranform, YamlDocument rootDocument)
		{
			transformYamlDocumentDict.Add(rootTranform, rootDocument);

			var transformNode = rootDocument.RootNode["RectTransform"] ?? rootDocument.RootNode["Transform"];
			YamlSequenceNode sequenceChild = transformNode["m_Children"] as YamlSequenceNode;

			for (int idx = 0; idx < rootTranform.childCount; idx++)
			{
				Transform child = rootTranform.GetChild(idx);
				string anchorId = sequenceChild[idx]["fileID"].ToString();
				YamlDocument anchorDocument = anchorDocumentDict[anchorId];

				MakeTransformConnectYamlDocument(child, anchorDocument);
			}
		}
	}
}
