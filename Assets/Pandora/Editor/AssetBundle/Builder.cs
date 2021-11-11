//#define USING_NGUI
#define USING_UGUI
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;
using ICSharpCode.SharpZipLib.Zip;

namespace com.tencent.pandora.tools
{
    /// <summary>
    /// 资源打包主要策略：
    /// 1.Prefab文件和其依赖的资源打成一个包，若同一个项目中Pandora面板数量较多且存在共享资源时，则可以将图集资源提取出来，做成依赖打包
    /// 2.Actions/Resources目录下一个目录的Lua文件打成一个包
    /// </summary>
    internal class Builder
    {
        private const string SPLIT_SHADER_NAME = "Pandora/Transparent Masked";

        private const string COPY_TOKEN = "_copy";
        /// <summary>
        /// 一个PathList中的资源打成一个Bundle
        /// </summary>
        private static List<List<string>> _assetPathListList;
        /// <summary>
        /// 生成的Bundle路径列表
        /// </summary>
        private static List<string> _bundlePathList;
        private static List<string> _copyAssetList;
        private static List<string> _versionAssetList;
        private static BuildTarget _buildTarget;
        //为防止打包的时候对Texture进行分离操作，Build资源的时候，不PostProcess Texture
        public static bool IS_BUILDING = false;

        //是否支持打包PC平台资源的开关
        public static bool IS_SUPPORT_PC = false;


        //打包配置文件，用来配置lua文件路径以及是否启用沙盒。
        public static string BUILD_CONFIG_FILENAME = "buildConfig.bytes";

        private static Dictionary<BuildTarget, string> BUILD_TARGET_NAME_DICT = new Dictionary<BuildTarget, string>()
        {
            { BuildTarget.StandaloneWindows, GetWindowsBuildTargetName() },
            { BuildTarget.StandaloneWindows64, GetWindowsBuildTargetName() },
            { BuildTarget.Android, "android" },
#if UNITY_4_6 || UNITY_4_7
            { BuildTarget.StandaloneOSXUniversal, "ios" },
            { BuildTarget.iPhone, "ios" },
#elif UNITY_5 || UNITY_2017_1 || UNITY_2017_2
            { BuildTarget.StandaloneOSXUniversal, "ios" },
            { BuildTarget.iOS, "ios" }
#elif UNITY_2017_3_OR_NEWER
            { BuildTarget.StandaloneOSX, "ios" },
            { BuildTarget.iOS, "ios" }
#endif
        };

        private static string GetWindowsBuildTargetName()
        {
            if(IS_SUPPORT_PC == true)
            {
                return "pc";
            }
            return "android";
        }

        private static void Initialize()
        {
            _assetPathListList = new List<List<string>>();
            _bundlePathList = new List<string>();
            _copyAssetList = new List<string>();
            _versionAssetList = new List<string>();
        }

        public static void Build(List<string> activityList, BuildTarget target, bool showReport = true)
        {
            IS_BUILDING = true;
            _buildTarget = target;
            LuaProcessor.PreProcessLuaFile();
			Initialize();
            CreateStreamingAssetFolder();
            CommitActivityPrefabList(activityList);
            BuildActivityPrefabList(activityList, target);
            ComplieActivityLuaList(activityList);
            CommitActivityLuaList(activityList);
            BuildActivityLuaList(activityList, target, BuilderSetting.LUA_32_PATH_TEMPLATE);
            BuildActivityLuaList(activityList, target, BuilderSetting.LUA_64_PATH_TEMPLATE);
            BuildResForPixUI(activityList);
            ExecuteBuild(target);
            DeleteTempBinList(activityList);
            DeleteCompiledLuaList(activityList);
            DeleteCopyPrefabList();
			LuaProcessor.PostProcessLuaFile();
            DeleteVersionAssetList();
            AssetDatabase.Refresh();
            //CopyBundleToGame("E:/Speedm/NssUnityProj/Assets/StreamingAssets/Pandora");
            if (showReport == true)
            {
                ShowReport();
            }
            IS_BUILDING = false;
        }

