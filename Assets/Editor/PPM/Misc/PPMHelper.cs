using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using cPackage.Tools;
using Semver;

namespace PPM
{
	public static class PPMHelper
	{
		#region Style

		public static GUIStyle GenerateStyle(float backgroudWidth, float backgroundHeight, Color backgroundColor)
		{
			GUIStyle style = new GUIStyle();
			style.border.left = style.border.right = 2;
			style.normal.background = PPMHelper.MakeTex(backgroudWidth, backgroundHeight, backgroundColor);
			return style;
		}

		public static Rect ComputeControlMiddleDrawRect(Rect controlRect, GUIStyle controlStyle)
		{
			float controlHeight = controlStyle.CalcHeight(GUIContent.none, controlRect.width);
			float middleYPos = controlRect.y + (controlRect.height - controlHeight) / 2;
			return new Rect(controlRect.x, middleYPos, controlRect.width, controlRect.height);
		}

		public static GUIStyle CloneStyle(GUIStyle cloneStyle)
		{
			return new GUIStyle(cloneStyle);
		}

		#endregion

		#region Logger

		public enum DebugLevel
		{
			Error,
			Warning,
			Log,
		}

		public static DebugLevel DLevel = DebugLevel.Log;

		public static void Log(object message, UnityEngine.Object context = null)
		{
			if (DLevel < DebugLevel.Log)
				return;

			System.Action action = () =>
			{
				Debug.Log(string.Format("<color={0}>PPM Log: {1}</color>", IsProSkin() ? "cyan" : "black", message), context);
			};
			ThreadUtils.ExecuteOnMainThread(action);
		}

		public static void LogError(object message, UnityEngine.Object context = null)
		{
			if (DLevel < DebugLevel.Error)
				return;

			System.Action action = () =>
			{
				Debug.LogError(string.Format("<color={0}>PPM Error: {1}</color>", IsProSkin() ? "red" : "red", message), context);
			};
			ThreadUtils.ExecuteOnMainThread(action);
		}

		public static void LogWarning(object message, UnityEngine.Object context = null)
		{
			if (DLevel < DebugLevel.Warning)
				return;

			System.Action action = () =>
			{
				Debug.LogWarning(string.Format("<color={0}>PPM Warning: {1}</color>", IsProSkin() ? "yellow" : "black", message), context);
			};
			ThreadUtils.ExecuteOnMainThread(action);
		}

		#endregion

		#region Texture

		public static Texture2D MakeTex(int width, int height, Color color)
		{
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix[i] = color;

			Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
			result.SetPixels(pix);
			result.Apply(false);

			return result;
		}

		public static Texture2D MakeTex(float width, float height, Color color)
		{
			return MakeTex((int)width, (int)height, color);
		}

		#endregion

		#region File & Direcotry

		private static string _cacheDirectory;

		public static string GetCacheDirectory()
		{
			if (string.IsNullOrEmpty(_cacheDirectory))
			{
#if UNITY_EDITOR_WIN
				string systemDirectory = System.Environment.GetEnvironmentVariable("APPDATA").Replace("\\", "/");
#elif UNITY_EDITOR_OSX
				string systemDirectory = System.Environment.GetEnvironmentVariable("HOME");
#endif
				_cacheDirectory = string.Format("{0}/PPM/", systemDirectory);
			}
			return _cacheDirectory;
		}

		public static void CreateDirectory(FileInfo fi, bool recreate = false)
		{
			if (fi.Directory.Exists)
			{
				if (recreate)
					Directory.Delete(fi.Directory.FullName, true);
				else
					return;
			}
			Directory.CreateDirectory(fi.Directory.FullName);
		}

		public static void CreateDirectory(string path, bool recreate = false)
		{
			FileInfo fi = new FileInfo(path);
			CreateDirectory(fi, recreate);
		}

		public static string UnityPathToSystemPath(this string unityPath)
		{
			return unityPath.Replace("Assets", Application.dataPath);
		}

		public static string SystemPathToUnityPath(this string systemPath)
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

