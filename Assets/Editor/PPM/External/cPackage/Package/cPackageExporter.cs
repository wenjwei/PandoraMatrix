using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;
using UnityEditor;

using cPackage.Tools;
using cPackage.UI;
using cPackage.Pipeline;

namespace cPackage.Package
{
	public class cPackageExporter
	{
		#region Export Interface

		public static void ExportCPackage(string[] assetPathNames, string exportPackageName)
		{
			if (assetPathNames == null || assetPathNames.Length == 0)
			{
				cPackageHelper.LogWarning("assetPathNames is empty, export failed");
				return;
			}

			if (string.IsNullOrEmpty(exportPackageName))
			{
				cPackageHelper.LogWarning("exportPackageName is empty, export failed");
				return;
			}

			List<string> exportGuidList = new List<string>();
			foreach (var assetPath in assetPathNames)
			{
				string assetSystemPath = cPackageHelper.UnityPathToSystemPath(assetPath);
				if (File.Exists(assetSystemPath))
				{
					string guid = AssetDatabase.AssetPathToGUID(assetPath);
					if (!exportGuidList.Contains(guid)&& !string.IsNullOrEmpty(guid))
						exportGuidList.Add(guid);
				}
				else if (Directory.Exists(assetSystemPath))
				{
					string[] allFiles = Directory.GetFiles(assetSystemPath, "*.*", SearchOption.AllDirectories);
					foreach(var file in allFiles)
					{
						if (file.EndsWith(".meta") || file.Contains(".git"))
							continue;
						string guid = AssetDatabase.AssetPathToGUID(cPackageHelper.SystemPathToUnityPath(file));
						if (!exportGuidList.Contains(guid))
							exportGuidList.Add(guid);
					}
				}
				else
				{
					cPackageHelper.LogWarning(string.Format("file: {0} is not exist, can not export", assetPath));
				}
			}

            string exportPackagePath = Path.IsPathRooted(exportPackageName) ? exportPackageName : string.Format("{0}{1}", Application.dataPath.Replace("Assets", ""), exportPackageName);
            ExportCPackage(exportGuidList, exportPackagePath);
		}

		public static void ExportCPackage(List<string> guidList, string exportPackagePath)
		{
			InitExportPipeline();

			if (guidList.Count == 0)
				return;

			EditorUtility.DisplayProgressBar("Exporting cPackage", "Gathering files...", 0.1f);

			string exportPackageFolder = cPackageHelper.TempCPackageFolder;
			if (Directory.Exists(exportPackageFolder))
				Directory.Delete(exportPackageFolder, true);
			Directory.CreateDirectory(exportPackageFolder);

			foreach (var guid in guidList)
			{
				string filePath = AssetDatabase.GUIDToAssetPath(guid);
				string metaFilePath = string.Format("{0}.meta", filePath);

				string destFolder = exportPackageFolder + guid;
				Directory.CreateDirectory(destFolder);

				FileInfo fi = new FileInfo(cPackageHelper.UnityPathToSystemPath(filePath));
				if (fi.Exists)
				{
					File.Copy(cPackageHelper.UnityPathToSystemPath(filePath), destFolder + "/asset");
				}

				FileInfo metaFi = new FileInfo(cPackageHelper.UnityPathToSystemPath(metaFilePath));
				if (metaFi.Exists)
				{
					File.Copy(cPackageHelper.UnityPathToSystemPath(metaFilePath), destFolder + "/asset.meta");
				}

				string pathnameFilePath = destFolder + "/pathname";
				cPackageHelper.WriteStingToFile(pathnameFilePath, filePath);

				ProcessExportPipeline(
					fi.Exists ? filePath : "",
					metaFi.Exists ? metaFilePath : "",
					destFolder);
			}

			EditorUtility.DisplayProgressBar("Exporting cPackage", "Compressing package...", 0.5f);
			CompressPackage(exportPackageFolder, exportPackagePath);

			EditorUtility.ClearProgressBar();
		}

		static void ProcessExportPipeline(string filePath, string metaFilePath, string destFolder)
		{
			if (!string.IsNullOrEmpty(filePath) && filePath.LastIndexOf(".") != -1)
			{
				_exportPrePipeline.Process(destFolder + "/asset", filePath);
			}
			if (!string.IsNullOrEmpty(metaFilePath) && metaFilePath.LastIndexOf(".") != -1)
			{
				_exportPrePipeline.Process(destFolder + "/asset.meta", metaFilePath);
			}
		}

		static void CompressPackage(string exportPackageFolder, string exportPackagePath)
		{
			// tar
			string tempTarPath = Application.temporaryCachePath + "/temp.tar";
			cPackageHelper.Tar(exportPackageFolder, tempTarPath);

			// compress
			cPackageHelper.Compress(tempTarPath, exportPackagePath, true);
		}

