//#define USING_NGUI
#define USING_UGUI

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace com.tencent.pandora
{
    /// <summary>
    /// 注意：
    /// 1.因为将Lua打包为AssetBundle时，lua文件的路径信息会丢失,所以模块下的Lua文件不能有子目录，并且所有Lua文件不能重名
    /// 2.Pandora资源Prefab暂不使用资源公用机制，所以资源加载后可以立马Unload(false)
    /// </summary>
    public class AssetPool
    {
        /// <summary>
        /// lua资源不释放
        /// </summary>
        public const int LUA_INIT_REFERENCE_COUNT = 999;
        //资源引用计数为0后再缓存一段时间后释放引用
        public const int DELETE_ASSET_INTERVAL = 60;

        //强引用资源字典
        private static Dictionary<string, Asset> _strongAssetDict = new Dictionary<string, Asset>();
        //弱引用资源字典
        //短时间内持有资源的一个引用，释放引用后可以被Resource.UnloadUnusedAssets回收
        //可以避免短时间内加载重复资源时创建重复的内存
        private static Dictionary<string, Asset> _weakAssetDict = new Dictionary<string, Asset>();
        private static List<string> _deletedKeyList = new List<string>();

        private static Dictionary<string, bool> _needSandBoxDict = new Dictionary<string, bool>();

        private static string buildConfigName = "buildConfig";


        private static bool FindBuildConfigFileName(UnityEngine.Object obj)
        {
            return obj.name == buildConfigName;
        }

        public static void AddLuaAsset(UnityEngine.Object[] luas)
        {
            for (int i = 0; i < luas.Length; i++)
            {
                UnityEngine.Object o = luas[i];
                if (o.name != buildConfigName)
                {
                    AddAsset(o.name, o, LUA_INIT_REFERENCE_COUNT, true);
                }
            }
        }

        public static void AddLuaAssetByPath(UnityEngine.Object[] luas, Dictionary<string, object> luaPathMap)
        {
            for (int i = 0; i < luas.Length; i++)
            {
                UnityEngine.Object o = luas[i];
                string path = o.name;
                if (luaPathMap.ContainsKey(path))
                {
                    path = luaPathMap[path] as string;
                }
                if (o.name != buildConfigName)
                {
                    AddAsset(path, o, LUA_INIT_REFERENCE_COUNT, true);
                }
            }
        }

        public static bool ParseLuaAsset(string url, AssetBundle assetBundle, AssetBundleRequest request)
        {
            bool result = false;
            try
            {
#if UNITY_2017_1_OR_NEWER
                UnityEngine.Object[] luas = request.allAssets;
#elif UNITY_5
                UnityEngine.Object[] luas = assetBundle.LoadAllAssets(typeof(TextAsset));
#elif UNITY_4_6 || UNITY_4_7
                UnityEngine.Object[] luas = assetBundle.LoadAll(typeof(TextAsset));
#endif
                UnityEngine.Object buildConfigFile =  Array.Find(luas, FindBuildConfigFileName);

                _needSandBoxDict.Add(assetBundle.name, false);

                bool needFallback = true;
                if (buildConfigFile != null && (buildConfigFile as TextAsset))
                {
                    TextAsset buildConfigText = buildConfigFile as TextAsset;
                    Logger.Log("buildConfigDict " + buildConfigText.text,PandoraSettings.SDKTag);
                    Dictionary<string, object> buildConfigDict = MiniJSON.Json.Deserialize(buildConfigText.text) as Dictionary<string, object>;
                    if (buildConfigDict.ContainsKey("sandbox"))
                    {
                        _needSandBoxDict[assetBundle.name] = (buildConfigDict["sandbox"] as string) == "1";
                    }
                    if (buildConfigDict.ContainsKey("luaFilePath"))
                    {
                        string luaPathMapStr = buildConfigDict["luaFilePath"] as string;
                        if (luaPathMapStr != null)
                        {
                            Dictionary<string, object> luaPathMap = MiniJSON.Json.Deserialize(luaPathMapStr) as Dictionary<string, object>;
                            if (luaPathMap != null && luaPathMap.Count == luas.Length - 1)
                            {
                                AddLuaAssetByPath(luas, luaPathMap);
                                needFallback = false;
                            }
                        }
                    }
                    
                }
                if (needFallback)
                {
                    Logger.Log("luaFilePathAsset not find " + assetBundle.name,PandoraSettings.SDKTag);
                    AddLuaAsset(luas);
                }

                assetBundle.Unload(false);
                result = true;
            }
            catch (Exception e)
            {
                string error = "资源解析失败 " + url + "\n" + e.Message;
                DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                Logger.LogError(error, PandoraSettings.SDKTag);
                LocalDirectoryHelper.DeleteAssetByUrl(url);
            }
            return result;
        }


        public static bool NeedSandBox(string luaName)
        {
            int size = IntPtr.Size;//由系统指针的size获得当前运行程序的平台是32位还是64位，size等于4时为32位，size等于8时为64位
            string Token = (size == 4) ? "lua32" : "lua64";
            string assetbundleName =  PandoraSettings.GetPlatformDescription() + "_" + luaName.ToLower() + "_" + Token + ".assetbundle";
            if (_needSandBoxDict.ContainsKey(assetbundleName))
            {
                return _needSandBoxDict[assetbundleName];
            }
            return false;
        }

        //支持以大小写不敏感的方式搜索Lua资源
        public static TextAsset FindLuaAsset(string key)
        {
            var enumerator = _strongAssetDict.Keys.GetEnumerator();
            while(enumerator.MoveNext())
            {
                if(enumerator.Current.ToLower() == key)
                {
                    return _strongAssetDict[enumerator.Current].Object as TextAsset;
                }
            }
            return null;
        }

        public static void ReleaseLuaAsset(string key)
        {
            string realKey = key;
            if (_strongAssetDict.ContainsKey(realKey) == false)
            {
                var enumerator = _strongAssetDict.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if(enumerator.Current.ToLower() == key)
                    {
                        realKey = enumerator.Current;
                        break;
                    }
                }
            }
            if (_strongAssetDict.ContainsKey(realKey) == true)
            {
                _strongAssetDict.Remove(realKey);
            }
        }

        private static void AddAsset(string key, System.Object obj, int referenceCount, bool isCacheInMemory)
        {
            if(_strongAssetDict.ContainsKey(key) == true)
            {
                Logger.LogError("StrongAssetDict 重复资源： " + key, PandoraSettings.SDKTag);
                return;
            }
            if(_weakAssetDict.ContainsKey(key) == true)
            {
                Logger.LogError("WeakAssetDict 重复资源： " + key, PandoraSettings.SDKTag);
                return;
            }
            
            if(isCacheInMemory == true)
            {
                _strongAssetDict.Add(key, new Asset(key, obj, referenceCount));
            }
            else
            {
                _weakAssetDict.Add(key, new Asset(key, obj, 0));
            }
        }

        public static bool HasAsset(string key)
        {
            return IsInStrongDict(key) || IsInWeakDict(key);
        }

        private static bool IsInStrongDict(string key)
        {
            if (_strongAssetDict.ContainsKey(key) == false)
            {
                return false;
            }
            return true;
        }

        private static bool IsInWeakDict(string key)
        {
            if(_weakAssetDict.ContainsKey(key) == false)
            {
                return false;
            }
            return true;
        }

        public static System.Object GetAsset(string key)
        {
            if(IsInStrongDict(key) == true)
            {
                Asset asset = _strongAssetDict[key];
                asset.ReferenceCount += 1;
                return asset.Object;
            }
            else if(IsInWeakDict(key) == true)
            {
                Asset asset = _weakAssetDict[key];
                return asset.Object;
            }
            return null;
        }

        public static void ReleaseAsset(string key)
        {
            if(IsInStrongDict(key) == false)
            {
                return;
            }
            Asset asset = _strongAssetDict[key];
            asset.ReferenceCount -= 1;
        }

        public static bool ParsePrefabAsset(string url, AssetBundle assetBundle, AssetBundleRequest request, bool isCacheInMemory)
        {
            bool result = false;
            try
            {
#if UNITY_2017_1_OR_NEWER
                UnityEngine.Object[] prefabs = new UnityEngine.Object[] { request.asset };
#elif UNITY_5
                UnityEngine.Object[] prefabs = assetBundle.LoadAllAssets(typeof(GameObject));
#elif UNITY_4_7 || UNITY_4_6
                UnityEngine.Object[] prefabs = new UnityEngine.Object[] { assetBundle.mainAsset };
#endif
                if (prefabs.Length > 1)
                {
                    throw new Exception("资源打包不符合规范，一个AssetBundle中只能有一个Prefab， " + url);
                }
                for (int i = 0; i < prefabs.Length; i++)
                {
                    UnityEngine.Object o = prefabs[i];
                    AddAsset(url, o, 0, isCacheInMemory);
                }
                assetBundle.Unload(false);
                result = true;
            }
            catch (Exception e)
            {
                string error = "资源解析失败： " + url + "\n" + e.Message;
                DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                Logger.LogError(error,PandoraSettings.SDKTag);
                LocalDirectoryHelper.DeleteAssetByUrl(url);
            }
            return result;
        }

		public static bool ParseImageAsset(string url, WwwWrapper wwwWrapper, bool isCacheInMemory)
		{
            bool result = false;
            try
            {
                Texture2D texture;
                if( PandoraSettings.UseLinearColorSpace )
                {
                    texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, PandoraSettings.UseLinearColorSpace);
                    texture.LoadImage(wwwWrapper.TextureNonReadable.EncodeToPNG(), false);
                }
                else
                {
                    texture = wwwWrapper.TextureNonReadable;
                }
                texture.name = url;
                texture.wrapMode = TextureWrapMode.Clamp;
                AddAsset(url, texture, 0, isCacheInMemory);
                result = true;
            }
            catch (Exception e)
            {
                string error = "资源解析失败： " + url + "\n" + e.Message;
                DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                Logger.LogError(error, PandoraSettings.SDKTag);
                LocalDirectoryHelper.DeleteAssetByUrl(url);
            }
            return result;
        }

		public static bool ParsePortraitAsset(string url, WwwWrapper wwwWrapper, bool isCacheInMemory)
		{
            bool result = false;
            try
            {
				Texture2D texture = GIFDecoder.Decode(wwwWrapper.Bytes, PandoraSettings.UseLinearColorSpace) ?? wwwWrapper.TextureNonReadable;
				texture.name = url;
                AddAsset(url, texture, 0, isCacheInMemory);
                result = true;
            }
            catch (Exception e)
            {
                string error = "资源解析失败： " + url + "\n" + e.Message;
                DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                Logger.LogError(error, PandoraSettings.SDKTag);
                LocalDirectoryHelper.DeleteAssetByUrl(url);
            }
            return result;
        }

		public static bool ParseAudioAsset(string url, WwwWrapper wwwWrapper, bool isCacheInMemory)
		{
            bool result = false;
            try
            {
				AudioClip clip = wwwWrapper.AudioClipCompressed;
				clip.name = url;
                AddAsset(url, clip, 0, isCacheInMemory);
                result = true;
            }
            catch (Exception e)
            {
                string error = "资源解析失败: " + url + "\n" + e.Message;
                DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                Logger.LogError(error, PandoraSettings.SDKTag);
                LocalDirectoryHelper.DeleteAssetByUrl(url);
            }
            return result;
        }

		public static bool ParseBinAsset(string url, WwwWrapper wwwWrapper, AssetBundle assetBundle, AssetBundleRequest request)
		{
			bool result = false;
			try
			{
				UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
                var assetDict = assets.ToDictionary(t => t.name, t => (t as TextAsset) != null ? ((TextAsset)t).bytes : null);
                AddAsset(url, assetDict, 0, false);
				result = true;
				assetBundle.Unload(false);
			}
			catch (Exception e)
			{
				string error = "资源解析失败 " + url + "\n" + e.Message;
				DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
				Logger.LogError(error, PandoraSettings.SDKTag);
				LocalDirectoryHelper.DeleteAssetByUrl(url);
			}
			return result;
		}

        public static bool ParseZipAsset(string url, WwwWrapper wwwWrapper, bool isCacheInMemory)
        {
            bool result = false;
            try
            {
                ZipUnity zipUnity = new ZipUnity();
                UnZipCallback callback = new UnZipCallback(url, isCacheInMemory);
                result = zipUnity.UnZipBytes(wwwWrapper.Bytes, null, callback);
            }
            catch (Exception e)
            {
                string error = "资源解析失败 " + url + "\n" + e.Message;
                DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                Logger.LogError(error, PandoraSettings.SDKTag);
                LocalDirectoryHelper.DeleteAssetByUrl(url);
            }
            return result;
        }
		public static bool ParseAssetBundle(string url, AssetBundle assetBundle)
        {
            AddAsset(url, assetBundle, 0, false);
            return true;
        }

		//此处暂只支持使用zlib压缩算法的压缩文件
		public static bool ParseTextAsset(string url, WwwWrapper wwwWrapper, bool isCacheInMemory)
		{
			if (wwwWrapper != null)
            {
                if(url.Contains(".zlib") || url.Contains(".zip") || url.Contains(".gz") || url.Contains(".tar"))
                {
					string content = Miniz.UnCompressToString(wwwWrapper.Bytes, wwwWrapper.Bytes.Length);
					if (string.IsNullOrEmpty(content) == true)
                    {
                        string error = "资源解压失败： " + url;
                        DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                        Logger.LogError(error, PandoraSettings.SDKTag);
                    }
                    AddAsset(url, content, 0, isCacheInMemory);
                }
                else
                {
					AddAsset(url, Encoding.UTF8.GetString(wwwWrapper.Bytes), 0, isCacheInMemory);
				}
			}
            return true;
        }

		public static bool ParseRawDataAsset(string url, WwwWrapper wwwWrapper, bool isCacheInMemory)
		{
			if (wwwWrapper != null)
            {
				AddAsset(url, wwwWrapper.Bytes, 0, isCacheInMemory);
			}
            return true;
        }

        public static void Clear()
        {
            _strongAssetDict.Clear();
            _weakAssetDict.Clear();
            _needSandBoxDict.Clear();
        }

        public static List<string> DeleteZeroReferenceAsset(bool isForce = false)
        {
            _deletedKeyList.Clear();
            DeleteZeroReferenceAsset(_strongAssetDict, isForce);
            DeleteZeroReferenceAsset(_weakAssetDict, isForce);
            return _deletedKeyList;
        }

        private static void DeleteZeroReferenceAsset(Dictionary<string, Asset> assetDict, bool isForce)
        {
            var enumerator = assetDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = enumerator.Current.Key;
                Asset asset = enumerator.Current.Value;
                if(isForce == true)
                {
                    if(asset.ReferenceCount == 0)
                    {
                        _deletedKeyList.Add(key);
                    }
                }
                else
                {
                    if (Time.realtimeSinceStartup > asset.ReleaseTime)
                    {
                        _deletedKeyList.Add(key);
                    }
                }
            }
            for (int i = 0; i < _deletedKeyList.Count; i++)
            {
                assetDict.Remove(_deletedKeyList[i]);
            }
        }

        public static bool ForceDeleteZeroReferenceTexture(string key)
        {
            if (_strongAssetDict.ContainsKey(key) == true)
            {
                Asset asset = _strongAssetDict[key];
                if ((asset.Object is Texture2D) && _strongAssetDict[key].ReferenceCount <= 0)
                {
                    Texture2D.Destroy(asset.Object as Texture2D);
                    _strongAssetDict.Remove(key);
                    _weakAssetDict.Remove(key);
                    return true;
                }
            }
            if (_weakAssetDict.ContainsKey(key) == true)
            {
                Asset asset = _weakAssetDict[key];
                if (asset.Object is Texture2D)
                {
                    Texture2D.Destroy(asset.Object as Texture2D);
                    _weakAssetDict.Remove(key);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 供UnityEditor下检查资源引用次数工具调用
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetAssetReferenceCountDict()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            var sEnumerator = _strongAssetDict.Keys.GetEnumerator();
            while (sEnumerator.MoveNext())
            {
                string key = sEnumerator.Current;
                result.Add(key, _strongAssetDict[key].ReferenceCount);
            }
            var wEnumerator = _weakAssetDict.Keys.GetEnumerator();
            while (wEnumerator.MoveNext())
            {
                string key = wEnumerator.Current;
                result.Add(key, _weakAssetDict[key].ReferenceCount);
            }
            return result;
        }

        /// <summary>
        /// 供UnityEditor下检查Prefab资源依赖的Texture列表及其尺寸
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, HashSet<Texture>> GetPrefabTextureSetDict()
        {
            Dictionary<string, HashSet<Texture>> result = new Dictionary<string, HashSet<Texture>>();
            var sEnumerator = _strongAssetDict.Keys.GetEnumerator();
            while (sEnumerator.MoveNext())
            {
                string key = sEnumerator.Current;
                if(_strongAssetDict[key].Object is GameObject)
                {
                    result[key] = GetGameObjectTextureSet(_strongAssetDict[key].Object as GameObject);
                }
            }
            var wEnumerator = _weakAssetDict.Keys.GetEnumerator();
            while (wEnumerator.MoveNext())
            {
                string key = wEnumerator.Current;
                if(_weakAssetDict[key].Object is GameObject)
                {
                    result[key] = GetGameObjectTextureSet(_weakAssetDict[key].Object as GameObject);
                }
            }
            return result;
        }

        private static HashSet<Texture> GetGameObjectTextureSet(GameObject go)
        {
            return DelegateAggregator.GetGameObjectTextureSet(go);
        }

        internal class Asset
        {
            private int _referenceCount;

            public Asset(string key, System.Object obj, int referenceCount)
            {
                this.Key = key;
                this.Object = obj;
                this.ReferenceCount = referenceCount;
            }
            /// <summary>
            /// 资源在相关字典中的Key，面板Prefab的Key为name，其他资源是url
            /// </summary>
            public string Key { get; set; }

            public System.Object Object { get; set; }

            public float ReleaseTime { get; set; }

            public int ReferenceCount
            {
                get
                {
                    return _referenceCount;
                }
                set
                {
                    _referenceCount = value;
                    if(_referenceCount == 0)
                    {
                        this.ReleaseTime = Time.realtimeSinceStartup + AssetPool.DELETE_ASSET_INTERVAL;
                    }
                    else
                    {
                        this.ReleaseTime = float.MaxValue;
                    }
                }
            }

        }

        internal class UnZipCallback : ZipUnity.UnZipCallback
        {
            private Dictionary<string, byte[]> _unzippedData = new Dictionary<string, byte[]>();
            private string _url;
            private bool _isCacheInMemory;

            public UnZipCallback(string url, bool isCacheInMemory)
            {
                this._url = url;
                this._isCacheInMemory = isCacheInMemory;
            }

            public override void OnFinished(bool isSuccess)
            {
                base.OnFinished(isSuccess);
                if (isSuccess)
                {
                    AssetPool.AddAsset(_url, _unzippedData, 0, _isCacheInMemory);
                }
                else
                {
                    Logger.LogError("Unzip resource failed", PandoraSettings.SDKTag);
                }
            }

            public override void OnPostUnzip(ZipEntry _entry, string filePath, byte[] fileContent)
            {
                base.OnPostUnzip(_entry, filePath, fileContent);
                string key = GetFileName(filePath);
                if (!string.IsNullOrEmpty(key) && fileContent != null && !_unzippedData.ContainsKey(key))
                {
                    _unzippedData.Add(key, fileContent);
                }
                else
                {
                    Logger.LogError("_unzippedData update failed", PandoraSettings.SDKTag);
                }
            }

            private string GetFileName(string filePath)
            {
                return filePath.Split(new char[] { '/' }).LastOrDefault();
            }
        }
    }
}