		public static string[] SearchFiles(string directory, string searchPattern, SearchOption searchOption)
		{
			return Directory.GetFiles(directory, searchPattern, searchOption);
		}

		#endregion

		#region Others

		public static bool IsProSkin()
		{
			return EditorGUIUtility.isProSkin;
		}

		public static void RefreshWindow<T>()
		{
			EditorWindow.GetWindow(typeof(T), true).Repaint();
		}

		public static void CloseWindow<T>()
		{
			EditorWindow.GetWindow(typeof(T), true).Close();
		}

		public static bool IsUnityPackage(string filePath)
		{
			return filePath.EndsWith(".unitypackage");
		}

		public static bool IscPackage(string filePath)
		{
			return filePath.EndsWith(".cpackage");
		}

		static readonly string _tempPPMFolder = "ppm/";

		public static string TempPPMFolder
		{
			get
			{
				return Application.dataPath.Replace("Assets", _tempPPMFolder);
			}
		}

		public static string GetServerAddress()
		{
			return "http://9.134.52.147:8000/";
		}

        private static bool ParseCommandLineArgs(PPMConfiguration conf)
        {
            string[] argList = System.Environment.GetCommandLineArgs();

            // -argName=argValue 形式解析为 argName=argValue字典格式
            var argsDict = argList.ToDictionary(
                v => v.Substring(1,
                    v.Contains("=") ? v.IndexOf("=", StringComparison.CurrentCulture) - 1 : v.Length - 1),
                v => v.Substring(v.Contains("=") ? v.IndexOf("=", StringComparison.CurrentCulture) + 1 : 0));

            Type type = conf.GetType();
            FieldInfo[] fieldInfos = type.GetFields();

            foreach (FieldInfo item in fieldInfos)
            {
                if (argsDict.ContainsKey(item.Name))
                {
                    item.SetValue(conf, Convert.ChangeType(argsDict[item.Name], item.FieldType));
                }
            }

            if (!conf.IsDataReady())
            {
                PPMHelper.LogError("打包信息填写不完整，检查重填");
                return false;
            }

            if (!IsVersionFormatValid(conf.PackageVersion))
            {
                PPMHelper.LogError("版本号命名需要提供三位数，如 x.y.z");
                return false;
            }

            return true;
        }

        private static bool IsVersionFormatValid(string packageVersion)
        {
            SemVersion parsedVersion;
            SemVersion.TryParse(packageVersion, out parsedVersion, true);
            return parsedVersion != null;
        }

        private static void CollectPackageFileList(PPMConfiguration conf)
        {
            string toPackPackagePath = conf.ToPackPackagePath;
            string tmpDir = string.Format("{0}/{1}/", Path.GetDirectoryName(toPackPackagePath), Path.GetFileNameWithoutExtension(toPackPackagePath));
            cPackageHelper.Decompress(toPackPackagePath, tmpDir);
            cPackageHelper.Untar(tmpDir + "temp.tar", tmpDir);
            string contentDir = tmpDir + "cPackage";
            foreach (var d in Directory.GetDirectories(contentDir))
            {
                PPMConfiguration.ResourceItem item = new PPMConfiguration.ResourceItem();
                item.GUID = Path.GetFileName(d);
                item.Path = File.ReadAllText(d + "/pathname");
                item.ContentMd5 = File.Exists(d + "/asset") ? cPackageHelper.CalculateMD5(d + "/asset") : "";
                item.MetaMd5 = cPackageHelper.CalculateMD5(d + "/asset.meta");
                conf.ResourceItemList.Add(item);
            }

            Directory.Delete(tmpDir, true);
        }

        private static void CreateConfigurationFile(PPMConfiguration conf, string outputFile)
        {
            string content = JsonConvert.SerializeObject(conf, Formatting.Indented);
            File.WriteAllText(outputFile, content, Encoding.UTF8);
        }

