using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using YamlDotNet.RepresentationModel;
using cPackage.Package;
using Newtonsoft.Json;

namespace cPackage.Tools
{
	public class cPackageHelper
	{
		#region File & Direcotry

		public static void CreateDirectory(FileInfo fi, bool recreate = false)
		{
			if (fi.Directory.Exists && recreate)
				Directory.Delete(fi.Directory.FullName, true);
			Directory.CreateDirectory(fi.Directory.FullName);
		}

		public static string UnityPathToSystemPath(string unityPath)
		{
			 if (unityPath.IndexOf("StreamingAssets") != -1)
            {
                int index = unityPath.IndexOf("Assets");
                unityPath = unityPath.Substring(index + 7);
                string target = Path.Combine(Application.dataPath, unityPath);
                return target;
            }
            else
            {
                return unityPath.Replace("Assets", Application.dataPath);
            }
		}

		public static string SystemPathToUnityPath(string systemPath)
		{
			return systemPath.Replace(Application.dataPath, "Assets");
		}

		public static void WriteStingToFile(string filePath, string str)
		{
			if (File.Exists(filePath))
				File.Delete(filePath);

			FileStream fs = new FileStream(filePath, FileMode.CreateNew);
			StreamWriter sw = new StreamWriter(fs);

			sw.Write(str);

			sw.Close();
			fs.Close();
		}

		public static string ReadStringFromFile(string filePath)
		{
			if (!File.Exists(filePath))
				return string.Empty;

			FileStream fs = new FileStream(filePath, FileMode.Open);
			StreamReader sr = new StreamReader(fs);

			string str = sr.ReadToEnd();

			sr.Close();
			fs.Close();

			return str;
		}

		public static string GetUniqueFilePath(string filePath)
		{
			int n = 1;
			while (File.Exists(filePath) || Directory.Exists(filePath))
			{
				filePath = string.Format("{0} {1}", filePath, n++);
			}
			return filePath;
		}

		#endregion

		#region YAML

		public static List<YamlDocument> LoadYamlDocuments(string filePath)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open);
			StreamReader sr = new StreamReader(fs);

			YamlStream ys = new YamlStream();
			ys.Load(sr);

			sr.Close();
			fs.Close();

