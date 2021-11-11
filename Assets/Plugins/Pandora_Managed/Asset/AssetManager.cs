using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{
    /*
        * 资源加载流程
        * 1.检查AssetPool中是否已经存在，存在的话，从AssetPool中获取资源。
        * 2.检查本地Cache中是否已经存在，存在的话，从本地加载到内存。
        * 3.从远端加载到本地，然后从本地加载到内存。
        * 4.若3中从远端加载到本地后，本地保存文件失败，则从远端直接加载到内存
        * */
    public class AssetManager
    {
        private const int NAME_MAX_LENGTH = 128;
        //路径中不能存在的非法字符集 
        //参考链接：https://docs.microsoft.com/zh-cn/dotnet/api/system.io.path.getinvalidfilenamechars?redirectedfrom=MSDN&view=netframework-4.8#System_IO_Path_GetInvalidFileNameChars
        private static readonly char[] INVALID_FILE_NAME_CHARS = new char[]{
            '\0',
            '\u0001',
            '\u0002',
            '\u0003',
            '\u0004',
            '\u0005',
            '\u0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\u000e',
            '\u000f',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001a',
            '\u001b',
            '\u001c',
            '\u001d',
            '\u001e',
            '\u001f',
            '"',
            '<',
            '>',
            '|',
            ':',
            '*',
            '?',
            '\\',
            '/'
        };
        private const string LUA_ASSET_TOKEN = "_lua";
        private static bool _isInitialized = false;

        private static WwwLoader _wwwLoader;
        private static AssetLoader _assetLoader;
        private static BundleLoader _bundleLoader;
        private static CacheLoader _cacheLoader;

        public static void Initialize()
        {
            if(_isInitialized == false)
            {
                _isInitialized = true;
                GameObject pandoraGo = DelegateAggregator.GetGameObject();
                _wwwLoader = pandoraGo.AddComponent<WwwLoader>();
                _cacheLoader = pandoraGo.AddComponent<CacheLoader>();
                _cacheLoader.Initialize();
                _assetLoader = pandoraGo.AddComponent<AssetLoader>();
                _assetLoader.Initialize();
                _bundleLoader = pandoraGo.AddComponent<BundleLoader>();
                _bundleLoader.Initialize();
            }
        }

        public static void LoadWww(string url, string requestJson, bool isPost, Action<string> callback)
        {
            _wwwLoader.LoadWww(url, requestJson, isPost, callback);
        }

        public static void LoadWww(string url, string requestJson, Byte[] binaryData, bool isPost, Action<string> callback)
        {
            _wwwLoader.LoadWww(url, requestJson, binaryData, isPost, callback);
        }

        public static void CacheAsset(string url, Action<string> callback)
        {
            _cacheLoader.LoadAsset(url, callback);
        }
        public static void SetProgramAssetProgressCallback(Action<Dictionary<string, string>> callback)
        {
            _cacheLoader.programAssetProgressCallback = callback;
        }

        #region 初次加载Lua资源列表
        /// <summary>
        /// 加载资源分组，分两步
        /// 1.将分组文件全部加载到Cache目录下
        /// 2.将分组中Lua文件加载到内存
        /// </summary>
        /// <param name="assetInfoList"></param>
        /// <param name="callback"></param>
        public static void LoadProgramAssetList(string group, List<RemoteConfig.AssetInfo> assetInfoList, Action<string, List<RemoteConfig.AssetInfo>> callback)
        {
            List<RemoteConfig.AssetInfo> downloadAssetInfoList = new List<RemoteConfig.AssetInfo>();
            for (int i = 0; i < assetInfoList.Count; i++)
            {
                RemoteConfig.AssetInfo assetInfo = assetInfoList[i];
                assetInfo.url = GetAssetRealUrl(assetInfo.url);
                if (assetInfo.isDownloaded == false && (IsLoadProgramAssetFromStreamingAssetsFolder() == true || VerifyProgramAsset(assetInfo.url, assetInfo.md5) == false))
                {
                    DeleteCachedAsset(assetInfo.url, true);
                    downloadAssetInfoList.Add(assetInfo);
                }
            }
            if (downloadAssetInfoList.Count == 0)
            {
                LoadLuaAssetList(group, assetInfoList, callback);
            }
            else
            {
                _cacheLoader.LoadProgramAssetList(group, downloadAssetInfoList, delegate (List<string> assetUrlList) { MarkDownloadedAssetInfo(downloadAssetInfoList); LoadLuaAssetList(group, assetInfoList, callback); });
            }
        }
        
        private static void MarkDownloadedAssetInfo(List<RemoteConfig.AssetInfo> assetInfoList)
        {
            for(int i = 0; i < assetInfoList.Count; i++)
            {
                assetInfoList[i].isDownloaded = true;
            }
        }

        /// <summary>
        /// 为了避免每次文件修改都提交cdn，可以设置测试阶段从本地StreamingAssets目录下下载资源
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetAssetRealUrl(string url)
        {
            if (IsLoadProgramAssetFromStreamingAssetsFolder() == false)
            {
                return url;
            }
            string name = Path.GetFileName(url);
            if (LocalDirectoryHelper.IsStreamingAssetsExists(name) == false)
            {
                return url;
            }
            return Path.Combine(LocalDirectoryHelper.GetStreamingAssetsUrl(), name);
        }

        /// <summary>
        /// 1.检查程序缓存文件是否存在
        /// 2.检查记录中缓存文件的Md5值和RemoteConfig中相应值是否相同
        /// </summary>
        /// <param name="url"></param>
        /// <param name="assetMd5"></param>
        /// <returns></returns>
        public static bool VerifyProgramAsset(string url, string assetMd5)
        {
            string assetPath = GetCachedAssetPath(url, true);
            if (File.Exists(assetPath) == false)
            {
                return false;
            }

            if (DelegateAggregator.GetRemoteConfig().GetFunctionSwitch("turnOffVerification") == true)
            {
                return true;
            }

            string existFileMd5 = CachedFileMd5Helper.GetFileMd5(assetPath);
            if (existFileMd5 == assetMd5)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 第二步将Lua资源加载到内存
        /// </summary>
        /// <param name="assetInfoList"></param>
        /// <param name="callback"></param>
        private static void LoadLuaAssetList(string group, List<RemoteConfig.AssetInfo> assetInfoList, Action<string, List<RemoteConfig.AssetInfo>> callback)
        {
            List<AssetConfig> configList = new List<AssetConfig>();
            //是否所有的Lua资源都在第一步中成功加载到了本地
            bool isAllLuaAssetInCache = true;
            for (int i = 0; i < assetInfoList.Count; i++)
            {
                RemoteConfig.AssetInfo assetInfo = assetInfoList[i];
                if(assetInfo.url.Contains(LUA_ASSET_TOKEN) == true)
                {
                    configList.Add(new AssetConfig(AssetType.Lua, true, true, true, assetInfo.url));
                    if(IsAssetCached(assetInfo.url, true) == false)
                    {
                        isAllLuaAssetInCache = false;
                    }
                }
            }
            if(isAllLuaAssetInCache == true)
            {
                _bundleLoader.LoadLuaAssetList(configList, delegate (List<string> urlList) { callback(group, assetInfoList); });
            }
            else
            {
                _assetLoader.LoadLuaAssetList(configList, delegate (List<string> urlList) { callback(group, assetInfoList); });
            }
        }
        #endregion

        public static void GetPanelGameObject(RemoteConfig.AssetInfo assetInfo, Action<GameObject> callback)
        {
            LoadAsset<GameObject>(AssetType.Prefab, false, true, true, assetInfo.url, callback);
        }

		public static void GetBinary(string url, Action<object> callback)
		{
			LoadAsset<object>(AssetType.Binary, false, true, true, url, callback);
		}

        public static void GetZip(string url, Action<object> callback)
        {
            LoadAsset<object>(AssetType.Zip, false, true, false, url, callback);
        }
		public static void GetText(string url, Action<string> callback)
        {
            LoadAsset<string>(AssetType.Text, false, false, false, url, callback);
        }

		public static void GetRawData(string url, Action<byte[]> callback)
        {
            LoadAsset<byte[]>(AssetType.RawData, false, false, false, url, callback);
        }

        public static void GetGameObject(string url, bool isCacheInMemory, Action<GameObject> callback)
        {
            LoadAsset<GameObject>(AssetType.Prefab, isCacheInMemory, false, true, url, callback);
        }

        public static void GetAssetBundle(string url, Action<AssetBundle> callback)
        {
            LoadAsset<AssetBundle>(AssetType.Assetbundle, false, false, true, url, callback);
        }

        public static void GetImage(string url, bool isCacheInMemory, Action<Texture2D> callback)
        {
            LoadAsset<Texture2D>(AssetType.Image, isCacheInMemory, false, false, url, callback);
        }

        public static void GetPortrait(string url, bool isCacheInMemory, Action<Texture2D> callback)
        {
            LoadAsset<Texture2D>(AssetType.Portrait, isCacheInMemory, false, false, url, callback);
        }

        public static void GetAudioClip(string url, bool isCacheInMemory, Action<AudioClip> callback)
        {
            LoadAsset<AudioClip>(AssetType.Audio, isCacheInMemory, false, false, url, callback);
        }

		private static void LoadAsset<T>(AssetType assetType, bool isCacheInMemory, bool isProgramAsset, bool isAssetBundle, string url, Action<T> callback) where T : class
        {
            AssetConfig config = new AssetConfig(assetType, isCacheInMemory, isProgramAsset, isAssetBundle, url);

            if (config.IsInPool == true)
            {
                GetAssetFromPool(config, callback);
            }
            else
            {
                if (config.IsInCache == true)
                {
                    LoadAssetFromCache<T>(config, callback);
                }
                else
                {
                    LoadAssetFromRemote<T>(config, callback);
                }
            }
        }

        private static void GetAssetFromPool<T>(AssetConfig config, Action<T> callback) where T : class
        {
            T asset = AssetPool.GetAsset(config.GetPoolKey()) as T;
            if (config.type == AssetType.Prefab)
            {
                GameObject go = null;
                go = GameObject.Instantiate(asset as GameObject) as GameObject;
#if UNITY_EDITOR
                ReplaceUIShader(go);
                ReplaceRendererShader(go);
#endif
                asset = go as T;
            }
            if (callback != null)
            {
                callback(asset);
            }
        }

        //从本地Cache加载资源
        private static void LoadAssetFromCache<T>(AssetConfig config, Action<T> callback) where T : class
        {
            if(config.IsInCache == true && config.isAssetBundle == true)
            {
                _bundleLoader.LoadAsset(config, delegate (string url) { GetAssetFromPool<T>(config, callback); });
                return;
            }
            _assetLoader.LoadAsset(config, delegate (string url) { GetAssetFromPool<T>(config, callback); });
        }

        //从远端加载资源
        private static void LoadAssetFromRemote<T>(AssetConfig config, Action<T> callback) where T : class
        {
            _cacheLoader.LoadAsset(config.url, delegate (string url) { LoadAssetFromCache<T>(config, callback); });
        }

        public static byte[] GetLuaBytes(string name)
        {
            string luaKey = name + ".lua";
            TextAsset asset = AssetPool.GetAsset(luaKey) as TextAsset;
            if(asset != null)
            {
                return asset.bytes;
            }
            asset = AssetPool.FindLuaAsset(luaKey);
            if(asset != null)
            {
                return asset.bytes;
            }
            return null;
        }

        public static void DeleteCachedAsset(string url, bool isProgramAsset =  false)
        {
            string assetPath = GetCachedAssetPath(url, isProgramAsset);
            if (File.Exists(assetPath) == true)
            {
                try
                {
                    File.Delete(assetPath);
                }
                catch
                {
                    Logger.LogError("删除文件出错： " + assetPath, PandoraSettings.SDKTag);
                }
            }
        }
        //是否从安装包的StreamingAssets目录下加载程序资源
        public static bool IsLoadProgramAssetFromStreamingAssetsFolder()
        {
            if (PandoraSettings.UseStreamingAssets == true)
            {
                return true;
            }
            if (DelegateAggregator.GetRemoteConfig().GetFunctionSwitch("loadAssetFromLocal") == true)
            {
                return true;
            }
            return false;
        }

        public static void ReleaseLuaAsset(string name)
        {
            string luaKey = name + ".lua";
            AssetPool.ReleaseLuaAsset(luaKey);
        }

        public static void ReleaseProgramAsset(RemoteConfig.AssetInfo assetInfo)
        {
            if(assetInfo != null)
            {
                string poolKey = GetAssetPoolKey(assetInfo.url, true);
                AssetPool.ReleaseAsset(poolKey);
            }
        }

        public static void ReleaseAsset(string url)
        {
            string poolKey = GetAssetPoolKey(url, false);
            AssetPool.ReleaseAsset(poolKey);
        }

        /// <summary>
        /// 获取资源对象的通用接口
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static System.Object GetAsset(string url)
        {
            string poolKey = GetAssetPoolKey(url, false);
            return AssetPool.GetAsset(poolKey);
        }

        public static bool IsProgramAssetCached(List<RemoteConfig.AssetInfo> assetInfoList)
        {
            for (int i = 0; i < assetInfoList.Count; i++)
            {
                RemoteConfig.AssetInfo assetInfo = assetInfoList[i];
                if (IsAssetCached(assetInfo.url, true) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查缓存资源是否存在
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsAssetCached(string url, bool isProgramAsset = false)
        {
            string assetPath = GetCachedAssetPath(url, isProgramAsset);
            return File.Exists(assetPath);
        }

        public static string GetCachedName(string url)
        {
            string name = Path.GetFileName(GetUrlWithoutParameters(url));
            for (int i = 0; i < INVALID_FILE_NAME_CHARS.Length; i++)
            {
				int index = name.IndexOf(INVALID_FILE_NAME_CHARS[i]);
                while (index != -1)
                {
                    name = name.Remove(index, 1);
                    index = name.IndexOf(INVALID_FILE_NAME_CHARS[i]);
                }
            }
            name = url.GetHashCode().ToString("X8") + "-" + name;
            if(name.Length > NAME_MAX_LENGTH)
            {
                name = name.Substring(0, NAME_MAX_LENGTH);
            }
            return name;
        }

        private static string GetUrlWithoutParameters(string url)
        {
            string result = string.Empty;
            try
            {
                result = new Uri(url).AbsolutePath;
            }
            catch
            {
                result = url;
            }
            return result;
        }

        public static string GetCachedAssetUrl(string url, bool isProgramAsset)
        {
            return PandoraSettings.GetFileProtocolToken() + GetCachedAssetPath(url, isProgramAsset);
        }

        public static string GetCachedAssetPath(string url, bool isProgramAsset = false)
        {
            if (isProgramAsset == true)
            {
                return Path.Combine(LocalDirectoryHelper.GetProgramAssetFolderPath(), Path.GetFileName(url));
            }
            return Path.Combine(LocalDirectoryHelper.GetAssetFolderPath(), GetCachedName(url));
        }

        public static string GetAssetPoolKey(string url, bool isProgramAsset)
        {
            if (IsAssetCached(url, isProgramAsset) == true)
            {
                return GetCachedAssetUrl(url, isProgramAsset);
            }
            return url;
        }

        public static void Clear()
        {
            _assetLoader.Clear();
            _bundleLoader.Clear();
            _cacheLoader.Clear();
            AssetPool.Clear();
        }

        public static void DeleteZeroReferenceAsset(bool isForce = false)
        {
            List<string> deleteList = AssetPool.DeleteZeroReferenceAsset(isForce);
            for (int i = 0; i < deleteList.Count; i++)
            {
                _assetLoader.RemoveFormLoadedSet(deleteList[i]);
                _bundleLoader.RemoveFormLoadedSet(deleteList[i]);
            }
        }

        public static void ForceDeleteZeroReferenceTexture(string url)
        {
            string key = GetAssetPoolKey(url, false);
            if(AssetPool.HasAsset(key) == true && AssetPool.ForceDeleteZeroReferenceTexture(key) == true)
            {
                _assetLoader.RemoveFormLoadedSet(key);
                _bundleLoader.RemoveFormLoadedSet(key);
            }
        }

        //写入资源
        public static void WriteAllBytes(string path, byte[] bytes, int errorCode)
        {
            int maxRetryCount = 3;
            for (int i = 0; i < maxRetryCount; i++)
            {
                try
                {
                    File.WriteAllBytes(path, bytes);
                    break;
                }
                catch (Exception e)
                {
                    if (i == (maxRetryCount - 1))
                    {
                        string error = "创建本地文件失败： " + path + " " + e.Message;
                        DelegateAggregator.ReportError(error, errorCode);
                        Logger.LogError(error, PandoraSettings.SDKTag);
                    }
                }
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// 在pc下测试移动平台的Prefab时，需要将assetbundle包中的shader替换为pc下的版本
        /// </summary>
        /// <param name="bundle"></param>
        private static void ReplaceUIShader(GameObject go)
        {
            List<Material> materialList = DelegateAggregator.GetUIMaterialList(go);
            for (int i = 0; i < materialList.Count; i++)
            {
                Material material = materialList[i];
                if (material != null)
                {
                    string shaderName = material.shader.name;
                    Shader shader = Shader.Find(shaderName);
                    if (shader != null)
                    {
                        material.shader = shader;
                    }
                }
            }
        }

        private static void ReplaceRendererShader(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material material = materials[j];
                    string shaderName = material.shader.name;
                    Shader shader = Shader.Find(shaderName);
                    if (shader != null)
                    {
                        material.shader = shader;
                    }
                    else
                    {
                        Logger.LogWarning("未找到Shader： " + shaderName, PandoraSettings.SDKTag);
                    }
                }
            }
        }
#endif
    }

}
