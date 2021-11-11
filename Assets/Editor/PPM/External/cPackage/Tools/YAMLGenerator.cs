
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using YamlDotNet.RepresentationModel;

namespace cPackage.Tools
{
	public class YamlGenerator
	{
		private static List<YamlDocument> customDocuments;

		public static YamlDocument GetYamlDocumentByType(Type type)
		{
			if (customDocuments == null)
				customDocuments = new List<YamlDocument>();

			YamlDocument customDocument = cPackageHelper.GetYamlDocumentByScriptName(customDocuments, type.FullName);
			if (customDocument != null)
				return customDocument;

			YamlDocument document = GenerateYamlDocumentByType(type);
			if (document != null)
			{
				customDocuments.Add(document);
			}
			return document;
		}

		private static YamlDocument GenerateYamlDocumentByType(Type type)
		{
			string generateYamlDir = cPackageHelper.GetYamlGenerateDirectory();
			string generateYamlPath = string.Format("{0}{1}.generate.yaml", generateYamlDir, type.FullName);
			if (!Directory.Exists(generateYamlDir))
				cPackageHelper.CreateDirectory(new FileInfo(generateYamlDir), true);

			if (!File.Exists(generateYamlPath))
			{
				string generatePrefabDir = cPackageHelper.GetPrefabGenerateDirectory();
				if (!Directory.Exists(generatePrefabDir))
					cPackageHelper.CreateDirectory(new FileInfo(generatePrefabDir), true);

				string generatePrefabPath = string.Format("{0}{1}.prefab", cPackageHelper.SystemPathToUnityPath(generatePrefabDir), type.FullName);
				GameObject gameObject = new GameObject(type.FullName);
				var componentIndex = GenerateComponentPrefab(gameObject, type, generatePrefabPath);
				UnityEngine.Object.DestroyImmediate(gameObject);
				cPackageHelper.Fix45And46EditorBug(generatePrefabPath);
				YamlDocument extractDocument = ExtractComponentYaml(generatePrefabPath, type, componentIndex);
				cPackageHelper.WriteYamlDocuments(generateYamlPath, new List<YamlDocument> { extractDocument });

				if (cPackageHelper.IsDebugMode() == false)
					AssetDatabase.DeleteAsset(generatePrefabPath);
			}

			List<YamlDocument> documents = cPackageHelper.LoadYamlDocuments(generateYamlPath);
			return documents[0];
		}

		static Dictionary<string, string> GetComponentDependencies()
		{
			return new Dictionary<string, string>
			{
				{"UnityEngine.AudioLowPassFilter", "UnityEngine.AudioSource,UnityEngine"},
				{"UnityEngine.AudioHighPassFilter", "UnityEngine.AudioSource,UnityEngine"},
				{"UnityEngine.AudioDistortionFilter", "UnityEngine.AudioSource,UnityEngine"},
				{"UnityEngine.AudioEchoFilter", "UnityEngine.AudioSource,UnityEngine"},
				{"UnityEngine.AudioChorusFilter", "UnityEngine.AudioSource,UnityEngine"},
				{"UnityEngine.AudioReverbFilter", "UnityEngine.AudioSource,UnityEngine"},
			};
		}

		private static int GenerateComponentPrefab(GameObject gameObject, Type componentClass, string generatePrefabPath)
		{
			var componentDependencies = GetComponentDependencies();
			if (componentDependencies.ContainsKey(componentClass.FullName))
			{
				gameObject.AddComponent(Type.GetType(componentDependencies[componentClass.FullName]));
			}
#if UNITY_2019_1_OR_NEWER
            Component c = gameObject.AddComponent(componentClass);
			if (componentClass.FullName.Equals("UnityEngine.Transform"))
            {
                c = gameObject.GetComponent<Transform>();
            }
#else
			Component c = gameObject.AddComponent(componentClass);
            if (componentClass.FullName.Equals("UnityEngine.Transform"))
            {
                c = gameObject.GetComponent<Transform>();
            }
#endif

            if (c == null)
			{
				cPackageHelper.LogError("component is invalid:" + componentClass.FullName);
			}
			else
			{
#if UNITY_2018_3_OR_NEWER
				// https://docs.unity3d.com/2018.3/Documentation/Manual/UpgradeGuide20183.html
				PrefabUtility.SaveAsPrefabAsset(gameObject, generatePrefabPath);
#else
				PrefabUtility.CreatePrefab(generatePrefabPath, gameObject);
#endif
			}

			return GetComponentIndex(gameObject, componentClass.FullName);
		}

		private static int GetComponentIndex(GameObject gameObject, string componentName)
		{
			Component[] components = gameObject.GetComponents<Component>();
			for (int index = 0; index < components.Length; index++)
			{
				Component component = components[index];
				if (component != null && component.GetType().FullName.Equals(componentName))
				{
					return index;
				}
			}
			return 0;
		}

		private static YamlDocument ExtractComponentYaml(string generatePrefabPath, Type componentClass, int componentIndex)
		{
			List<YamlDocument> yamlDocuments = cPackageHelper.LoadYamlDocuments(cPackageHelper.UnityPathToSystemPath(generatePrefabPath));
			YamlDocument gameObjectDocument = cPackageHelper.GetYamlDocumentByKeyName(yamlDocuments, "GameObject");
			if (IsGameObjectSerializedVersionSupported((gameObjectDocument.RootNode as YamlMappingNode)["GameObject"]["serializedVersion"].ToString()))
			{
				string anchorId = cPackageHelper.GetAnchorIdByComponentIndex(gameObjectDocument, componentIndex);
				YamlDocument nativeComponentDocument = cPackageHelper.GetYamlDocumentByAnchorId(yamlDocuments, anchorId);
				(nativeComponentDocument.RootNode as YamlMappingNode).Add("ScriptName", componentClass.FullName);

				return nativeComponentDocument;
			}
			else
			{
				cPackageHelper.LogError("unsupported gameObject serializedVersion");
				return null;
			}
		}

		private static bool IsGameObjectSerializedVersionSupported(string version)
		{
			return version.Equals("4") || version.Equals("5") || version.Equals("6");
		}
	}
}
