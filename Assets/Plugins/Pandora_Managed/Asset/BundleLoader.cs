using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_5 || UNITY_2017_1_OR_NEWER
namespace com.tencent.pandora
{
    /// <summary>
    /// BundleLoader 使用Assetbunle相关接口将本地的Assetbundle资源加载到内存
    /// </summary>
    public class BundleLoader : AbstractLoader
    {
        private Dictionary<string, AssetConfig> _assetConfigDict = new Dictionary<string, AssetConfig>();

        public void LoadLuaAssetList(List<AssetConfig> configList, Action<List<string>> callback)
        {
            List<string> urlList = new List<string>();
            for(int i = 0; i < configList.Count; i++)
            {
                AssetConfig config = configList[i];
                string url = config.GetDownloadUrl();
                _assetConfigDict[url] = config;
                urlList.Add(url);
            }
            base.LoadAssetList(urlList, callback, 0);
        }

        public void LoadAsset(AssetConfig config, Action<string> callback)
        {
            Logger.Log("<color=#0000ff>将资源从本地加载到内存</color>, url: " + config.url, PandoraSettings.SDKTag);
            string url = config.GetDownloadUrl();
            _assetConfigDict[url] = config;
            base.LoadAsset(url, callback, 0);
        }

        protected override IEnumerator DaemonLoad()
        {
            while(true)
            {
                if(_toLoadQueue.Count > 0 && PandoraSettings.PauseDownloading == false)
                {
                    string url = _toLoadQueue.Dequeue();
                    string path = ConvertUrlToPath(url);
                    _loadingSet.Add(url);
                    AssetBundle bundle = AssetBundle.LoadFromFile(path);
                    if (bundle == null)
                    {
                        _errorQueue.Enqueue(url);
                        _loadingSet.Remove(url);
                        string error = "从本地加载assetbundle失败url: " + url;
                        Logger.LogError(error, PandoraSettings.SDKTag);
                        DelegateAggregator.ReportError(error, ErrorCode.ASSET_PARSE_FAILED);
                    }
                    else
                    {
                        AssetBundleRequest request = GetAssetBundleRequest(bundle);
                        if(request != null)
                        {
#if UNITY_EDITOR && UNITY_2018_4_16
                            // UNITY2018.4.16 Editor版本执行yield return request函数存在Bug
                            // 对于Prefab类型资源，会卡住无返回，这里先单独特殊处理
                            while (true)
                            {
                                if (request.progress > 0.99f)
                                    break;
                                yield return null;
                            }
#else
                            //https://docs.unity3d.com/ScriptReference/AsyncOperation.html
                            yield return request;
#endif
                        }
                        _loadingSet.Remove(url);
                        if (HandleLoadedContent(url, bundle, request) == true)
                        {
                            _errorMaxRetryCountDict.Remove(url);
                            _errorRetryCountDict.Remove(url);
                            _loadedSet.Add(url);
                        }
                        else
                        {
                            _errorQueue.Enqueue(url);
                        }
                    }
                }
                yield return null;
            }
        }

        private string ConvertUrlToPath(string url)
        {
            return url.Replace(PandoraSettings.GetFileProtocolToken(), "");
        }

        protected AssetBundleRequest GetAssetBundleRequest(AssetBundle bundle)
        {
#if UNITY_2017_1_OR_NEWER
            return bundle.LoadAllAssetsAsync();
#else
            return null;
#endif
        }

        protected bool HandleLoadedContent(string url, AssetBundle bundle, AssetBundleRequest request)
        {
            bool result = false;
            Logger.Log("<color=#008888>资源从本地加载到内存成功</color>, url: " + url, PandoraSettings.SDKTag);
            AssetConfig config = _assetConfigDict[url];
            switch(config.type)
            {
                case AssetType.Lua:
                    result = AssetPool.ParseLuaAsset(url, bundle, request);
                    break;
                case AssetType.Prefab:
                    result = AssetPool.ParsePrefabAsset(url, bundle, request, config.isCacheInMemory);
                    break;
                case AssetType.Assetbundle:
                    result = AssetPool.ParseAssetBundle(url, bundle);
                    break;
				case AssetType.Binary:
					result = AssetPool.ParseBinAsset(url, null, bundle, request);
					break;
			}
            return result;
        }

        protected override void HandleError(string url)
        {
            Logger.LogError("将资源加载到内存失败，且超过最大重试次数, url: " + url, PandoraSettings.SDKTag);
        }

    }
}
#endif