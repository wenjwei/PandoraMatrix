using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;

namespace com.tencent.pandora.tools
{
    public class PackageExporter
    {
        [MenuItem("PandoraTools/ExportSDKPackage")]
        public static void Export()
        {
			BeforeExport();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            string[] paths = new string[] {
                                            "Assets/Pandora/Scripts",
                                            "Assets/Pandora/Resources",
                                            "Assets/Pandora/Slua/LuaObject",
                                            "Assets/Plugins",
                                           };

            //string name = string.Format("PandoraSDK_{0}_{1}.{2}", Pandora.Instance.GameCode, DateTime.Now.ToString("yyyy_MM_dd_HH"), "unitypackage");
            string name = string.Format("PandoraSDK_{0}_{1}_V{2}.{3}", Pandora.Instance.GameCode, DateTime.Now.ToString("yyyy_MM_dd_HH"), Pandora.Instance.CombinedSDKVersion(), "unitypackage");
            AssetDatabase.ExportPackage(paths, name, ExportPackageOptions.Recurse);
            AfterExport();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void BeforeExport()
		{
			string path = Application.dataPath + "/Plugins/Pandora_Managed/PandoraSettings.cs";
			string code = File.ReadAllText(path);
			code = code.Replace("public static int DEFAULT_LOG_LEVEL = Logger.DEBUG; //给游戏输出SDK包时替换为Logger.INFO，减少Log对项目组的干扰", "public static int DEFAULT_LOG_LEVEL = Logger.INFO; //给游戏输出SDK包时替换为Logger.INFO，减少Log对项目组的干扰");
			code = code.Replace("return Application.dataPath + \"/CACHE\";", "return Application.dataPath + \"\\\\..\\\\CACHE\";");
			Debug.Log(code);
			File.WriteAllText(path, code);

			if (Directory.Exists(Application.dataPath + "/Plugins/ppmutils.bundle"))
			{
				FileUtil.MoveFileOrDirectory("Assets/Plugins/ppmutils.bundle", "Assets/ppmutils.bundle");
				FileUtil.MoveFileOrDirectory("Assets/Plugins/ppmutils.bundle.meta", "Assets/ppmutils.bundle.meta");
			}
		}

		private static void AfterExport()
		{
			string path = Application.dataPath + "/Plugins/Pandora_Managed/PandoraSettings.cs";
			string code = File.ReadAllText(path);
			code = code.Replace("public static int DEFAULT_LOG_LEVEL = Logger.INFO; //给游戏输出SDK包时替换为Logger.INFO，减少Log对项目组的干扰", "public static int DEFAULT_LOG_LEVEL = Logger.DEBUG; //给游戏输出SDK包时替换为Logger.INFO，减少Log对项目组的干扰");
			code = code.Replace("return Application.dataPath + \"\\\\..\\\\CACHE\";", "return Application.dataPath + \"/CACHE\";");
			File.WriteAllText(path, code);

			if (Directory.Exists(Application.dataPath + "/ppmutils.bundle"))
			{
				FileUtil.MoveFileOrDirectory("Assets/ppmutils.bundle", "Assets/Plugins/ppmutils.bundle");
				FileUtil.MoveFileOrDirectory("Assets/ppmutils.bundle.meta", "Assets/Plugins/ppmutils.bundle.meta");
			}

			Debug.Log("打包成功！");
		}
	}

}