        private static void CreateStreamingAssetFolder()
        {
            if (!Directory.Exists(BuilderSetting.STREAM_ASSET_PATH))
            {
                Directory.CreateDirectory(BuilderSetting.STREAM_ASSET_PATH);
            }
        }

        private static void CommitActivityPrefabList(List<string> activityList)
        {
            foreach(string s in activityList)
            {
                if(ActivityManager.IsActivityBuildPrefab(s) == true && ActivityManager.IsActivityCommitSvn(s) == true)
                {
                    string prefabFolder = string.Format(BuilderSetting.PREFAB_PATH_TEMPLATE, s);
                    List<string> prefabPathList = GetPrefabPathList(prefabFolder);
                    foreach (string path in prefabPathList)
                    {
                        CreateVersionFile(path, path.Replace(".prefab", "_version.lua.bytes"));
                    }
                }
            }
        }

        private static void CreateVersionFile(string assetPath, string versionAssetPath)
        {
            string localPath = ConvertToFilePath(assetPath);
            string revision = SvnHelper.Commit(localPath);
            if(revision == SvnHelper.COMMIT_ERROR)
            {
                EditorUtility.DisplayDialog("提交错误", assetPath + " 自动提交错误，请手动解决错误~", "马上就去");
            }
            else if(string.IsNullOrEmpty(revision) == true)
            {
                revision = SvnHelper.GetLocalFileRevision(localPath);
            }
            if(string.IsNullOrEmpty(revision) == true)
            {
                revision = SvnHelper.COMMIT_ERROR;
            }
            File.WriteAllText(ConvertToFilePath(versionAssetPath), revision);
            _versionAssetList.Add(versionAssetPath);
            AssetDatabase.ImportAsset(versionAssetPath, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// 打包某个Activity的Prefab文件列表
        /// 其中Prefab文件和图集资源根据依赖关系打在一起，若将来存在公共资源的情况时需要优化
        /// 文本的字体资源会剥离出来，不打包
        /// </summary>
        /// <param name="activityList"></param>
        /// <param name="target"></param>
        private static void BuildActivityPrefabList(List<string> activityList, BuildTarget target)
        {
            foreach (string s in activityList)
            {
                if (ActivityManager.IsActivityBuildPrefab(s) == true)
                {
                    string prefabFolder = string.Format(BuilderSetting.PREFAB_PATH_TEMPLATE, s);
                    List<string> prefabPathList = GetPrefabPathList(prefabFolder);
                    foreach (string path in prefabPathList)
                    {
                        BuildPrefab(path, target);
                    }
                }
            }
        }

        private static List<string> GetPrefabPathList(string prefabFolder)
        {
            List<string> result = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { prefabFolder });
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.Contains("_copy.prefab") == false)
                {
                    result.Add(path);
                }
            }
            return result;
        }

        private static void BuildPrefab(string path, BuildTarget target)
        {
            string copyPath = CreatePolishedPrefab(path, target);
            _copyAssetList.Add(copyPath);
            string prefabVersionPath = path.Replace(".prefab", "_version.lua.bytes");
            _assetPathListList.Add(new List<string>() { copyPath, prefabVersionPath });
        }

