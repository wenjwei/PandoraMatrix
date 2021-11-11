
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace PPMUtils
{
    public class LogHelper
    {
        public delegate void LogoutDelegate(object message, UnityEngine.Object context = null);

        public static LogoutDelegate Log;
        public static LogoutDelegate LogWarning;
        public static LogoutDelegate LogError;

        public static void Init(LogoutDelegate log, LogoutDelegate logWarning, LogoutDelegate logError)
        {
            Log += log;
            LogWarning += logWarning;
            LogError += logError;
        }

        public static void Dispose()
        {
            Log = null;
            LogWarning = null;
            LogError = null;
        }
    }

    public class Requestor
    {
        public static Action<string> LoginCallback;

        public class ElecCookie
        {
            public string bk_uid;
            public string bk_ticket;
        }

        static T ParseToData<T>(string strData)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(strData);
            }
            catch (System.Exception e)
            {
                LogHelper.Log("[PPMUtils] Parse Exception caught: " + e.GetType() + ", string:" + strData);
                return default(T);
            }
        }

        static void ResponseCheck<T>(string strJson, Action<T> retCallback)
        {
            T retObj = default(T);
            if (string.IsNullOrEmpty(strJson) == false)
            {
                ResponseStatus rs = ParseToData<ResponseStatus>(strJson);
                if (rs.ret == 0)
                {
                    LogHelper.Log(string.Format("[PPMUtils] {0} response: {1}", typeof(T).Name, strJson));
                    retObj = ParseToData<T>(rs.content);
                }
                else if (rs.ret == 1004)
                {
#if UNITY_2019_1_OR_NEWER
                    string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        path = Directory.GetParent(path).ToString();
                    }

                    string electCookies = string.Format("{0}/elec-ppm.cookies", path);
                    if (File.Exists(electCookies))
                    {
                        var cookies = File.ReadAllText(electCookies);
                        var elecCookie = JsonConvert.DeserializeObject<ElecCookie>(cookies);
                        LogHelper.Log("读取elec-ppm设置的cookie: " + cookies);
                        File.Delete(electCookies);

                        SetCookie(elecCookie.bk_uid, elecCookie.bk_ticket, (retStatus) => LoginCallback(elecCookie.bk_uid));
                    }
                    else
                    {
                        if (!EditorUtility.DisplayDialog("登录提示",
                            "Unity2019以上版本不支持内置web View登录，请使用单独的登录程序", "我已下载登录程序，现在去登录", "跳转到登录程序下载地址"))
                        {
                            Application.OpenURL("https://git.code.oa.com/jonsu/ppmlogin");
                        }
                    }
#else
                    LogHelper.LogError(string.Format("[PPMUtils] {0}", rs.msg));
                    LoginWebWindow.Load(LoginCallback);
#endif
                }
                else
                {
                    LogHelper.LogError(string.Format("[PPMUtils] {0} response error: {1}", typeof(T).Name, rs.msg));
                }
            }
            else
            {
                LogHelper.LogError(string.Format("[PPMUtils] {0} response Json is empty", typeof(T).Name));
            }

            if (retCallback != null)
            {
                retCallback(retObj);
            }
        }

        public static Thread StartThread(Action threadAction)
        {
            Thread thread = new Thread(() => ThreadRunner(threadAction));
            thread.Start();
            return thread;
        }

        private static void ThreadRunner(Action threadAction)
        {
            if (threadAction != null)
            {
                threadAction();
            }
        }

#region PackageManger

        static public void PackageUpload(string packageName, string packageType, string version, string tag, string description,
                string dependence, string uploadFile, string changeLog, Action<ResponseStatus> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.PackageUpload(packageName, packageType, version, tag, description,
            dependence, uploadFile, changeLog, (strJson) => funCall(ParseToData<ResponseStatus>(strJson)));
            };
            StartThread(threadAction);
        }


        static public void PackagePublish(string packageName, string version, int state, Action<ResponseStatus> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.PackagePublish(packageName, version,
                    state, (strJson) => funCall(ParseToData<ResponseStatus>(strJson)));
            };
            StartThread(threadAction);
        }


        static public void PackageDelete(string packageName, string packageType, string version, string location, Action<ResponseStatus> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.PackageDelete(packageName, packageType, version,
                    location, (strJson) => funCall(ParseToData<ResponseStatus>(strJson)));
            };
            StartThread(threadAction);
        }


        static public void PackageDownload(string packageName, string engineType, string version, Action<ResponseStatus> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.PackageDownload(packageName, engineType, version,
                    (strJson) => funCall(ParseToData<ResponseStatus>(strJson)));
            };
            StartThread(threadAction);
        }

        static public void GetPackageList(string sortType, string packageTag,
            int pageIndex, int onePageCount, Action<DataPackageList> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.GetPackageList(sortType, packageTag, pageIndex,
                    onePageCount, (strJson) => ResponseCheck(strJson, funCall));
            };

            StartThread(threadAction);
        }

        static public void GetOwnPackages(string type, Action<DataOwnPackages> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.GetOwnPackages(type, (strJson) => ResponseCheck(strJson, funCall));
            };

            StartThread(threadAction);
        }

        static public void GetPackageDetail(string packageName, Action<DataPackageDetail> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.GetPackageDetail(packageName, (strJson) => ResponseCheck(strJson, funCall));
            };

            StartThread(threadAction);
        }

#endregion

        static public void SetServerPath(string server, Action<string> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.SetServerPath(server, (strJson) => ResponseCheck(strJson, funCall));
            };

            StartThread(threadAction);
        }

        static public void GetServerPath(Action<ServerInfo> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.GetServerPath((strJson) => ResponseCheck(strJson, funCall));
            };

            StartThread(threadAction);
        }

#region User

        public static void SetCookie(string bkUID, string bkTicket, Action<ResponseStatus> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.SetCookie(bkUID, bkTicket, (strJson) => funCall(ParseToData<ResponseStatus>(strJson)));
            };
            StartThread(threadAction);
        }

        public static void GetCookieUID(Action<string> funCall)
        {
            Action threadAction = delegate ()
            {
                LibPackageInterface.GetCookieUID((strJson) => funCall(ParseToData<ResponseStatus>(strJson).msg));
            };
            StartThread(threadAction);
        }

#endregion
    }
}