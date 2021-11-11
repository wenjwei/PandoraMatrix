using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.tencent.pandora
{
    /// <summary>
    /// AssetLoader 将资源加载到内存
    /// </summary>
    public class AssetLoader : AbstractLoader
    {

        // 每60s清理一次0引用资源
        protected const int DELETE_ZERO_REFERENCE_INTERVAL = 60;
        protected float _lastDeleteTime;

        private Dictionary<string, AssetConfig> _assetConfigDict = new Dictionary<string, AssetConfig>();

        public override void Initialize()
        {
            base.Initialize();
            _concurrentCount = 2;
            _lastDeleteTime = Time.realtimeSinceStartup;
        }

        public void LoadLuaAssetList(List<AssetConfig> configList, Action<List<string>> callback)
        {
            List<string> urlList = new List<string>();
            for (int i = 0; i < configList.Count; i++)
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
            string url = config.GetDownloadUrl();
            _assetConfigDict[url] = config;
            base.LoadAsset(url, callback, 0);
        }

        protected override AssetBundleRequest GetAssetBundleRequest(string url, WwwWrapper wwwWrapper)
        {
#if UNITY_2017_1_OR_NEWER
            AssetConfig config = _assetConfigDict[url];
            if (config.type == AssetType.Prefab || config.type == AssetType.Lua || config.type == AssetType.Assetbundle)
            {
                return wwwWrapper.AssetBundle == null ? null : wwwWrapper.AssetBundle.LoadAllAssetsAsync();
            }
#endif
            return null;
        }

        protected override WwwWrapper CreateWwwWrapper(string url)
        {
            AssetConfig config = _assetConfigDict[url];
            switch (config.type)
            {
                case AssetType.Lua:
                    return WwwWrapper.Get(url, RequestResourceType.AssetBundle);
                case AssetType.Prefab:
                    return WwwWrapper.Get(url, RequestResourceType.AssetBundle);
                case AssetType.Assetbundle:
                    return WwwWrapper.Get(url, RequestResourceType.AssetBundle);
                case AssetType.Image:
                    return WwwWrapper.Get(url, RequestResourceType.Texture);
                case AssetType.Text:
                    return WwwWrapper.Get(url, RequestResourceType.Default);
                case AssetType.RawData:
                    return WwwWrapper.Get(url, RequestResourceType.Default);
                case AssetType.Portrait:
                    return WwwWrapper.Get(url, RequestResourceType.Texture);
                case AssetType.Audio:
                    return WwwWrapper.Get(url, RequestResourceType.AudioClip);
				case AssetType.Binary:
					return WwwWrapper.Get(url, RequestResourceType.AssetBundle);
                case AssetType.Zip:
                    return WwwWrapper.Get(url, RequestResourceType.Default);
				default:
                    return WwwWrapper.Get(url, RequestResourceType.Default);
            }
        }

        protected override bool HandleLoadedContent(string url, WwwWrapper wwwWrapper, AssetBundleRequest request)
        {
            Logger.Log("<color=#00ff00>资源从本地加载到内存成功</color>, url: " + url,PandoraSettings.SDKTag);
            bool result = false;
            AssetConfig config = _assetConfigDict[url];
            switch (config.type)
            {
                case AssetType.Lua:
                    result = AssetPool.ParseLuaAsset(url, wwwWrapper.AssetBundle, request);
                    break;
                case AssetType.Prefab:
                    result = AssetPool.ParsePrefabAsset(url, wwwWrapper.AssetBundle, request, config.isCacheInMemory);
                    break;
                case AssetType.Assetbundle:
                    result = AssetPool.ParseAssetBundle(url, wwwWrapper.AssetBundle);
                    break;
                case AssetType.Image:
                    result = AssetPool.ParseImageAsset(url, wwwWrapper, config.isCacheInMemory);
                    break;
                case AssetType.Text:
                    result = AssetPool.ParseTextAsset(url, wwwWrapper, config.isCacheInMemory);
                    break;
                case AssetType.RawData:
                    result = AssetPool.ParseRawDataAsset(url, wwwWrapper, config.isCacheInMemory);
                    break;
                case AssetType.Portrait:
                    result = AssetPool.ParsePortraitAsset(url, wwwWrapper, config.isCacheInMemory);
                    break;
                case AssetType.Audio:
                    result = AssetPool.ParseAudioAsset(url, wwwWrapper, config.isCacheInMemory);
                    break;
				case AssetType.Binary:
					result = AssetPool.ParseBinAsset(url, wwwWrapper, wwwWrapper.AssetBundle, request);
					break;
                case AssetType.Zip:
                    result = AssetPool.ParseZipAsset(url, wwwWrapper, config.isCacheInMemory);
                    break;
			}
            wwwWrapper.Dispose();
            return result;
        }

        protected override void HandleError(string url)
        {
            Logger.LogError("将资源加载到内存失败，且超过最大重试次数, url: " + url, PandoraSettings.SDKTag);
        }

        protected override void Update()
        {
            base.Update();
            DaemonDeleteZeroReferenceAsset();
        }

        private void DaemonDeleteZeroReferenceAsset()
        {
            float delta = Time.realtimeSinceStartup - _lastDeleteTime;
            if (delta >= DELETE_ZERO_REFERENCE_INTERVAL)
            {
                _lastDeleteTime = Time.realtimeSinceStartup;
                AssetManager.DeleteZeroReferenceAsset();
            }
        }
    }
}