        /// <summary>
        /// 1.剥离font资源
        /// 2.替换目标平台资源
        /// </summary>
        /// <param name="path"></param>
        private static string CreatePolishedPrefab(string path, BuildTarget target)
        {
            string copyPath = path.Replace(".prefab", COPY_TOKEN + ".prefab");
            AssetDatabase.DeleteAsset(copyPath);
            AssetDatabase.CopyAsset(path, copyPath);
            AssetDatabase.ImportAsset(copyPath, ImportAssetOptions.ForceUpdate);
            GameObject copyPrefab = AssetDatabase.LoadAssetAtPath(copyPath, typeof(GameObject)) as GameObject;
            GameObject go = PrefabUtility.InstantiatePrefab(copyPrefab) as GameObject;
#if USING_UGUI
            StripUGUIFont(go);
            AddImagePartner(go);
#endif
#if USING_NGUI
            StripNGUIFont(go);
            AddUISpritePartner(go);
#endif
            AddPanelOnDestroyHook(go);
            PrefabUtility.CreatePrefab(copyPath, go, ReplacePrefabOptions.ConnectToPrefab);
            Object.DestroyImmediate(go, true);
            return copyPath;
        }

#if USING_UGUI
        private static void StripUGUIFont(GameObject go)
        {
            UnityEngine.UI.Text[] texts = go.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in texts)
            {
                if (t.font != null)
                {
                    TextPartner partner = t.gameObject.AddComponent<TextPartner>();
                    partner.fontName = t.font.name;
                    t.font = null;
                }
            }
        }

        private static void AddImagePartner(GameObject go)
        {
            UnityEngine.UI.Image[] images = go.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (UnityEngine.UI.Image image in images)
            {
                image.gameObject.AddComponent<ImagePartner>();
            }
        }
#endif

#if USING_NGUI
        private static void StripNGUIFont(GameObject go)
        {
            UILabel[] labels = go.GetComponentsInChildren<UILabel>(true);
            foreach (UILabel l in labels)
            {
                if (l.trueTypeFont != null)
                {
                    TextPartner partner = l.gameObject.AddComponent<TextPartner>();
                    partner.fontName = l.ambigiousFont.name;
                    l.trueTypeFont = null;
                    l.bitmapFont = null;
                    l.ambigiousFont = null;
                }
            }
            UIPopupList[] popups = go.GetComponentsInChildren<UIPopupList>(true);
            foreach (UIPopupList l in popups)
            {
                if (l.trueTypeFont != null)
                {
                    TextPartner partner = l.gameObject.AddComponent<TextPartner>();
                    partner.fontName = l.ambigiousFont.name;
                    l.trueTypeFont = null;
                    l.bitmapFont = null;
                    l.ambigiousFont = null;
                }
            }
        }

        private static void AddUISpritePartner(GameObject go)
        {
            UISprite[] sprites = go.GetComponentsInChildren<UISprite>(true);
            foreach (UISprite sprite in sprites)
            {
                sprite.gameObject.AddComponent<UISpritePartner>();
            }
        }