        public static void PackPandoraPackage()
        {
            PPMConfiguration conf = new PPMConfiguration();
            if (!ParseCommandLineArgs(conf))
                return;

            string tmpDir = Path.GetDirectoryName(conf.ToPackPackagePath) + "/ppm_package/";
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
            Directory.CreateDirectory(tmpDir);

            if (PPMHelper.IscPackage(conf.ToPackPackagePath))
                CollectPackageFileList(conf);
            CreateConfigurationFile(conf, tmpDir + "ppm_package.json");
            File.Copy(conf.ToPackPackagePath, tmpDir + Path.GetFileName(conf.ToPackPackagePath));

            string outputDir = Path.GetDirectoryName(conf.ToPackPackagePath);
            string outputPath = string.Format("{0}/{1}_v{2}.ppm", outputDir, conf.PackageName, conf.PackageVersion);
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            cPackageHelper.Tar(tmpDir, outputPath);
            Directory.Delete(tmpDir, true);
            PPMHelper.Log(string.Format("插件制作成功: {0}", outputPath));
        }

		#endregion

		#region Version

		private static string _currentVersion = "";

		public static string GetCurrentPPMVersion()
		{
			if (string.IsNullOrEmpty(_currentVersion))
			{
				string changelogPath = string.Format("{0}/Editor/PPM/changelog.txt", Application.dataPath);
				string content = ReadStringFromFile(changelogPath);
				Match match = Regex.Match(content, "v(\\d*\\.\\d*\\.\\d*)");
				if (match.Success)
				{
					_currentVersion = match.Groups[1].Value;
				}
				else
				{
					PPMHelper.LogError("当前PPM版本未知，PPM/changelog.txt文件缺失");
				}
			}

			return _currentVersion;
		}

		public static PPMConfiguration GetPackageConfiguration(string packageName)
		{
			string config = ReadStringFromFile(GetInstalledConfigurationPath());
			List<PPMConfiguration> configurationList = JsonConvert.DeserializeObject<List<PPMConfiguration>>(config);
			foreach (var conf in configurationList)
			{
				if (conf.PackageName.Equals(packageName))
					return conf;
			}
			return null;
		}

		public static bool IsPackageAlreadyInstalled(string packageName)
		{
			string config = ReadStringFromFile(GetInstalledConfigurationPath());
			List<PPMConfiguration> configurationList = JsonConvert.DeserializeObject<List<PPMConfiguration>>(config);
			foreach (var conf in configurationList)
			{
				if (conf.PackageName.Equals(packageName))
					return true;
			}
			return false;
		}

		private static string _installedConfigurationPath;

		public static string GetInstalledConfigurationPath()
		{
			if (string.IsNullOrEmpty(_installedConfigurationPath))
			{
				_installedConfigurationPath = string.Format("{0}/../ppm_installed/config.json", Application.dataPath);
				CreateDirectory(new FileInfo(_installedConfigurationPath));
			}

			return _installedConfigurationPath;
		}

		public static void InstallSelf()
		{
			PPMConfiguration config = new PPMConfiguration();
			config.PackageName = "Pandora Package Manager";
			config.PackageType = "Unity";
			config.PackageVersion = GetCurrentPPMVersion();
			config.PackageAuthor = "潘多拉SDK团队";
			config.DisableAutoUpdate = true;
			config.PackageDescription = "潘多拉Unity插件管理器";
			config.PackageTags = "工具";
			InstallPackage(config, false);
		}

