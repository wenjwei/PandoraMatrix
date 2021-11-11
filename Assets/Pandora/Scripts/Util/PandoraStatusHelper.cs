using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.tencent.pandora
{
    internal class PandoraStatusHelper
    {
        public const string STATUS_PRODUCT_TEMPLATE = "资源尚未准备好，请稍后再来~~";
        public const string STATUS_TEST_TEMPLATE = "资源尚未准备好，请稍后再来~~(Error:{0})";
        public const string STATUS_CGI_FAILED = "CGI Failed";
        public const string STATUS_ASSET_FAILED = "Asset Failed";
        public const string STATUS_ASSET_LOADING = "Asset Loading";
        public const string STATUS_DATA_FAILED = "Data Failed";
        public const string STATUS_DATA_LOADING = "Data Loading";

        //资源加载失败集合
        private HashSet<string> _assetFailedSet;
        //资源加载成功集合
        private HashSet<string> _assetSucceedSet;
        //模块数据ready集合
        private HashSet<string> _groupReadySet;
        //模块数据失败集合
        private HashSet<string> _groupFailedSet;
        private bool _isCgiFailed;

        public PandoraStatusHelper()
        {
            _assetFailedSet = new HashSet<string>();
            _assetSucceedSet = new HashSet<string>();
            _groupReadySet = new HashSet<string>();
            _groupFailedSet = new HashSet<string>();
            _isCgiFailed = false;
        }

        public void OnInternalEvent(Dictionary<string, string> dict)
        {
            string type = string.Empty;
            if (dict.ContainsKey("type") == true)
            {
                type = dict["type"];
            }
            if (type == "pandoraError" && dict["content"] == "cgiFailed")
            {
                _isCgiFailed = true;
                return;
            }
            if (type == "assetLoadStart")
            {
                return;
            }
            if (type == "assetLoadError")
            {
                _assetFailedSet.Add(dict["name"]);
            }
            if (type == "assetLoadComplete")
            {
                _assetFailedSet.Remove(dict["name"]);
                _assetSucceedSet.Add(dict["name"]);
            }
            if (type == "pandoraReady")
            {
                _groupFailedSet.Remove(dict["content"]);
                _groupReadySet.Add(dict["content"]);
            }
            if (type == "pandoraFailed")
            {
                _groupFailedSet.Add(dict["content"]);
            }
            if (type == "assetLoadProgress")
            {
                return;
            }
        }

        public string GetStatus(string groupName)
        {
            //CGI访问失败了或内容为空
            if (_isCgiFailed == true)
            {
                _isCgiFailed = false;
                return STATUS_CGI_FAILED;
            }
            //资源加载失败了
            if (_assetFailedSet.Contains(groupName) == true)
            {
                return STATUS_ASSET_FAILED;
            }
            //资源加载尚未完成
            if (_assetSucceedSet.Contains(groupName) == false)
            {
                return STATUS_ASSET_LOADING;
            }
            //数据加载失败了
            if (_groupFailedSet.Contains(groupName) == true)
            {
                return STATUS_DATA_FAILED;
            }
            //资源加载完成但数据尚未准备好
            if (_groupReadySet.Contains(groupName) == false)
            {
                return STATUS_DATA_LOADING;
            }
            return string.Empty;
        }

        public void Reset()
        {
            _assetFailedSet.Clear();
            _assetSucceedSet.Clear();
            _groupFailedSet.Clear();
            _groupReadySet.Clear();
            _isCgiFailed = false;
        }
    }
}

