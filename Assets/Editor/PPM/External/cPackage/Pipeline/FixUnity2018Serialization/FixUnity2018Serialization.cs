
using UnityEngine;
using cPackage.Tools;
using UnityEditor;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using System.IO;

namespace cPackage.Package
{
	public partial class Configuration
	{
		[ConfigurationExtensionField]
		[ConfigurationField("FixUnity2018Serialization:", "Make Unity 2018 GameObject Compatible with earlier Unity Versions.")]
		public bool FixUnity2018Serialization;
	}
}

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$", "FixUnity2018Serialization")]
	public class FixUnity2018Serialization : BaseFeatureProcess
	{
		public FixUnity2018Serialization()
		{
			ExecutePriority = 50;
		}

		public override string GetProgressBarTitle()
		{
			return "cPackage Fix Unity2018 Serialization";
		}

		private List<string> GetAssemblyQualifiedNameList()
		{
			return new List<string>()
			{
				"UnityEngine.GameObject, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				"UnityEngine.Transform, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				"UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				"UnityEngine.Transform, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				"UnityEngine.RectTransform, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
			};
		}

		enum AdapterType
		{
			None,
			Transform,
			RectTransform,
			GameObejct,
			Prefab
		}

		public override void ProcessFeature(string filePath)
		{
			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			List<YamlDocument> newDocuments = new List<YamlDocument>();
			List<string> assemblyQualifiedNameList = GetAssemblyQualifiedNameList();
			int updateDocumentCount = 0;

			bool needPatchPrefabNode = true;
			string rootGameObjectId = "";

			float totalCount = documents.Count;
			for (int i = 0; i < totalCount; i++)
			{
				YamlDocument document = documents[i];
				YamlDocument newDocument = null;

				if (document.RootNode["Prefab"] != null)
					needPatchPrefabNode = false;

                YamlNode nodeTransform = document.RootNode["Transform"] ?? document.RootNode["RectTransform"];
                if (nodeTransform != null && nodeTransform["m_Father"]["fileID"].ToString() == "0") {
                    rootGameObjectId = nodeTransform["m_GameObject"]["fileID"].ToString();
                }

                AdapterType type = document.RootNode["Prefab"] == null ? AdapterType.None : AdapterType.Prefab;
				if (type == AdapterType.Prefab)
					continue;

				if (type == AdapterType.None)
				{
					YamlNode node = document.RootNode["ScriptName"];
					if (node != null)
					{
						string assemblyQualifiedName = node.ToString();
						if (!string.IsNullOrEmpty(assemblyQualifiedName) && assemblyQualifiedNameList.Contains(assemblyQualifiedName))
						{
							type = AdapterType.GameObejct;
							if (assemblyQualifiedName.Contains("UnityEngine.Transform"))
								type = AdapterType.Transform;
							else if (assemblyQualifiedName.Contains("UnityEngine.RectTransform"))
								type = AdapterType.RectTransform;
						}
					}
				}

				if (type != AdapterType.None)
				{
					if (cPackageHelper.IsDebugMode())
					{
						cPackageHelper.Log(string.Format("FixUnity2018Serialization for {0}", type.ToString()));
					}
					DisplayProgressBar(type.ToString(), i / totalCount);
					YamlDocument curUnityVersionDocument = GenerateYamlDocument(type);
					newDocument = cPackageHelper.CompareAndGenerateSuitableSerializeStruct(document, curUnityVersionDocument, "m_Component");
					if (newDocument != null)
						updateDocumentCount++;
				}
				
				newDocuments.Add(newDocument ?? document);
			}

			if (needPatchPrefabNode)
			{
				YamlDocument document = GenerateYamlDocument(AdapterType.Prefab);
				(document.RootNode["Prefab"]["m_RootGameObject"] as YamlMappingNode).Update("fileID", rootGameObjectId);
				newDocuments.Add(document);
			}

			if (updateDocumentCount > 0)
				cPackageHelper.WriteYamlDocuments(filePath, newDocuments);
		}

		private static YamlDocument GenerateYamlDocument(AdapterType type)
		{
			string generateYamlDir = cPackageHelper.GetYamlGenerateDirectory();
			string generateYamlPath = string.Format("{0}{1}.generate.yaml", generateYamlDir, type.ToString());
			if (!Directory.Exists(generateYamlDir))
				cPackageHelper.CreateDirectory(new FileInfo(generateYamlDir), true);

			if (!File.Exists(generateYamlPath))
			{
				string generatePrefabDir = cPackageHelper.GetPrefabGenerateDirectory();
				if (!Directory.Exists(generatePrefabDir))
					cPackageHelper.CreateDirectory(new FileInfo(generatePrefabDir), true);

				string generatePrefabPath = string.Format("{0}{1}.prefab", cPackageHelper.SystemPathToUnityPath(generatePrefabDir), type.ToString());
				GameObject gameObject = new GameObject(type.ToString());
				if (type == AdapterType.RectTransform)
					gameObject.AddComponent<RectTransform>();
#if UNITY_2018_3_OR_NEWER
				// https://docs.unity3d.com/2018.3/Documentation/Manual/UpgradeGuide20183.html
				PrefabUtility.SaveAsPrefabAsset(gameObject, generatePrefabPath);
#else
				PrefabUtility.CreatePrefab(generatePrefabPath, gameObject);
#endif
				UnityEngine.Object.DestroyImmediate(gameObject);
				cPackageHelper.Fix45And46EditorBug(generatePrefabPath);
				List<YamlDocument> yamlDocuments = cPackageHelper.LoadYamlDocuments(cPackageHelper.UnityPathToSystemPath(generatePrefabPath));
				YamlDocument document = null;
				switch(type)
				{
					case AdapterType.GameObejct:
						document = cPackageHelper.GetYamlDocumentByKeyName(yamlDocuments, "GameObject");
						break;
					case AdapterType.Transform:
						document = cPackageHelper.GetYamlDocumentByKeyName(yamlDocuments, "Transform");
						break;
					case AdapterType.Prefab:
						document = cPackageHelper.GetYamlDocumentByKeyName(yamlDocuments, "Prefab");
						break;
					case AdapterType.RectTransform:
						document = cPackageHelper.GetYamlDocumentByKeyName(yamlDocuments, "RectTransform");
						break;
				}
				(document.RootNode as YamlMappingNode).Add("ScriptName", type.ToString());
				cPackageHelper.WriteYamlDocuments(generateYamlPath, new List<YamlDocument> { document });

				if (cPackageHelper.IsDebugMode() == false)
					AssetDatabase.DeleteAsset(generatePrefabPath);
			}

			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(generateYamlPath);
			return documents[0];
		}
	}
}