			return ys.Documents as List<YamlDocument>;
		}

		public static void WriteYamlDocuments(string filePath, List<YamlDocument> documents, bool writeU3dHead = true)
		{
			if (File.Exists(filePath))
				File.Delete(filePath);

			FileStream fs = new FileStream(filePath, FileMode.CreateNew);
			StreamWriter sw = new StreamWriter(fs);

			if (writeU3dHead == true)
			{
				sw.WriteLine("%YAML 1.1");
				sw.WriteLine("%TAG !u! tag:unity3d.com,2011:");
			}

			YamlStream ys = new YamlStream(documents);
			ys.Save(sw, false);

			sw.Close();
			fs.Close();
		}

		public static YamlDocument GetMonoBehaviourYamlDocumentHeader()
		{
			string headerOutputPath = Application.dataPath + "/BundleSystem/generated/Custom/CustomClassSerializeHeader.yaml";
			List<YamlDocument> documents = LoadYamlDocuments(headerOutputPath);
			return documents[0];
		}

		public static YamlNode GetUnityBuiltinYamlNode(Type type)
		{
			string outputPath = string.Format("{0}/BundleSystem/generated/Custom/CustomClassTemplate{1}SerializeInfo.yaml",
				Application.dataPath, type.Name.ToString());

			YamlDocument document = LoadYamlDocuments(outputPath)[0];
			return document.RootNode[type.Name.ToString().ToLower()];
		}

		public static YamlDocument GetYamlDocumentByKeyName(List<YamlDocument> yamlDocuments, string yamlKey)
		{
			foreach (var yamlDocument in yamlDocuments)
			{
				var yamlNode = yamlDocument.RootNode[yamlKey];
				if (yamlNode != null)
					return yamlDocument;
			}

			return null;
		}

		public static YamlDocument GetYamlDocumentByScriptName(List<YamlDocument> yamlDocuments, string scriptName)
		{
			foreach (var yamlDocument in yamlDocuments)
			{
				var yamlNode = yamlDocument.RootNode["ScriptName"];
				if (yamlNode != null && yamlNode.Equals(new YamlScalarNode(scriptName)))
					return yamlDocument;
			}

			return null;
		}

		public static string GetAnchorIdByComponentIndex(YamlDocument yamlDocument, int componentSequence)
		{
			if (yamlDocument.RootNode.NodeType != YamlNodeType.Mapping)
				return null;

			var node = yamlDocument.RootNode as YamlMappingNode;
			return node["GameObject"]["m_Component"][componentSequence][0]["fileID"].ToString();
		}

		public static YamlDocument GetYamlDocumentByAnchorId(List<YamlDocument> yamlDocuments, string anchorId)
		{
			foreach (var yamlDocument in yamlDocuments)
			{
				if (yamlDocument.RootNode.Anchor.Equals(anchorId))
					return yamlDocument;
			}

			return null;
		}

		public static YamlNode GetMScriptNodeByType(Type type)
		{
			if (type == null)
				return null;

			YamlDocument document = YamlGenerator.GetYamlDocumentByType(type);
			return document != null ? document.RootNode[0]["m_Script"] : null;
		}

		public static YamlDocument CompareAndGenerateSuitableSerializeStruct(YamlDocument inputDocument, YamlDocument referenceDocument)
		{
			return CompareAndGenerateSuitableSerializeStruct(inputDocument, referenceDocument, null);
		}

		public static YamlDocument CompareAndGenerateSuitableSerializeStruct(YamlDocument inputDocument, YamlDocument referenceDocument, string filterStr)
		{
			if (referenceDocument == null)
				return null;

			referenceDocument.RootNode.Anchor = inputDocument.RootNode.Anchor;
			referenceDocument.RootNode.Tag = inputDocument.RootNode.Tag;
			TraverseYamlNode(inputDocument.RootNode, referenceDocument.RootNode, filterStr);
			return referenceDocument;
		}

		private static void TraverseYamlNode(YamlNode inputNode, YamlNode referenceNode, string filterStr)
		{
			if (inputNode == null || referenceNode == null)
				return;

			if (inputNode.NodeType != referenceNode.NodeType)
				return;

			switch (referenceNode.NodeType)
			{
				case YamlNodeType.Mapping:
					{
						YamlMappingNode refNode = referenceNode as YamlMappingNode;
						YamlMappingNode inNode = inputNode as YamlMappingNode;
						Dictionary<string, YamlNode> remainDict = new Dictionary<string, YamlNode>();
						foreach (var child in refNode.Children)
						{
							// compare key (key type always is scalar)
							YamlScalarNode keyNode = child.Key as YamlScalarNode;
							YamlNode inValueNode = inNode[keyNode];
							if (inValueNode != null)
							{
								if (keyNode.ToString().Equals(filterStr))
								{
									remainDict.Add(keyNode.ToString(), inValueNode);
								}
								else
								{
									TraverseYamlNode(inValueNode, child.Value, filterStr);
								}
							}
						}

						foreach (var replace in remainDict)
						{
							refNode.Update(replace.Key, replace.Value);
						}

						break;
					}
				case YamlNodeType.Sequence:
					{
						YamlSequenceNode refNode = referenceNode as YamlSequenceNode;
						YamlSequenceNode inNode = inputNode as YamlSequenceNode;

						if (refNode.Children.Count == 0)
						{
							refNode.Style = inNode.Style;
							foreach (var child in inNode.Children)
							{
								refNode.Children.Add(child);
							}
						}
						else    // nested structure
						{
							YamlMappingNode refChildNode = refNode[0] as YamlMappingNode;
							refNode.Del(refChildNode);
							foreach (var child in inNode.Children)
							{
								YamlMappingNode node = new YamlMappingNode(refChildNode.Children);
								refNode.Add(node);
								TraverseYamlNode(child, node, filterStr);
							}
						}

						break;
					}
				case YamlNodeType.Scalar:
					{
						YamlScalarNode refNode = referenceNode as YamlScalarNode;
						YamlScalarNode inNode = inputNode as YamlScalarNode;
						refNode.Style = inNode.Style;
						refNode.Value = inNode.Value;
						break;
					}
				default:
					{
						Debug.LogError("unsupported type: " + referenceNode.NodeType.ToString());
						break;
					}
			}
		}

		#endregion

		#region Misc

		public static bool IsAdvancedMode()
		{
			return GetConfiguration().AdvancedMode;
		}

		static string Get7ZToolPath()
		{
#if UNITY_EDITOR_OSX
			return string.Format("{0}/Tools/7za", EditorApplication.applicationContentsPath);
#else
			return string.Format("{0}/Tools/7z.exe", EditorApplication.applicationContentsPath);
#endif
		}

		public static bool IsProSkin()
		{
			return EditorGUIUtility.isProSkin;
		}

		public static List<string> GetDependencies(List<string> guids)
		{
			List<string> dependencies = new List<string>();
			List<string> toFindList = new List<string>(guids);
			List<string> alreadyFindList = new List<string>();
			while(toFindList.Count > 0)
			{
				string toFind = toFindList[0];
				toFindList.RemoveAt(0);
				alreadyFindList.Add(toFind);

				string[] findedDependencies = AssetDatabase.GetDependencies(new string[] { AssetDatabase.GUIDToAssetPath(toFind) });
				foreach (var findedDependency in findedDependencies)
				{
					if (!dependencies.Contains(findedDependency))
						dependencies.Add(AssetDatabase.AssetPathToGUID(findedDependency));

					if (!alreadyFindList.Contains(findedDependency))
						toFindList.Add(findedDependency);
				}
			}
			return dependencies;
		}

		public static List<string> GetCSharpScriptGuids()
		{
			List<string> scriptGuids = new List<string>();
			if (cPackageHelper.GetConfiguration().IncludeCSharp)
			{
				string[] csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
				foreach(var cs in csFiles)
				{
					string unityPath = SystemPathToUnityPath(cs);
					scriptGuids.Add(AssetDatabase.AssetPathToGUID(unityPath));
				}
			}
			return scriptGuids;
		}

		public static T GetClassFieldAttribute<T>(Type clsType, string fieldName)
		{
			FieldInfo fi = clsType.GetField(fieldName);
			if (fi == null)
				return default(T);

			object[] attributes = fi.GetCustomAttributes(false);
			foreach (var attribute in attributes)
			{
				if (attribute is T)
					return (T)attribute;
			}
			return default(T);
		}

		public static List<FieldInfo> GetClassFieldsWithCustomAttribute<T>(Type clsType)
		{
			List<FieldInfo> fieldInfos = new List<FieldInfo>();
			FieldInfo[] fis = clsType.GetFields();
			foreach(var fi in fis)
			{
				T attr = GetClassFieldAttribute<T>(clsType, fi.Name);
				if (attr != null)
					fieldInfos.Add(fi);
			}
			return fieldInfos;
		}

		public static T GetClassAttribute<T>(Type clsType)
		{
			object[] attributes = clsType.GetCustomAttributes(false);
			foreach (var attribute in attributes)
			{
				if (attribute is T)
					return (T)attribute;
			}
			return default(T);
		}

		public static List<Type> GetPipelineFeatures<T>()
		{
			List<Type> types = new List<Type>();

			foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (ass.FullName.StartsWith("Assembly-CSharp-Editor"))
				{
					foreach (var type in ass.GetTypes())
					{
						T attr = GetClassAttribute<T>(type);
						if (attr != null)
							types.Add(type);
					}
				}
			}

			return types;
		}

		public static string GetGenerateDirectory()
		{
			return Application.dataPath + "/cPackage/Generated/";
		}

		public static string GetPrefabGenerateDirectory()
		{
			return GetGenerateDirectory() + "Prefab/";
		}

		public static string GetYamlGenerateDirectory()
		{
			return GetGenerateDirectory() + "YAML/";
		}

		public const string cPackageVersion = "1.7.0";

		public static void Fix45And46EditorBug(string prefabPath)
		{
#if UNITY_4_5 || UNITY_4_6
			// fix 4.5 and 4.6 editor prefab create bug
			AssetDatabase.Refresh();
			AssetDatabase.CopyAsset(SystemPathToUnityPath(prefabPath), SystemPathToUnityPath(prefabPath) + ".tmp");
			AssetDatabase.DeleteAsset(SystemPathToUnityPath(prefabPath) + ".tmp");
#endif
		}

		public static string GetGameObjectSerializedVersion()
		{
			if (String.IsNullOrEmpty(GetConfiguration().GameObjectSerializedVersion))
			{
				string tmpPath = Application.dataPath + "/tmp.prefab";
				GameObject gameObject = new GameObject();
				

#if UNITY_2018_3_OR_NEWER
				// https://docs.unity3d.com/2018.3/Documentation/Manual/UpgradeGuide20183.html
				PrefabUtility.SaveAsPrefabAsset(gameObject, SystemPathToUnityPath(tmpPath));
#else
				PrefabUtility.CreatePrefab(SystemPathToUnityPath(tmpPath), gameObject);
#endif

				Fix45And46EditorBug(tmpPath);
				List<YamlDocument> documents = LoadYamlDocuments(tmpPath);
				GetConfiguration().GameObjectSerializedVersion = GetYamlDocumentByKeyName(documents, "GameObject").RootNode["GameObject"]["serializedVersion"].ToString();
				AssetDatabase.DeleteAsset(SystemPathToUnityPath(tmpPath));
				GameObject.DestroyImmediate(gameObject);
			}
			return GetConfiguration().GameObjectSerializedVersion;
		}

		private static Configuration _configuration;
		private const string ConfigurationSaveKey = "cPackage.Configuration";

		public static Configuration GetConfiguration()
		{
			if (_configuration == null)
			{
				string saveValue = PlayerPrefs.GetString(ConfigurationSaveKey);
				_configuration = JsonConvert.DeserializeObject<Configuration>(saveValue);
				if (_configuration == null)
					_configuration = new Configuration();

			}
			return _configuration;
		}

		public static void SaveConfiguration()
		{
			string saveValue = JsonConvert.SerializeObject(_configuration);
			PlayerPrefs.SetString(ConfigurationSaveKey, saveValue);
			PlayerPrefs.Save();
		}

		public static bool IsUGUISupported()
		{
			Type type = Type.GetType("UnityEngine.RectTransform, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			return type != null;
		}

		public static bool IsSettingsReady()
		{
			if (EditorSettings.serializationMode != SerializationMode.ForceText)
			{
				bool changeSetting = EditorUtility.DisplayDialog("Warning", @"cPackage only support ForceText Settings, please modify your project settings

Edit->Project Settings->Editor->Asset Serialization", "change setting!", "not change!");

				if (changeSetting)
				{
					EditorSettings.serializationMode = SerializationMode.ForceText;
					TemplateGenerator.OnScriptsReloaded();
				}
				return changeSetting;
			}

			return true;
		}

		#endregion

		#region Compress & Decompress

		public static bool Compress(string inputFile, string outputFile, bool openExplorer)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			process.StartInfo = startInfo;
			startInfo.FileName = Get7ZToolPath();

			string tempGzipPath = Application.temporaryCachePath + "/temp.tar.gz";
			if (File.Exists(tempGzipPath))
				File.Delete(tempGzipPath);
			startInfo.Arguments = string.Format("a -tgzip -y -bd {0} {1}", tempGzipPath, inputFile);
			process.Start();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				Debug.LogError("cPackage: compress file failed! input:" + inputFile);
				return false;
			}

			if (File.Exists(outputFile))
				File.Delete(outputFile);
			File.Move(tempGzipPath, outputFile);
			File.Delete(tempGzipPath);

			if (openExplorer)
			{
				FileInfo fi = new FileInfo(outputFile);
				if (fi != null)
					Application.OpenURL("file://" + fi.Directory.FullName);
			}

			return true;
		}

		public static bool Decompress(string inputFile, string outputDir)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			process.StartInfo = startInfo;
			startInfo.FileName = Get7ZToolPath();
			startInfo.Arguments = string.Format("x -r -y -bd -o\"{0}\" \"{1}\"", outputDir, inputFile);
			process.Start();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				Debug.LogError("cPackage: decompress file failed! input:" + inputFile);
				return false;
			}

			return true;
		}

		#endregion

		#region Tar & Untar

		public static bool Tar(string inputDir, string outputFile)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			process.StartInfo = startInfo;
			startInfo.FileName = Get7ZToolPath();

			if (File.Exists(outputFile))
				File.Delete(outputFile);
			startInfo.Arguments = string.Format("a -ttar -y -bd \"{0}\" \"{1}\"", outputFile, inputDir);
			process.Start();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				Debug.LogError("cPackage: tar file failed! input:" + inputDir);
				return false;
			}

			return true;
		}

		public static bool Untar(string inputFile, string outputDir)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			process.StartInfo = startInfo;
			startInfo.FileName = Get7ZToolPath();
			startInfo.Arguments = string.Format("x -r -y -bd -o\"{0}\" \"{1}\"", outputDir, inputFile);
			process.Start();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				Debug.LogError("cPackage: untar file failed! input:" + inputFile);
				return false;
			}

			return true;
		}

		#endregion

		#region Tools

		static readonly string _tempCPackageFolder = "cPackage/";

		public static string TempCPackageFolder
		{
			get
			{
				return Application.dataPath.Replace("Assets", _tempCPackageFolder);
			}
		}

		public static string CalculateMD5(string filePath)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(filePath))
				{
					var hash = md5.ComputeHash(stream);
					return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
				}
			}
		}

		#endregion

		#region Debug

		public static bool IsDebugMode()
		{
			return GetConfiguration().DebugMode;
		}

		#endregion

		#region Logger

		public static void Log(object message, UnityEngine.Object context = null)
		{
			Debug.Log(string.Format("<color={0}>cPackage Log: {1}</color>", IsProSkin() ? "cyan" : "black", message), context);
		}

		public static void LogError(object message, UnityEngine.Object context = null)
		{
			Debug.LogError(string.Format("<color={0}>cPackage Error: {1}</color>", IsProSkin() ? "red" : "red", message), context);
		}

		public static void LogWarning(object message, UnityEngine.Object context = null)
		{
			Debug.LogWarning(string.Format("<color={0}>cPackage Warning: {1}</color>", IsProSkin() ? "yellow" : "black", message), context);
		}

		#endregion
	}
}
	