		public static bool UninstallPackage(PPMPackageInfo packageInfo, bool showTips = true)
		{
			string uninstallPackageName = packageInfo.name;
			string uninstallPackageVersion = packageInfo.releases[0].version;

			string config = ReadStringFromFile(GetInstalledConfigurationPath());
			List<PPMConfiguration> configurationList = JsonConvert.DeserializeObject<List<PPMConfiguration>>(config);
			PPMConfiguration toUninstallItem = null;
			foreach (var conf in configurationList)
			{
				if (conf.PackageName.Equals(uninstallPackageName)
					&& conf.PackageVersion.Equals(uninstallPackageVersion)
					&& conf.DisableAutoUpdate == false)
				{
					toUninstallItem = conf;
					break;
				}
			}

			if (toUninstallItem != null)
			{
				try
				{
					toUninstallItem.ResourceItemList.Sort((i, j) => j.Path.Length.CompareTo(i.Path.Length));
					foreach (var resourceItem in toUninstallItem.ResourceItemList)
					{
						string sysPath = UnityPathToSystemPath(resourceItem.Path);
						if (File.Exists(sysPath))
						{
							string md5 = cPackageHelper.CalculateMD5(sysPath);
							if (md5.Equals(resourceItem.ContentMd5)
								|| EditorUtility.DisplayDialog("插件卸载", string.Format("文件:{0} 内容已经被更改，确认卸载?", resourceItem.Path), "确认"))
							{
								File.Delete(sysPath);
								if (File.Exists(sysPath + ".meta"))
									File.Delete(sysPath + ".meta");
							}
							else
							{
								Log(string.Format("文件:{0} 未卸载", resourceItem.Path));
							}
						}
						else if (Directory.Exists(sysPath))
						{
							string[] files = Directory.GetFiles(sysPath, "*", SearchOption.AllDirectories);
							if (files.Length == 0)
							{
								Directory.Delete(sysPath);
								if (File.Exists(sysPath + ".meta"))
									File.Delete(sysPath + ".meta");
							}
						}					
					}

					configurationList.Remove(toUninstallItem);
					WriteStingToFile(GetInstalledConfigurationPath(), JsonConvert.SerializeObject(configurationList, Formatting.Indented));
					if (showTips)
						Log(string.Format("卸载插件: {0} 成功", toUninstallItem.PackageName));
					AssetDatabase.Refresh();
				}
				catch (Exception e)
				{
					LogError(string.Format("插件:{0} 卸载失败. Err:{1} Stack:{2}", toUninstallItem.PackageName, e.Message, e.StackTrace));
					return false;
				}
				return true;
			}
			else
			{
				PPMHelper.Log(string.Format("卸载插件失败，未安装或需要手动删除: {0}-v{1}", uninstallPackageName, uninstallPackageVersion));
				return false;
			}
		}

		public static void InstallPackage(PPMConfiguration configuration, bool showTips = true)
		{
			string config = ReadStringFromFile(GetInstalledConfigurationPath());
			List<PPMConfiguration> configurationList;
			if (!string.IsNullOrEmpty(config))
			{
				configurationList = JsonConvert.DeserializeObject<List<PPMConfiguration>>(config);
				bool needAdd = true;
				foreach(var conf in configurationList)
				{
					if (conf.PackageName.Equals(configuration.PackageName))
					{
						conf.CopyFrom(configuration);
						needAdd = false;
						break;
					}
				}

				if (needAdd)
					configurationList.Add(configuration);
			}
			else
			{
				configurationList = new List<PPMConfiguration>() { configuration };
			}

			WriteStingToFile(GetInstalledConfigurationPath(), JsonConvert.SerializeObject(configurationList, Formatting.Indented));
			if (showTips)
				Log(string.Format("安装插件: {0} 成功", configuration.PackageName));
		}

		public static PPMConfiguration DecompressPPM(string filePath, ref string packagePath)
		{
			DeletePPMDecompressDir();
			cPackageHelper.Untar(filePath, TempPPMFolder);

			string[] files = Directory.GetFiles(TempPPMFolder + "ppm_package/", "*.*", SearchOption.AllDirectories);
			foreach(var file in files)
			{
				if (IscPackage(file) || IsUnityPackage(file))
				{
					packagePath = file;
					break;
				}
			}

			string configFile = TempPPMFolder + "ppm_package/ppm_package.json";
			return JsonConvert.DeserializeObject<PPMConfiguration>(File.ReadAllText(configFile));
		}

		public static void DeletePPMDecompressDir()
		{
			if (Directory.Exists(TempPPMFolder))
				Directory.Delete(TempPPMFolder, true);
		}

		#endregion
	}
}
