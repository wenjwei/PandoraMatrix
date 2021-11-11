using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.tencent.pandora
{
    /// <summary>
    /// 由应用层设置这些委托
    /// </summary>
    public class DelegateAggregator
    {
        private static Action<string, int, int> _reportDelegate;
        private static Func<UserData> _getUserDataDelegate;
        private static Func<RemoteConfig> _getRemoteConfigDelegate;
        private static Func<GameObject> _getGameObjectDelegate;
        private static Func<GameObject> _getPanelRootDelegate;
        private static Func<GameObject, Texture2D, bool> _applyTextureDelegate;
        private static Func<GameObject, List<Material>> _getUIMaterialListDelegate;
        private static Func<GameObject, HashSet<Texture>> _getGameObjectTextureSetDelegate;
        private static Action<uint, string> _executeLuaDelegate;
        private static Func<string> _getLogUploadUrlDelegate;


        public static void SetReportDelegate(Action<string, int, int> reportDelegate)
        {
            _reportDelegate = reportDelegate;
        }

        public static void Report(string content, int id, int type)
        {
            _reportDelegate(content, id, type);
        }

        public static void ReportError(string error, int id = 0)
        {
            Report(error, id, 0);
        }

        public static void SetGetUserDataDelegate(Func<UserData> getUserDataDelegate)
        {
            _getUserDataDelegate = getUserDataDelegate;
        }

        public static UserData GetUserData()
        {
            return _getUserDataDelegate();
        }

        public static void SetGetRemoteConfigDelegate(Func<RemoteConfig> getRemoteConfigDelegate)
        {
            _getRemoteConfigDelegate = getRemoteConfigDelegate;
        }

        public static RemoteConfig GetRemoteConfig()
        {
            return _getRemoteConfigDelegate();
        }

        public static void SetGetGameObjectDelegate(Func<GameObject> getGameObjectDelegate)
        {
            _getGameObjectDelegate = getGameObjectDelegate;
        }

        /// <summary>
        /// 该节点是添加各种Pandora组件的节点，如BrokerSocket等
        /// </summary>
        /// <returns></returns>
        public static GameObject GetGameObject()
        {
            return _getGameObjectDelegate();
        }

        public static void SetGetPanelRootDelegate(Func<GameObject> getPanelRootDelegate)
        {
            _getPanelRootDelegate = getPanelRootDelegate;
        }

        /// <summary>
        /// 该节点是Pandora面板默认的父节点
        /// </summary>
        /// <returns></returns>
        public static GameObject GetPanelRoot()
        {
            return _getPanelRootDelegate();
        }

        public static void SetApplyPanelTextureDelegate(Func<GameObject, Texture2D, bool> applyTextureDelegate)
        {
            _applyTextureDelegate = applyTextureDelegate;
        }

        public static bool ApplyPanelTexture(GameObject go, Texture2D texture)
        {
            return _applyTextureDelegate(go, texture);
        }

        public static void SetGetUIMaterialListDelegate(Func<GameObject, List<Material>> getUIMaterialListDelegate)
        {
            _getUIMaterialListDelegate = getUIMaterialListDelegate;
        }

        /// <summary>
        /// 获取UI的材质列表，以在Editor下更换材质Shader
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static List<Material> GetUIMaterialList(GameObject go)
        {
            return _getUIMaterialListDelegate(go);
        }

        public static void SetGetGameObjectTextureSetDelegate(Func<GameObject, HashSet<Texture>> getGameObjectTextureSetDelegate)
        {
            _getGameObjectTextureSetDelegate = getGameObjectTextureSetDelegate;
        }

        /// <summary>
        /// 获取Prefab所依赖的Texture集合
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static HashSet<Texture> GetGameObjectTextureSet(GameObject go)
        {
            return _getGameObjectTextureSetDelegate(go);
        }

        public static void SetExecuteLuaCallbackDelegate(Action<uint, string> executeLuaCallbackDelegate)
        {
            _executeLuaDelegate = executeLuaCallbackDelegate;
        }

        public static void ExecuteLuaCallback(uint callId, string message)
        {
            _executeLuaDelegate(callId, message);
        }

        public static void SetGetLogUploadUrlDelegate(Func<string> getLogUploadUrlDelegate)
        {
            _getLogUploadUrlDelegate = getLogUploadUrlDelegate;
        }

        public static string GetLogUploadUrl()
        {
            return _getLogUploadUrlDelegate();
        }
    }
}
