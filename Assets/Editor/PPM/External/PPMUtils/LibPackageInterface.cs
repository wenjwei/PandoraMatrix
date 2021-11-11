using AOT;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;

namespace PPMUtils
{
    public class LibPackageInterface
    {
        private delegate void CallbackDelegate(int nMethodId, [MarshalAs(UnmanagedType.LPStr)]string strMsg);

        [DllImport("ppmutils", EntryPoint = "LC_GlobalInit")]
        extern static void LC_GlobalInit(CallbackDelegate callback);

        [DllImport("ppmutils", EntryPoint = "CallPPM")]
        extern static void CallPPM(int callId, string content);

        private static void CallToServer(string content, Action<string> pFunCall)
        {
            int nFunId = NetFunId();
            s_mapCallFun.Add(nFunId, pFunCall);

            LogHelper.Log("[PPMUtils] callPPM: " + content);
            CallPPM(nFunId, content);
        }

        private class FunCallInfo
        {
            public FunCallInfo(int nFunid, string strJsonRet)
            {
                this.nFunid = nFunid;
                this.strJsonRet = strJsonRet;
            }

            public int nFunid { get; set; }
            public string strJsonRet { get; set; }
        }

        [MonoPInvokeCallback(typeof(CallbackDelegate))]
        private static void DllFunCall(int nMethodId, [MarshalAs(UnmanagedType.LPStr)]string strMsg)
        {
            LogHelper.Log(string.Format("[PPMUtils] {0} DllFunCall: {1}", nMethodId.ToString(), strMsg));
            FunCallInfo callInfor = new FunCallInfo(nMethodId, strMsg);
            lock (s_syncLocker)
            {
                s_queueEvent.Enqueue(callInfor);
            }
        }

        static Queue<FunCallInfo> s_queueEvent = new Queue<FunCallInfo>();
        static object s_syncLocker = new object();
        static int s_nBase = 1;
        static Queue<int> s_queueFreeId = new Queue<int>();
        private static Dictionary<int, Action<string>> s_mapCallFun = new Dictionary<int, Action<string>>();

        const string PackageType = "Unity";
        const string SortField = "createTime";

        private static int NetFunId()
        {
            int nTempId = s_nBase + 1;
            if (s_queueFreeId.Count > 0)
                nTempId = s_queueFreeId.Dequeue();
            else
                s_nBase = nTempId;

            return nTempId;
        }

        private static void FreeId(int nFunId)
        {
            s_queueFreeId.Enqueue(nFunId);
            s_mapCallFun.Remove(nFunId);
        }

        private static void TimeTick()
        {
            lock (s_syncLocker)
            {
                while (s_queueEvent.Count > 0)
                {

                    FunCallInfo callInfor = s_queueEvent.Dequeue();

                    if (s_mapCallFun.ContainsKey(callInfor.nFunid))
                    {
                        s_mapCallFun[callInfor.nFunid].Invoke(callInfor.strJsonRet);
                        FreeId(callInfor.nFunid);
                    }
                    else
                    {
                        LogHelper.LogError("[PPMUtils] TimeTick Not Found id:" + callInfor.nFunid);
                    }
                }
            }
        }

		[UnityEditor.Callbacks.DidReloadScripts]
		public static void Init()
        {
			try
			{
				LC_GlobalInit(DllFunCall);

				EditorApplication.update -= TimeTick;
				EditorApplication.update += TimeTick;
			}
			catch
			{
				UnityEngine.Debug.LogError("初次导入动态库文件，请重启Unity后使用");
			}
        }
        #region PackageList
        public static void GetPackageList(string sortType, string packageTag,
            int pageIndex, int onePageCount, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "getAllPackageList");
            jsonText.Add("type", PackageType);
            jsonText.Add("page", pageIndex.ToString());
            jsonText.Add("pageSize", onePageCount.ToString());
            jsonText.Add("orderFiled", SortField);
            jsonText.Add("order", sortType);
            jsonText.Add("tag", packageTag);

            CallToServer(jsonText.ToString(), pFunCall);
        }

        public static void GetOwnPackages(string type, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "getOwnPackageList");
            jsonText.Add("type", type);

            CallToServer(jsonText.ToString(), pFunCall);
        }

        public static void GetPackageDetail(string packageName, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "getSpecifiedPackage");
            jsonText.Add("packageName", packageName);
            jsonText.Add("type", "Unity");

            CallToServer(jsonText.ToString(), pFunCall);
        }
        #endregion

        #region Package
        // PackagePublish, state, 1/0，发布/撤销
        public static void PackagePublish(string packageName, string version, int state, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "publish");
            jsonText.Add("packagename", packageName);
            jsonText.Add("packageType", "Unity");
            jsonText.Add("version", version);
            jsonText.Add("state", state.ToString());

            CallToServer(jsonText.ToString(), pFunCall);
        }

        public static void PackageUpload(string packageName, string packageType, string version, string tag, string description,
            string dependence, string uploadFile, string changeLog, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "upload");
            jsonText.Add("packagename", packageName);
            jsonText.Add("packageType", packageType);
            jsonText.Add("version", version);
            jsonText.Add("tags", tag);
            jsonText.Add("description", description);
            jsonText.Add("dependence", dependence);
            jsonText.Add("uploadFile", uploadFile);
            jsonText.Add("changeLog", changeLog);

            CallToServer(jsonText.ToString(), pFunCall);
        }


        public static void PackageDelete(string packageName, string packageType, string version, string location, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "delete");
            jsonText.Add("packagename", packageName);
            jsonText.Add("packageType", packageType);
            jsonText.Add("version", version);
            jsonText.Add("location", location);

            CallToServer(jsonText.ToString(), pFunCall);
        }

        public static void PackageDownload(string packageName, string type, string version, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "download");
            jsonText.Add("packageName", packageName);
            jsonText.Add("engineType", type);
            jsonText.Add("version", version);

            CallToServer(jsonText.ToString(), pFunCall);
        }
        #endregion

        #region Server
        public static void SetServerPath(string serverPath, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "setServerPath");
            jsonText.Add("server", serverPath);

            CallToServer(jsonText.ToString(), pFunCall);
        }

        public static void GetServerPath(Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "getServerPath");

            CallToServer(jsonText.ToString(), pFunCall);
        }

        #endregion

        #region User

        public static void SetCookie(string bkUID, string bkTicket, Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "setCookie");
            jsonText.Add("bk_uid", bkUID);
            jsonText.Add("bk_ticket", bkTicket);

            CallToServer(jsonText.ToString(), pFunCall);
        }

        public static void GetCookieUID(Action<string> pFunCall)
        {
            JObject jsonText = new JObject();
            jsonText.Add("method", "getCookieUID");

            CallToServer(jsonText.ToString(), pFunCall);
        }

        #endregion
    }
}
