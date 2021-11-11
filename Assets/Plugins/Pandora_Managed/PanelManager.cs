using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.tencent.pandora
{
    public class PanelManager
    {
        public const int SUCCESS = 0;
        public const int ERROR_ASSET_NOT_EXISTS = -1;
        public const int ERROR_NO_PARENT = -2;

        //TODO:是否允许存在同一个面板资源的多个实例还需要商榷
        public const int ERROR_ALREADY_EXISTS = -3;
        /// <summary>
        /// 面板父节点字典
        /// Key为面板的名称，Value为父节点对象
        /// </summary>
        private static Dictionary<string, GameObject> _panelParentDict;
        /// <summary>
        /// 面板字典
        /// Key为面板名称，Value为面板对象
        /// </summary>
        private static Dictionary<string, GameObject> _panelDict;

        public static void Initialize()
        {
            _panelParentDict = new Dictionary<string, GameObject>();
            _panelDict = new Dictionary<string, GameObject>();
        }

        public static void SetPanelParent(string name, GameObject parent)
        {
            if(_panelParentDict.ContainsKey(name) == false)
            {
                _panelParentDict.Add(name, parent);
            }
            else
            {
                _panelParentDict[name] = parent;
            }
        }

        public static GameObject GetPanel(string name)
        {
            if(_panelDict.ContainsKey(name) == false)
            {
                return null;
            }
            return _panelDict[name];
        }

        //TODO:后续考虑加入加载完Prefab后，实例化Go的时候，指定新的名字
        public static void CreatePanel(string name, Action<int> callback)
        {
            string fullName = GetPanelAssetFullName(name);
            RemoteConfig remoteConfig = DelegateAggregator.GetRemoteConfig();
            if(remoteConfig != null)
            {
                RemoteConfig.AssetInfo assetInfo = remoteConfig.GetAssetInfo(fullName);
                if (assetInfo != null)
                {
                    AssetManager.GetPanelGameObject(assetInfo, delegate (GameObject go) { OnGetGameObject(go, callback, name); });
                }
            }
        }

        public static void OnGetGameObject(GameObject go, Action<int> callback,string assetName)
        {
            go.name = go.name.Replace("_copy(Clone)", "");
            string name = go.name;
            Transform parentTrans = null;
            if (_panelDict.ContainsKey(assetName) == true)
            {
                string error = "已经存在同名面板。 " + assetName;
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error);
                Destroy(go);
                callback(ERROR_ALREADY_EXISTS);
                return;
            }

            if (_panelParentDict.ContainsKey(assetName) == true)
            {
                if (_panelParentDict[assetName] == null)
                {
                    string error = "面板配置的父节点已经不存在: " + assetName;
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.ReportError(error);
                    Destroy(go);
                    callback(ERROR_NO_PARENT);
                    return;
                }
                else
                {
                    parentTrans = _panelParentDict[assetName].transform;
                }
            }
            if(parentTrans == null)
            {
                parentTrans = DelegateAggregator.GetPanelRoot().transform;
            }
            go.transform.SetParent(parentTrans);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(true);
            _panelDict.Add(assetName, go);
            callback(SUCCESS);
        }

        /// <summary>
        /// 显示玩家头像资源，注意图片格式可能是GIF格式，当头像格式为GIF时，显示第一帧图片
        /// </summary>
        public static void ShowPortrait(string panelName, string url, GameObject go, bool isCacheInMemory, Action<int> callback)
        {
            if (_panelDict.ContainsKey(panelName) == false)
            {
                Logger.LogError("面板不存在, " + panelName, PandoraSettings.SDKTag);
                callback(-1);
                return;
            }

            if (go == null)
            {
                Logger.LogError("面板中指定的go不存在, " + panelName, PandoraSettings.SDKTag);
                callback(-3);
                return;
            }

            AssetManager.GetPortrait(url, isCacheInMemory, delegate (Texture2D texture) { OnGetImage(go, texture, url, callback); });
        }

        public static void ShowImage(string panelName, string url, GameObject go, bool isCacheInMemory, Action<int> callback)
        {
            if (_panelDict.ContainsKey(panelName) == false)
            {
                Logger.LogError("面板不存在, " + panelName, PandoraSettings.SDKTag);
                callback(-1);
                return;
            }

            if(go == null)
            {
                Logger.LogError("面板中指定的go不存在, " + panelName, PandoraSettings.SDKTag);
                callback(-3);
                return;
            }

            AssetManager.GetImage(url, isCacheInMemory, delegate (Texture2D texture) { OnGetImage(go, texture, url, callback); });
        }

        private static void OnGetImage(GameObject go, Texture2D texture, string url, Action<int> callback)
        {
            bool reslut = DelegateAggregator.ApplyPanelTexture(go, texture);
            if(reslut == false)
            {
                Logger.LogError("应用Texture失败, " + url, PandoraSettings.SDKTag);
                callback(-4);
                return;
            }
            callback(0);
        }

        public static void Hide(string name)
        {
            if(_panelDict.ContainsKey(name))
            {
                GameObject go = _panelDict[name];
                if(go != null)
                {
                    go.SetActive(false);
                }
            }
        }

        public static void HideAll()
        {
            var enumerator = _panelDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var kvp = enumerator.Current;
                Hide(kvp.Key);
            }
        }

        public static void Destroy(string name)
        {
            if (_panelDict.ContainsKey(name))
            {
                GameObject go = _panelDict[name];
                if (go != null)
                {
                    Destroy(go);
                }
                _panelDict.Remove(name);
            }
        }

        private static void Destroy(GameObject go)
        {
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
        }

        public static void DestroyAll()
        {
            List<string> nameList = new List<string>(_panelDict.Keys);
            for(int i = 0; i< nameList.Count; i++)
            {
                Destroy(nameList[i]);
            }
            _panelParentDict.Clear();
        }

        private static string GetPanelAssetFullName(string name)
        {
            return PandoraSettings.GetPlatformDescription() + "_" + name.ToLower() + ".assetbundle";
        }

        private static RemoteConfig.AssetInfo GetPanelAssetAssetInfo(string name)
        {
            string fullName = GetPanelAssetFullName(name);
            RemoteConfig remoteConfig = DelegateAggregator.GetRemoteConfig();
            if(remoteConfig == null)
            {
                return null;
            }
            RemoteConfig.AssetInfo assetInfo = remoteConfig.GetAssetInfo(fullName);
            return assetInfo;
        }
    }
}