		private static void InitExportPipeline()
		{
			if (_exportPrePipeline == null)
				_exportPrePipeline = new ProcessPipeline<ExportPrePipelineFeatureAttribute>();
		}

		private static ProcessPipeline<ExportPrePipelineFeatureAttribute> _exportPrePipeline;

		#endregion

		#region Import Interface

		public static void ImportCPackage(string importPackagePath, Action<bool> importRetCallback)
		{
			InitImportPipeline();

			string importTempDir = cPackageHelper.TempCPackageFolder;

			bool decompressRet = DecompressPackage(importPackagePath, importTempDir);
			if (!decompressRet)
				return;

			ProcessImportPrePipeline(importTempDir);
			List<ImportItem> importItemList = GetImportItemList(importTempDir);
			cPackageImportWindow.ShowImportTreeView(importItemList, importRetCallback);
		}

		static bool DecompressPackage(string importPackagePath, string importTempDir)
		{
			if (Directory.Exists(importTempDir))
				Directory.Delete(importTempDir, true);
			Directory.CreateDirectory(importTempDir);

			// decompress
			cPackageHelper.Decompress(importPackagePath, importTempDir);

			// untar
			string tempTarPath = importTempDir + "temp.tar";
			return cPackageHelper.Untar(tempTarPath, importTempDir);
		}

		static void ProcessImportPrePipeline(string importTempDir)
		{
			string decompressTempDir = importTempDir + "cPackage/";

			string[] childDirectories = Directory.GetDirectories(decompressTempDir);

			foreach (var childDirectory in childDirectories)
			{
				string importAssetPath = childDirectory + "/asset";
				if (File.Exists(importAssetPath))
				{
					string importPathnamePath = childDirectory + "/pathname";
					string importPath = cPackageHelper.ReadStringFromFile(importPathnamePath);
					if (!string.IsNullOrEmpty(importPath))
					{
						_importPrePipeline.Process(importAssetPath, importPath);
					}
				}

				string importAssetMetaPath = childDirectory + "/asset.meta";
				if (File.Exists(importAssetMetaPath))
				{
					string importPathnamePath = childDirectory + "/pathname";
					string importPath = cPackageHelper.ReadStringFromFile(importPathnamePath);
					if (!string.IsNullOrEmpty(importPath))
					{
						_importPrePipeline.Process(importAssetMetaPath, string.Format("{0}.meta", importPath));
					}
				}
			}
		}

		static List<ImportItem> GetImportItemList(string importTempDir)
		{
			// add
			// 1. guid not exist, no file exist in import path => copy
			// 2. guid not exist, file exist in import path => copy and auto rename (create unique file for imported asset as file exists with different GUID (Assets/Actions/Resources/PandoraEntry/Lua/Activities.lua.bytes => Assets/Actions/Resources/PandoraEntry/Lua/Activities.lua 1.bytes))

			// update
			// 1. guid exist, file content changed => copy and override
			// 2. guid exist, file path changed, file content not change  => do nothing

			string decompressTempDir = importTempDir + "cPackage/";
			List<ImportItem> importItemList = new List<ImportItem>();

			string[] childDirectories = Directory.GetDirectories(decompressTempDir);
			List<string> sortedChildDirectories = new List<string>(childDirectories);

			Func<string, string, int> pathCompare = (s1, s2) =>
			{
				string pathnamePath1 = s1 + "/pathname";
				string pathnamePath2 = s2 + "/pathname";
				int path1Length = cPackageHelper.ReadStringFromFile(pathnamePath1).Length;
				int path2Length = cPackageHelper.ReadStringFromFile(pathnamePath2).Length;
				return path1Length.CompareTo(path2Length);
			};
			sortedChildDirectories.Sort((s1, s2) => pathCompare(s1, s2));

			foreach (var childDirectory in sortedChildDirectories)
			{
				ImportItem item = new ImportItem();
				item.ImportDir = childDirectory;

				DirectoryInfo di = new DirectoryInfo(childDirectory);
				string importAssetPath = childDirectory + "/asset";
				item.IsDir = !File.Exists(importAssetPath);

				string importPathnamePath = childDirectory + "/pathname";
				item.ImportPath = cPackageHelper.ReadStringFromFile(importPathnamePath);
				item.DestPath = item.ImportPath;

				string guidStr = di.Name;
				string assetPath = AssetDatabase.GUIDToAssetPath(guidStr);

				// AssetDatabase.GUIDToAssetPath will still return the old path while the file has been removed.
				bool reallyExist = File.Exists(cPackageHelper.UnityPathToSystemPath(assetPath)) || Directory.Exists(cPackageHelper.UnityPathToSystemPath(assetPath));
				if (!reallyExist || string.IsNullOrEmpty(assetPath) || assetPath.StartsWith("Assets/__DELETED_GUID_Trash/"))
				{
					foreach (var importItem in importItemList)
					{
						if (importItem.IsDir && item.ImportPath.Contains(importItem.ImportPath))
						{
							item.ImportPath = item.ImportPath.Replace(importItem.ImportPath, importItem.DestPath);
						}
					}

					string destPath = cPackageHelper.UnityPathToSystemPath(item.DestPath);
					if (File.Exists(destPath))
					{
						destPath = cPackageHelper.GetUniqueFilePath(destPath);
						item.DestPath = cPackageHelper.SystemPathToUnityPath(destPath);
					}
					if (!item.IsDir)
						item.AssetChange = true;
					item.MetaChange = true;
					item.IsNew = true;
				}
				else
				{
					string assetSystemPath = cPackageHelper.UnityPathToSystemPath(assetPath);
					bool isFileTypeNotChanged = item.IsDir ? Directory.Exists(assetSystemPath) : File.Exists(assetSystemPath);
					if (!isFileTypeNotChanged)
					{
						//item.AssetChange = true;
						//item.MetaChange = true;
						item.FileTypeChange = true;
					}
					else if (assetPath.Equals(item.DestPath))
					{
						string destPath = cPackageHelper.UnityPathToSystemPath(item.DestPath);
						if (!item.IsDir)
							item.AssetChange = cPackageHelper.CalculateMD5(destPath) != cPackageHelper.CalculateMD5(importAssetPath);
						item.MetaChange = cPackageHelper.CalculateMD5(destPath + ".meta") != cPackageHelper.CalculateMD5(importAssetPath + ".meta");
					}
					else
					{
						item.DestPath = assetPath;
						string destPath = cPackageHelper.UnityPathToSystemPath(item.DestPath);
						if (!item.IsDir)
							item.AssetChange = cPackageHelper.CalculateMD5(destPath) != cPackageHelper.CalculateMD5(importAssetPath);
						item.MetaChange = cPackageHelper.CalculateMD5(destPath + ".meta") != cPackageHelper.CalculateMD5(importAssetPath + ".meta");
					}
				}

				importItemList.Add(item);
			}

			return importItemList;
		}

