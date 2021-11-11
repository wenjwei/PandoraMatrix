
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using YamlDotNet.RepresentationModel;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
	[ImportPrePipelineFeature("\\.prefab$")]
	public class ClassYamlSerializationAdapter : BaseFeatureProcess
	{
		public override string GetProgressBarTitle()
		{
			return "cPackage Class YAML Adapter";
		}

		private List<string> GetAssemblyQualifiedNameWhiteList()
		{
			return new List<string>()
			{
				"UnityEngine.GameObject, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				"UnityEngine.Transform, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
				"UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
			};
		}

		private YamlDocument AutoFillUnityYAMLData(YamlDocument toProcessedYAMLDocument)
		{
			string componentName = toProcessedYAMLDocument.RootNode["ScriptName"].ToString();

			if (componentName.Equals("UnityEngine.UI.Image"))
			{
				YamlMappingNode spriteNode = toProcessedYAMLDocument.RootNode["MonoBehaviour"]["m_Sprite"] as YamlMappingNode;
				if (spriteNode["guid"] == null)
					spriteNode.Add("guid", "0000000000000000000000000000000f");
				if (spriteNode["type"] == null)
					spriteNode.Add("type", "0");

				YamlMappingNode materialNode = toProcessedYAMLDocument.RootNode["MonoBehaviour"]["m_Material"] as YamlMappingNode;
				if (materialNode["guid"] == null)
					materialNode.Add("guid", "0000000000000000000000000000000f");
				if (materialNode["type"] == null)
					materialNode.Add("type", "0");
			}

			return toProcessedYAMLDocument;
		}

		public override void ProcessFeature(string filePath)
		{
			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(filePath);
			List<YamlDocument> newDocuments = new List<YamlDocument>();
			List<string> assemblyQualifiedNameWhiteList = GetAssemblyQualifiedNameWhiteList();
			int updateDocumentCount = 0;

			float totalCount = documents.Count;
			for (int i = 0; i < totalCount; i++)
			{
				YamlDocument document = documents[i];
				YamlDocument newDocument = null;
				YamlNode node = document.RootNode["ScriptName"];
				if (node != null)
				{
					string assemblyQualifiedName = node.ToString();
					if (!string.IsNullOrEmpty(assemblyQualifiedName) && !assemblyQualifiedNameWhiteList.Contains(assemblyQualifiedName))
					{
						Type type = Type.GetType(assemblyQualifiedName);
						if (type != null)
						{
							if (cPackageHelper.IsDebugMode())
							{
								cPackageHelper.Log(string.Format("ClassYamlSerializationAdapter for {0}", type.AssemblyQualifiedName));
							}
							DisplayProgressBar(assemblyQualifiedName, i / totalCount);
							YamlDocument curUnityVersionDocument = YamlGenerator.GetYamlDocumentByType(type);
							newDocument = cPackageHelper.CompareAndGenerateSuitableSerializeStruct(document, AutoFillUnityYAMLData(curUnityVersionDocument));
							if (newDocument != null)
								updateDocumentCount++;
						}
					}
				}
				newDocuments.Add(newDocument ?? document);
			}

			if (updateDocumentCount > 0)
				cPackageHelper.WriteYamlDocuments(filePath, newDocuments);
		}

		#region Test Case

		//[MenuItem("cPackage/ClassYamlSerializationAdapter")]
		static void CompareAndGenerateStruct()
		{
			// sequence (scalar element / mapping element)

			string inputStr = @"
--- !u!114 &114003670977857724
MonoBehaviour:
 m_ObjectHideFlags: 1
 m_PrefabParentObject: {fileID: 0}
 m_PrefabInternal: {fileID: 100100000}
 m_GameObject: {fileID: 1652590795706176}
 m_Enabled: 1
 m_EditorHideFlags: 0
 m_Script: {fileID: 11500000, guid: ef3721ed03ae1524d97df2ef1f7a2e43, type: 3}
 m_Name: 
 m_EditorClassIdentifier: 
 vector2: {x: 0, y: 0}
 vector3: {x: 5, y: 0, z: 0}
 vector4: {x: 0, y: 0, z: 7, w: 0}
 rect:
  serializedVersion: 2
  x: 0
  y: 0
  width: 0
  height: 0
 quaternion: {x: 0, y: 0, z: 0, w: 0}
 quaternion1: {x: 0, y: 0, z: 0, w: 0}
 matrix4x4:
  e00: 0
  e01: 0
  e02: 0
  e03: 0
  e10: 0
  e11: 0
  e12: 0
  e13: 0
  e20: 0
  e21: 0
  e22: 0
  e23: 0
  e30: 0
  e31: 0
  e32: 0
  e33: 0
 color: {r: 0, g: 0, b: 0, a: 0}";

			string referenceStr = @"
---
MonoBehaviour:
 m_ObjectHideFlags: 1
 m_PrefabParentObject: {fileID: 0}
 m_PrefabInternal: {fileID: 0}
 m_GameObject: {fileID: 0}
 m_Enabled: 1
 m_EditorHideFlags: 0
 m_Script: {fileID: 0, guid: 0, type: 3}
 m_Name: 
 m_EditorClassIdentifier: 
 vector2: {x: 0, y: 0}
 vector3: {x: 0, y: 0, z: 0}
 vector4: {x: 0, y: 0, z: 0, w: 0}
 rect:
  serializedVersion: 2
  x: 0
  y: 0
  width: 0
  height: 0
 quaternion: {x: 0, y: 0, z: 0, w: 0}
 matrix4x4:
  e10: 0
  e11: 0
  e12: 0
  e13: 0
  e20: 0
  e21: 0
  e22: 0
  e23: 0
  e30: 0
  e31: 0
  e32: 0
  e33: 0
 color: {r: 1, g: 0, b: 0}";

			var input = new StringReader(inputStr);
			var inputYaml = new YamlStream();
			inputYaml.Load(input);

			var reference = new StringReader(referenceStr);
			var referenceYaml = new YamlStream();
			referenceYaml.Load(reference);

			YamlDocument outputDocument = cPackageHelper.CompareAndGenerateSuitableSerializeStruct(inputYaml.Documents[0], referenceYaml.Documents[0]);

			string outputPath = string.Format("{0}/BundleSystem/generated/Custom/SampleCompare.yaml", Application.dataPath);
			cPackageHelper.WriteYamlDocuments(outputPath, new List<YamlDocument> { outputDocument });
		}

		#endregion
	}
}
