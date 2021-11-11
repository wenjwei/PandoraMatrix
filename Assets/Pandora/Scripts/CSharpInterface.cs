using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace com.tencent.pandora
{
    public partial class CSharpInterface
    {
        public static UInt64 NowMilliseconds()
        {
            return TimeHelper.NowMilliseconds();
        }

        public static void UnloadAssets(UnityEngine.Object obj)
        {
            Resources.UnloadAsset(obj);
        }

        public static GameObject LoadGameObjectByResources(string path)
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }
            return GameObject.Instantiate(prefab) as GameObject;
        }

        public static void PlaySound(string id)
        {
            Pandora.Instance.PlaySound(id);
        }

        public static void Report(string content, int id, int type)
        {
            Pandora.Instance.Report(content, id, type);
        }

        public static void ReportError(string error, int id)
        {

            Pandora.Instance.ReportError(error, id);
        }

        /// <summary>
        /// 开放给Lua层直接上报事件处理报错
        /// </summary>
        /// <param name="error"></param>
        public static void ReportLuaError(string error)
        {
            LuaStateManager.ReportLuaError(error);
        }

        public static GameObject CloneAndAddToParent(GameObject template, string name, GameObject parent)
        {
            if (template == null)
            {
                Logger.LogError("源节点不存在", PandoraSettings.SDKTag);
                return null;
            }

            GameObject item = GameObject.Instantiate(template) as GameObject;
            item.name = name;
            SetParent(item, parent);
            return item;
        }

        public static void SetParent(GameObject child, GameObject parent)
        {
            if (child == null)
            {
                Logger.LogError("源节点不存在", PandoraSettings.SDKTag);
                return;
            }

            if (parent == null)
            {
                Logger.LogError("父节点不存在", PandoraSettings.SDKTag);
                return;
            }
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
        }

        public static void SetPanelParent(string panelName, GameObject panelParent)
        {
            Pandora.Instance.SetPanelParent(panelName, panelParent);
        }

        public static bool IsDebug()
        {
            return Pandora.Instance.IsDebug;
        }

        public static bool IsIOSPlatform
        {
            get
            {
                return PandoraSettings.IsIOSPlatform;
            }
        }

        public static string GetPlatformDescription()
        {
            return PandoraSettings.GetPlatformDescription();
        }

        public static string GetSDKVersion()
        {
            return Pandora.Instance.GetUserData().sSdkVersion;
        }

        /// <summary>
        /// Resources.UnloadUnusedAssets();该接口只应该由游戏方调用
        /// Pandora慎用该接口，可能导致游戏卡顿，在确认必要性很高时才能使用该接口
        /// </summary>
        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        public static bool PauseDownloading
        {
            get
            {
                return Pandora.Instance.PauseDownloading;
            }
            set
            {
                Pandora.Instance.PauseDownloading = value;
            }
        }

        public static bool PauseSocketSending
        {
            get
            {
                return Pandora.Instance.PauseSocketSending;
            }
            set
            {
                Pandora.Instance.PauseSocketSending = value;
            }
        }

        public static bool WriteCookie(string fileName, string content)
        {
            return CookieHelper.Write(fileName, content);
        }

        public static string ReadCookie(string fileName)
        {
            return CookieHelper.Read(fileName);
        }

        public static UserData GetUserData()
        {
            return Pandora.Instance.GetUserData();
        }

        public static void RefreshUserDataTokens()
        {
            Pandora.Instance.RefreshUserDataTokens();
        }

        public static RemoteConfig GetRemoteConfig()
        {
            return Pandora.Instance.GetRemoteConfig();
        }

        public static void ShowPortrait(string panelName, string url, GameObject go, bool isCacheInMemory, uint callId)
        {
            PanelManager.ShowPortrait(panelName, url, go, isCacheInMemory, delegate (int returnCode) { OnShowImage(panelName, url, callId, returnCode); });
        }

        public static void ShowImage(string panelName, string url, GameObject go, bool isCacheInMemory, uint callId)
        {
            PanelManager.ShowImage(panelName, url, go, isCacheInMemory, delegate (int returnCode) { OnShowImage(panelName, url, callId, returnCode); });
        }

        private static void OnShowImage(string panelName, string url, uint callId, int returnCode)
        {
            Dictionary<string, System.Object> dict = new Dictionary<string, System.Object>();
            dict["PanelName"] = panelName;
            dict["Url"] = url;
            dict["RetCode"] = returnCode.ToString();
            ExecuteLuaCallback(callId, dict);
        }

        public static void CacheImage(string url)
        {
            AssetManager.CacheAsset(url, null);
        }

        public static bool IsImageCached(string url)
        {
            return AssetManager.IsAssetCached(url, false);
        }

        public static void LoadText(string url, uint callId)
        {
            AssetManager.GetText(url, delegate (string text) { OnLoadText(text, callId); });
        }

        private static void OnLoadText(string text, uint callId)
        {
            ExecuteLuaCallback(callId, text);
        }

        public static void LoadAssetBundle(string url, uint callId)
        {
            AssetManager.GetAssetBundle(url, delegate (AssetBundle assetBundle) { OnLoadAsset(url, assetBundle, callId); });
        }

        public static void LoadGameObject(string url, bool isCacheInMemory, LuaFunction callback)
        {
            AssetManager.GetGameObject(url, isCacheInMemory, delegate (GameObject gameObject) { OnLoadGameObject(gameObject, callback); });
        }

        private static void OnLoadGameObject(GameObject go, LuaFunction callback)
        {
            object o = callback.call(go);
            LuaStateManager.Release(o);
        }

        public static void LoadImage(string url, bool isCacheInMemory, uint callId)
        {
            AssetManager.GetImage(url, isCacheInMemory, delegate (Texture2D texture) { OnLoadAsset(url, texture, callId); });
        }

        private static void OnLoadAsset(string url, UnityEngine.Object obj, uint callId)
        {
            Dictionary<string, System.Object> dict = new Dictionary<string, System.Object>();
            dict["Type"] = (obj != null) ? obj.GetType().ToString() : "";
            dict["Url"] = url;
            dict["RetCode"] = (obj != null) ? 0 : 1;
            ExecuteLuaCallback(callId, dict);
        }

        public static System.Object GetAsset(string url)
        {
            return AssetManager.GetAsset(url);
        }

        public static void CacheAsset(string url, LuaFunction callback)
        {
            AssetManager.CacheAsset(url, delegate (string retUrl) { OnAssetCached(retUrl, callback); });
        }

        private static void OnAssetCached(string url, LuaFunction callback)
        {
            if (callback != null)
            {
                object o = callback.call(AssetManager.GetCachedAssetPath(url), AssetManager.IsAssetCached(url));
                LuaStateManager.Release(o);
            }
        }

        public static bool IsAssetCached(string url)
        {
            return AssetManager.IsAssetCached(url);
        }

        public static void DeleteCacheAsset(string url)
        {
            AssetManager.DeleteCachedAsset(url);
        }

        public static void ReleaseAsset(string url)
        {
            AssetManager.ReleaseAsset(url);
        }

        public static void ForceDeleteZeroReferenceAsset()
        {
            AssetManager.DeleteZeroReferenceAsset(true);
        }

        /// <summary>
        /// iOS强制需要使用https
        /// requestJson应该是一个Dictionary序列化后的字符串
        /// 可能需要根据业务的具体情况调整WwwLoader中的封包细节
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestJson"></param>
        /// <param name="isPost"></param>
        /// <param name="callId"></param>
        public static void LoadWww(string url, string requestJson, bool isPost, uint callId)
        {
            AssetManager.LoadWww(url, requestJson, isPost, delegate (string response) { OnLoadWww(response, callId); });
        }

        public static void LoadWwwWithBinaryData(string url, string requestJson, Byte[] binaryData, bool isPost, uint callId)
        {
            AssetManager.LoadWww(url, requestJson, binaryData, isPost, delegate (string response) { OnLoadWww(response, callId); });
        }

        private static void OnLoadWww(string response, uint callId)
        {
            Dictionary<string, System.Object> result = new Dictionary<string, System.Object>();
            result["Resp"] = response;
            result["RetCode"] = string.IsNullOrEmpty(response) ? "-1" : "0";
            ExecuteLuaCallback(callId, result);
        }

        public static void CreatePanel(uint callId, string panelName)
        {
            PanelManager.CreatePanel(panelName, delegate (int returnCode) { OnCreatePanel(panelName, callId, returnCode); });
        }

        private static void OnCreatePanel(string panelName, uint callId, int returnCode)
        {
            Dictionary<string, System.Object> result = new Dictionary<string, System.Object>();
            result["PanelName"] = panelName;
            result["RetCode"] = returnCode.ToString();
            ExecuteLuaCallback(callId, result);
        }

        public static GameObject GetPanel(string panelName)
        {
            return PanelManager.GetPanel(panelName);
        }

        public static void HidePanel(string panelName)
        {
            PanelManager.Hide(panelName);
        }

        public static void HideAllPanel()
        {
            PanelManager.HideAll();
        }

        public static void DestroyPanel(string panelName)
        {
            PanelManager.Destroy(panelName);
        }

        public static void DestroyAllPanel()
        {
            PanelManager.DestroyAll();
        }

        public static bool GetTotalSwitch()
        {
            return Pandora.Instance.GetRemoteConfig().totalSwitch;
        }

        public static bool GetFunctionSwitch(string functionName)
        {
            return Pandora.Instance.GetRemoteConfig().GetFunctionSwitch(functionName);
        }

        /// <summary>
        /// Lua发送消息给游戏
        /// </summary>
        /// <param name="jsonStr"></param>
        public static void LuaCallGame(string jsonStr)
        {
            Pandora.Instance.CallGame(jsonStr);
        }

        /// <summary>
        /// 游戏发送消息给Lua
        /// </summary>
        /// <param name="jsonStr"></param>
        internal static void GameCallLua(string jsonStr)
        {
            try
            {
                if (LuaStateManager.IsInitialized == true)
                {
                    object o = LuaStateManager.CallLuaFunction("Common.CommandFromGame", jsonStr);
                    LuaStateManager.Release(o);
                }
            }
            catch (Exception e)
            {
                string error = "Lua执行游戏发送过来的消息失败, " + jsonStr + " " + e.Message;
                Logger.LogError(error, PandoraSettings.SDKTag);
                Pandora.Instance.ReportError(error, ErrorCode.GAME_2_PANDORA_EXCEPTION);
            }
        }

        /// <summary>
        /// callId为Common.lua中生成的回调唯一id
        /// CommandId为Broker命令字
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="jsonStr"></param>
        /// <param name="commandId"></param>
        public static void CallBroker(uint callId, string jsonStr, int commandId)
        {
            Pandora.Instance.BrokerSocket.Send(callId, jsonStr, commandId);
        }

        /// <summary>
        /// 经分调整为走AtmSocket上报
        /// AtmSocket上报的特点是不会有回调过程
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <param name="commandId"></param>
        public static void CallAtm(string jsonStr, int commandId)
        {
            Pandora.Instance.AtmSocket.Send(jsonStr, commandId);
        }

        internal static void ExecuteLuaCallback(uint callId, Dictionary<string, System.Object> result)
        {
            string jsonStr = MiniJSON.Json.Serialize(result);
            ExecuteLuaCallback(callId, jsonStr);
        }

        internal static void ExecuteLuaCallback(uint callId, string jsonStr)
        {
            try
            {
                if (Pandora.Instance.IsDebug == true)
                {
                    Logger.Log("回调Lua, callId " + callId + ": ", PandoraSettings.SDKTag);
                    Logger.Log(jsonStr, PandoraSettings.SDKTag);
                }
                if (LuaStateManager.IsInitialized == true)
                {
                    object o = LuaStateManager.CallLuaFunction("Common.ExecuteCallback", callId, jsonStr);
                    LuaStateManager.Release(o);
                }
            }
            catch (Exception e)
            {
                string error = "回调Lua出错了, jsonStr: " + jsonStr + " " + e.Message;
                Logger.LogError(error, PandoraSettings.SDKTag);
                Pandora.Instance.ReportError(error, ErrorCode.EXECUTE_LUA_CALLBACK_EXCEPTION);
            }
        }

        public static void ShowItem(GameObject go, uint itemId, uint itemCount)
        {
            Pandora.Instance.ShowItem(go, itemId, itemCount);
        }

        public static void ShowItemIcon(GameObject go, uint itemId)
        {
            Pandora.Instance.ShowItemIcon(go, itemId);
        }

        public static void ShowItemTips(GameObject go, uint itemId)
        {
            Pandora.Instance.ShowItemTips(go, itemId);
        }

        public static Dictionary<string, string> GetCurrency()
        {
            return Pandora.Instance.GetCurrency();
        }

        public static void Jump(string type, string content)
        {
            Pandora.Instance.Jump(type, content);
        }
    }

}
