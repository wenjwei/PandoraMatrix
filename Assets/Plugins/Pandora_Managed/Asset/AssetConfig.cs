using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

 namespace com.tencent.pandora
{
    public class AssetConfig
    {
        public AssetType type;
        public bool isCacheInMemory;
        public bool isProgramAsset;
        public bool isAssetBundle; //AssetBundle资源需要使用BundleLoader从本地加载到内存
        public string url;

		public AssetConfig(AssetType type, bool isCacheInMemory, bool isProgramAsset, bool isAssetBundle, string url)
		{
			Init(type, isCacheInMemory, isProgramAsset, isAssetBundle, url);
		}

		private void Init(AssetType type, bool isCacheInMemory, bool isProgramAsset, bool isAssetBundle, string url)
		{
			this.type = type;
			this.isCacheInMemory = isCacheInMemory;
			this.isProgramAsset = isProgramAsset;
			this.isAssetBundle = isAssetBundle;
			this.url = url;
		}

        public bool IsInPool
        {
            get
            {
                return AssetPool.HasAsset(GetPoolKey());
            }
        }

        public bool IsInCache
        {
            get
            {
                return AssetManager.IsAssetCached(this.url, this.isProgramAsset);
            }
        }

        public string GetPoolKey()
        {
			string poolKey = AssetManager.GetAssetPoolKey(this.url, this.isProgramAsset);
			return poolKey;
        }

        public string GetDownloadUrl()
        {
            if(this.IsInCache == true)
            {
                return AssetManager.GetCachedAssetUrl(this.url, this.isProgramAsset);
            }
            return this.url;
        }
    }
}