		public static void ImportItemList(List<ImportItem> importItemList)
		{
			EditorUtility.DisplayProgressBar("Importing cPackage", "Gathering files...", 0.1f);

			int totalCount = importItemList.Count;
			for (int i = 0; i < totalCount; i++)
			{
				ImportItem item = importItemList[i];

				// FileTypeChange do nothing

				EditorUtility.DisplayProgressBar("Importing cPackage", "Importing file", 0.1f + (i / totalCount) * 0.8f);

				string destSystemPath = cPackageHelper.UnityPathToSystemPath(item.DestPath);
				string destMetaSystemPath = destSystemPath + ".meta";

				if (item.AssetChange)
				{
					if (File.Exists(destSystemPath))
						File.Delete(destSystemPath);

					cPackageHelper.CreateDirectory(new FileInfo(destSystemPath));
					File.Copy(item.ImportDir + "/asset", destSystemPath);
				}

				if (item.MetaChange)
				{
					if (File.Exists(destMetaSystemPath))
						File.Delete(destMetaSystemPath);
					cPackageHelper.CreateDirectory(new FileInfo(destMetaSystemPath));
					File.Copy(item.ImportDir + "/asset.meta", destMetaSystemPath);
				}
			}

			EditorUtility.ClearProgressBar();

			AssetDatabase.Refresh();

			cPackage.Tools.cPackageHelper.Log("start import post process");

			for (int i = 0; i < totalCount; i++)
			{
				ImportItem item = importItemList[i];
				string destSystemPath = cPackageHelper.UnityPathToSystemPath(item.DestPath);

				if (item.AssetChange && destSystemPath.LastIndexOf(".") != -1)
				{
					_importPostPipeline.Process(destSystemPath, destSystemPath);
				}

				if (item.MetaChange)
				{
					string destMetaSystemPath = destSystemPath + ".meta";
					_importPostPipeline.Process(destMetaSystemPath, destMetaSystemPath);
				}
			}

			cPackage.Tools.cPackageHelper.Log("complete import post process");

			AssetDatabase.Refresh();
		}

		public static void DestroyTempCPackageFolder()
		{
			Directory.Delete(cPackageHelper.TempCPackageFolder, true);
		}

		private static void InitImportPipeline()
		{
			if (_importPrePipeline == null)
				_importPrePipeline = new ProcessPipeline<ImportPrePipelineFeatureAttribute>();

			if (_importPostPipeline == null)
				_importPostPipeline = new ProcessPipeline<ImportPostPipelineFeatureAttribute>();
		}

		private static ProcessPipeline<ImportPrePipelineFeatureAttribute> _importPrePipeline;
		private static ProcessPipeline<ImportPostPipelineFeatureAttribute> _importPostPipeline;

		#endregion
	}
}