#endif

        private static void AddPanelOnDestroyHook(GameObject go)
        {
            go.AddComponent<com.tencent.pandora.PanelOnDestroyHook>();
        }

        private static string GetPrefabBundleName(string path, BuildTarget target)
        {
            string dicPath = Path.GetDirectoryName(path);
            string architectureName = Path.GetFileName(dicPath);
            string dicPath2 = Path.GetDirectoryName(dicPath);
            string activityName = Path.GetFileName(dicPath2);


            string result = string.Empty;
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            result = GetPlatformName(target)+ "_" + activityName + "_" + obj.name.Replace(COPY_TOKEN, "") + ".assetbundle";
            return result;
        }

        private static void DeleteCompiledLuaList(List<string> activityList)
        {
            foreach (string s in activityList)
            {
                string lua32Folder = string.Format(BuilderSetting.LUA_32_PATH_TEMPLATE, s);
                string lua64Folder = string.Format(BuilderSetting.LUA_64_PATH_TEMPLATE, s);
                AssetDatabase.DeleteAsset(lua32Folder);
                AssetDatabase.DeleteAsset(lua64Folder);
            }
        }

        private static void CommitActivityLuaList(List<string> activityList)
        {
            foreach(string s in activityList)
            {
                if(ActivityManager.IsActivityCommitSvn(s) == true)
                {
                    string luaFolder = string.Format(BuilderSetting.LUA_PATH_TEMPLATE, s);
                    string versionFileName = s + "_version.lua.bytes";
                    string luaVersionPath = luaFolder + "/" + versionFileName;
                    string lua32VersionPath = string.Format(BuilderSetting.LUA_32_PATH_TEMPLATE, s) + "/" + versionFileName;
                    string lua64VersionPath = string.Format(BuilderSetting.LUA_64_PATH_TEMPLATE, s) + "/" + versionFileName;
                    CreateVersionFile(luaFolder, luaVersionPath);
                    AssetDatabase.CopyAsset(luaVersionPath, lua32VersionPath);
                    AssetDatabase.CopyAsset(luaVersionPath, lua64VersionPath);
                    AssetDatabase.ImportAsset(lua32VersionPath, ImportAssetOptions.ForceUpdate);
                    AssetDatabase.ImportAsset(lua64VersionPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private static void ComplieActivityLuaList(List<string> activityList)
        {
            LuaCompilerWrapper.Error = string.Empty;
            foreach (string s in activityList)
            {
                LuaMonitor.Build(s);
                string luaFolder = string.Format(BuilderSetting.LUA_PATH_TEMPLATE, s);
                List<string> luaPathList = GetLuaPathList(luaFolder);
                foreach (string p in luaPathList)
                {
                    string path = string.Concat(Application.dataPath.Replace("/Assets", "/"), p);
                    LuaCompilerWrapper.Compile(path);
                }
                AssetDatabase.ImportAsset(string.Format(BuilderSetting.LUA_32_PATH_TEMPLATE, s), ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(string.Format(BuilderSetting.LUA_64_PATH_TEMPLATE, s), ImportAssetOptions.ForceUpdate);
            }
            if (string.IsNullOrEmpty(LuaCompilerWrapper.Error) == false)
            {
                EditorUtility.DisplayDialog("Lua编译发生错误", LuaCompilerWrapper.Error, "改改改~");
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 每一个Activity的所有Lua文件合并打成一个包
        /// </summary>
        /// <param name="activityList"></param>
        /// <param name="target"></param>
        private static void BuildActivityLuaList(List<string> activityList, BuildTarget target, string luaPathTemplate)
        {
            foreach (string s in activityList)
            {
                string luaFolder = string.Format(luaPathTemplate, s);
                List<string> luaPathList = GetLuaPathList(luaFolder);
                if (luaPathList.Count > 0)
                {
                    if (s != "Frame" && s != "DJC")
                    {
                        Dictionary<string, string> luaFilePathDict = new Dictionary<string, string>();
                        foreach (var item in luaPathList)
                        {
                            string file = item.Replace("\\", "/").Replace(luaFolder + "/", "");
                            //string fileName
                            if (Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)) == s)
                            {
                                luaFilePathDict.Add(Path.GetFileNameWithoutExtension(file), Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)).Replace("\\", "/"));
                            }
                            else
                            {
                                string path = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)).Replace("\\", "/");
                                luaFilePathDict.Add(Path.GetFileNameWithoutExtension(file), Path.Combine(s, path).Replace("\\", "/"));
                            }
                        }
                        Dictionary<string, string> buildConfigDict = new Dictionary<string, string>();
                        string content = MiniJSON.Json.Serialize(luaFilePathDict);
                        buildConfigDict["sandbox"] = "1";
                        buildConfigDict["luaFilePath"] = content;
                        UTF8Encoding encoding = new UTF8Encoding(false);
                        System.IO.File.WriteAllText(Path.Combine(luaFolder, BUILD_CONFIG_FILENAME), MiniJSON.Json.Serialize(buildConfigDict), encoding);
                        Debug.Log("BuildActivityLuaList  " + MiniJSON.Json.Serialize(luaFilePathDict));
                        luaPathList.Add(Path.Combine(luaFolder, BUILD_CONFIG_FILENAME));
                    }
                    _assetPathListList.Add(luaPathList);
                }
            }
        }
        
        private static void BuildResForPixUI(List<string> activityList)
        {
            CommitActivityBinList(activityList);
            if (BuilderSetting.UseZipResForPixUI)
            {
                BuildActivityBinIntoZip(activityList);
            }
            else
            {
                BuildActivityBinIntoAssetbundle(activityList);
            }
        }
        // 拷贝二进制文件
        private static void CommitActivityBinList(List<string> activityList)
        {
            foreach(string s in activityList)
            {
                var binFolder = string.Format(BuilderSetting.BIN_PATH_TEMPLATE, s);
                var tempBinFolder = string.Format(BuilderSetting.TEMP_BIN_PATH_TEMPLATE, s);
                CopyDir(binFolder, tempBinFolder, false);		// PixUI Bin文件夹暂时非强制需要
                if (!Directory.Exists(tempBinFolder)) {
                    continue;
                }
                var allFiles = Directory.GetFiles(tempBinFolder, "*", SearchOption.AllDirectories);
                var metaFiles = allFiles.Where(file => file.EndsWith(".meta")).ToArray();
                foreach (var filePath in metaFiles) {
                    File.Delete(filePath);
                }
                var binFiles = allFiles.Where(file => !file.EndsWith(".meta")).ToArray();
                if (!BuilderSetting.UseZipResForPixUI)
                {
                    foreach (var filePath in binFiles)
                    {
                        File.Move(filePath, filePath + ".bytes");
                    }
                }
                AssetDatabase.ImportAsset(tempBinFolder, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.Refresh();
        }
        
        // 删除临时二进制文件
        private static void DeleteTempBinList(List<string> activityList)
        {
            foreach(string s in activityList)
            {
                var tempBinFolder = string.Format(BuilderSetting.TEMP_BIN_PATH_TEMPLATE, s);
                DeleteDir(tempBinFolder, false);        // PixUI Bin文件夹暂时非强制需要
			}
            AssetDatabase.Refresh();
        }

        // 打包二进制文件
        private static void BuildActivityBinIntoAssetbundle(List<string> activityList)
        {
            foreach (string s in activityList)
            {
                var tempBinFolder = string.Format(BuilderSetting.TEMP_BIN_PATH_TEMPLATE, s);
                if (!Directory.Exists(tempBinFolder)) {
                    continue;
                }
                var binPathList = GetBinPathList(tempBinFolder);
                if (binPathList.Count > 0)
                {
                    _assetPathListList.Add(binPathList);
                }
            }
        }

        //使用Zip方式打包PixUI资源
        private static void BuildActivityBinIntoZip(List<string> activityList)
        {
            foreach (string s in activityList)
            {
                var tempBinFolder = string.Format(BuilderSetting.TEMP_BIN_PATH_TEMPLATE, s);
                if (!Directory.Exists(tempBinFolder))
                {
                    continue;
                }
                string sourceDir = Path.Combine(Application.dataPath.Replace("Assets", ""), tempBinFolder);
                string zipName = string.Format("{0}_{1}_bin.zip", GetPlatformName(_buildTarget), s.ToLower());
                string dstPath = Path.Combine(BuilderSetting.STREAM_ASSET_PATH, zipName);
                ZipUnity zipUnity = new ZipUnity();
                var fileEntries = Directory.GetFileSystemEntries(sourceDir);
                zipUnity.Zip(Directory.GetFileSystemEntries(sourceDir), dstPath, null, new ZipCallback());
            }
        }


        private static List<string> GetLuaPathList(string luaFolder)
        {
            List<string> result = Directory.GetFiles(luaFolder, "*.lua.bytes", SearchOption.AllDirectories).Where<string>((s) => { return s.Contains(".meta") == false; }).ToList<string>();
            return result;
        }

        private static List<string> GetBinPathList(string binFolder)
        {
            List<string> result = Directory.GetFiles(binFolder, "*.*", SearchOption.AllDirectories).Where<string>((s) => { return s.Contains(".meta") == false; }).ToList<string>();
            return result;
        }

        private static string GetLuaBundleName(string luaAssetPath, BuildTarget target)
        {
            string dicPath = Path.GetDirectoryName(luaAssetPath);
            string architectureName = Path.GetFileName(dicPath);
            string dicPath2 = Path.GetDirectoryName(dicPath);
            string activityName = Path.GetFileName(dicPath2);
            string postfix = "_lua32.assetbundle";
            if (architectureName == "Lua64")
            {
                postfix = "_lua64.assetbundle";
            }
            string result = string.Empty;

            result = GetPlatformName(target) + "_" + activityName + postfix;
            return result;
        }

        private static string GetBinBundleName(string binPath, BuildTarget target) {
            binPath = binPath.Replace('\\', '/');
            binPath = binPath.Replace("//", "/");
            string activityName = Regex.Match(binPath, @"/(\w+?)/TempBin").Groups[1].Value;
            string result = string.Empty;
            result = GetPlatformName(target) + "_" + activityName + "_bin.assetbundle";
            return result;
        }

        private static void ExecuteBuild(BuildTarget target)
        {
#if UNITY_4_7 || UNITY_4_6
            ExecuteBuild_4_7(target);

#elif UNITY_5 || UNITY_2017_1_OR_NEWER
            ExecuteBuild_5_3(target);
#endif
        }
        
#if UNITY_4_7 || UNITY_4_6
        private static void ExecuteBuild_4_7(BuildTarget target)
        {
            for (int i = 0; i < _assetPathListList.Count; i++)
            {
                List<string> assetPathList = _assetPathListList[i];

                if (assetPathList[0].ToLower().Contains(".prefab") == true)
                {
                    BuildAssetList(assetPathList, target, typeof(UnityEngine.Object), GetPrefabBundleName);
                }
                else if(assetPathList[0].ToLower().Contains(".lua") == true)
                {
                    BuildAssetList(assetPathList, target, typeof(TextAsset), GetLuaBundleName);
                }
            }
        }

        private static void BuildAssetList(List<string> assetPathList, BuildTarget target, Type assetType, Func<string, BuildTarget, string> getBundleName)
        {
            UnityEngine.Object[] assets = new UnityEngine.Object[assetPathList.Count];
            for(int i = 0; i< assetPathList.Count; i++)
            {
                assets[i] = AssetDatabase.LoadAssetAtPath(assetPathList[i], assetType);
            }
            
            string bundleName = getBundleName(assetPathList[0], target).ToLower();
            string bundlePath = BuilderSetting.STREAM_ASSET_PATH + "/" + bundleName;
			BuildPipeline.BuildAssetBundle(assets[0], assets, bundlePath, GetBuildOptions(), target);
            _bundlePathList.Add(bundlePath);
        }

        private static BuildAssetBundleOptions GetBuildOptions()
        {
            return BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CompleteAssets;
        }
#endif

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        private static void ExecuteBuild_5_3(BuildTarget target)
        {
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            for (int i = 0; i < _assetPathListList.Count; i++)
            {
                List<string> assetPathList = _assetPathListList[i];
                if (assetPathList[0].ToLower().Contains(".prefab") == true)
                {
                    buildList.Add(BuildAssetList(assetPathList, target, GetPrefabBundleName));
                }
                else if (assetPathList[0].ToLower().Contains(".lua") == true)
                {
                    buildList.Add(BuildAssetList(assetPathList, target, GetLuaBundleName));
                } 
                else 
                {
                    buildList.Add(BuildAssetList(assetPathList, target, GetBinBundleName));
                }
            }
            BuildPipeline.BuildAssetBundles(BuilderSetting.STREAM_ASSET_PATH, buildList.ToArray(), BuildAssetBundleOptions.DeterministicAssetBundle, target);
        }

        private static AssetBundleBuild BuildAssetList(List<string> assetPathList, BuildTarget target, Func<string, BuildTarget, string> getBundleName)
        {
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = getBundleName(assetPathList[0], target).ToLower();
            build.assetNames = assetPathList.ToArray();
            _bundlePathList.Add(BuilderSetting.STREAM_ASSET_PATH + "/" + build.assetBundleName);
            return build;
        }
#endif

        private static void DeleteCopyPrefabList()
        {
            foreach (string s in _copyAssetList)
            {
                AssetDatabase.DeleteAsset(s);
            }
        }

        private static void DeleteVersionAssetList()
        {
            foreach(string s in _versionAssetList)
            {
                AssetDatabase.DeleteAsset(s);
            }
        }

        private static void ShowReport()
        {
            if (_bundlePathList.Count == 0)
            {
                EditorUtility.DisplayDialog("打包结果： ", "需要打包的文件列表为空", "朕知道了~");
                return;
            }
            foreach (string path in _bundlePathList)
            {
                //AssetDatabase.ImportAsset(path);
                StringBuilder sb = new StringBuilder();
                sb.Append("生成文件： ");
                sb.Append(path);
                sb.Append(" 体积： ");
                sb.Append(GetFileSize(path));
                sb.Append("kb");
                Debug.Log("<color=#00ff00>" + sb.ToString() + "</color>");
            }
        }

        //打包后将资源复制到游戏工程StreamingAssets目录下
        private static void CopyBundleToGame(string targetFolderPath)
        {
            try
            {
                if (Directory.Exists(targetFolderPath) == false)
                {
                    Directory.CreateDirectory(targetFolderPath);
                }
                foreach (string path in _bundlePathList)
                {
                    string targetPath = Path.Combine(targetFolderPath, Path.GetFileName(path));
                    File.Copy(path, targetPath, true);
                }
            }
            catch
            {
                Debug.LogError("复制bundle到 " + targetFolderPath + " 失败~");
            }
        }

        private static int GetFileSize(string path)
        {
            FileInfo info = new FileInfo(path);
            if (info.Exists == false)
            {
                Debug.LogError("未找到文件: " + path);
            }
            return Mathf.CeilToInt(info.Length / 1024.0f);
        }

        public static string GetPlatformName(BuildTarget target)
        {
            string result = string.Empty;
            if (BUILD_TARGET_NAME_DICT.ContainsKey(target))
            {
                result = BUILD_TARGET_NAME_DICT[target];
            }
            else
            {
                throw new Exception("发现未预定义平台描述信息~，请先添加相关信息。");
            }
            return result;
        }

        //将Unity资源路径转换为系统目录
        private static string ConvertToFilePath(string assetPath)
        {
            return Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
        }

        // 拷贝文件夹（包含文件）
        private static void CopyDir(string srcDir, string dstDir, bool forceOutputLog = true)
		{
            if (!Directory.Exists(srcDir))
			{
				if (forceOutputLog == true)
				{
					Debug.LogError(string.Format("dir not exist {0}", srcDir));
				}
                return;
            }
            if (Directory.Exists(dstDir))
			{
                DeleteDir(dstDir);
            }
            Directory.CreateDirectory(dstDir);
            var dirs = Directory.GetDirectories(srcDir, "*", SearchOption.AllDirectories).OrderBy(s => s.Length).ToArray();
            foreach (var dirPath in dirs)
			{
                Directory.CreateDirectory(dirPath.Replace(srcDir, dstDir));
            }
            var files = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories);
            foreach (var filePath in files)
			{
                File.Copy(filePath, filePath.Replace(srcDir, dstDir), true);
            }
        }

        // 删除文件夹（清空文件）
        private static void DeleteDir(string targetDir, bool forceOutputLog = true)
		{
            if (!Directory.Exists(targetDir))
			{
				if (forceOutputLog == true)
				{
					Debug.LogError(string.Format("dir not exist {0}", targetDir));
				}
                return;
            }
            var files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
            foreach (var filePath in files)
			{
                File.Delete(filePath);
            }
            var dirs = Directory.GetDirectories(targetDir, "*", SearchOption.AllDirectories).OrderByDescending(s => s.Length).ToArray();
            foreach (var dirPath in dirs)
			{
                Directory.Delete(dirPath);
            }
            Directory.Delete(targetDir);
        }

    }
    internal class ZipCallback : ZipUnity.ZipCallback
    {
        //meta文件不打入zip包
        public override bool OnPreZip(ZipEntry entry)
        {
            if (entry.Name.Contains(".meta"))
            {
                return false;
            }
            return true;
        }
    }
}
