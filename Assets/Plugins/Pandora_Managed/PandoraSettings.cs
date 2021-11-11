using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace com.tencent.pandora
{
    public class PandoraSettings
    {
        //SDK 版本号，SDK维护同学发布新版SDK时才改变
        private static string _sdkVersion = "1.0";
        private static PlatformType _loginPlatform = PlatformType.InternalPlatform;
#if UNITY_EDITOR
        public static int DEFAULT_LOG_LEVEL = Logger.DEBUG; //给游戏输出SDK包时替换为Logger.INFO，减少Log对项目组的干扰
#else
        public static int DEFAULT_LOG_LEVEL = Logger.DEBUG;
#endif
        private static bool _isDebug = true;
        private static bool _useZipResForPixUI = true;
        private static CGIVersion _cgiVersion = CGIVersion.Internal;
        public static string SDKTag
        {
            get
            {
                return "SDK";
            }
        }        
        public static bool IsDebug
        {
            get
            {
                return _isDebug;
            }
            set
            {
                _isDebug = value;
            }
        }

        public static bool UseHttps { get; set; }

        //可以在测试环境手动指定平台
        public static RuntimePlatform Platform
        {
            get
            {
#if UNITY_IOS || UNITY_IPHONE
                return RuntimePlatform.IPhonePlayer;
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
                return RuntimePlatform.WindowsPlayer;
#else
                return RuntimePlatform.Android;
#endif
            }
        }

        public static bool IsProductEnvironment
        {
            get;
            set;
        }

        /// <summary>
        /// 可以在Cache/Settings文件夹下读取settings.txt文件获取连接正式环境或测试环境
        /// </summary>
        public static void ReadEnvironmentSetting()
        {
            //从本地settings中读取log设置
            try
            {
                string filePath = Path.Combine(LocalDirectoryHelper.GetSettingsFolderPath(), "settings.txt");
                if (File.Exists(filePath) == true)
                {
                    string content = File.ReadAllText(filePath);
                    Dictionary<string, System.Object> dict = MiniJSON.Json.Deserialize(content) as Dictionary<string, System.Object>;
                    if (dict.ContainsKey("isProductEnvironment") == true)
                    {
                        IsProductEnvironment = (dict["isProductEnvironment"] as string) == "1";
                    }
                }
            }
            catch
            {
                //left blank
            }
        }

        /// <summary>
        /// 获取CGI正式或测试环境地址
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static string GetCgiUrl(string appId)
        {
            string token = "http://";
            if (PandoraSettings.UseHttps == true)
            {
                token = "https://";
            }
            if (IsProductEnvironment == true)
            {
                return token + "pandora.game.qq.com/cgi-bin/api/tplay/" + appId + "_cloud.cgi";
            }
            else
            {
                return token + "pandora.game.qq.com/cgi-bin/api/tplay/cloudtest_v3.cgi";
            }
        }

        /// <summary>
        /// 获取临时文件目录
        /// </summary>
        /// <returns></returns>
        public static string GetTemporaryCachePath()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR
            return Application.dataPath + "/CACHE";
#else
#if UNITY_IOS
                    return Application.temporaryCachePath;
#else
                    return Application.persistentDataPath;
#endif
#endif
        }

        public static bool UseHttpDns
        {
            get;
            set;
        }

        /// <summary>
        /// 各个平台上file协议开头不一样
        /// </summary>
        /// <returns></returns>
        public static string GetFileProtocolToken()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR //添加后，BuildSetting为Android时，也可加载assetbundle
            return "file:///";
#elif UNITY_IOS || UNITY_EDITOR_OSX || UNITY_ANDROID
            return "file://";
#endif
        }

        /// <summary>
        /// 是否从本地StreamingAssets目录下加载资源
        /// </summary>
        public static bool UseStreamingAssets
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsAndroidPlatform
        {
            get
            {
                return Application.platform == RuntimePlatform.Android;
            }
        }

        public static bool IsIOSPlatform
        {
            get
            {
                return Application.platform == RuntimePlatform.IPhonePlayer;
            }
        }

        public static bool IsWindowsPlatform
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsPlayer;
            }
        }

        public static string GetPlatformDescription()
        {
            if (PandoraSettings.Platform == RuntimePlatform.IPhonePlayer)
            {
                return "ios";
            }
            if (PandoraSettings.Platform == RuntimePlatform.WindowsPlayer)
            {
                return "pc";
            }
            return "android";
        }

        public static string SDKVersion
        {
            get
            {
                return _sdkVersion;
            }
        }

        /// <summary>
        /// 是否使用Post的方式访问云端CGI
        /// </summary>
        public static bool RequestCgiByPost { set; get; }

        /// <summary>
        /// 是否暂停Pandora资源加载
        /// </summary>
        public static bool PauseDownloading { set; get; }
        public static bool PauseSocketSending { set; get; }
        public static PlatformType LoginPlatform
        {
            get
            {
                return _loginPlatform;
            }
            set
            {
                _loginPlatform = value;
            }
        }
        /// <summary>
        /// 有些游戏使用线性颜色空间(LinearColorSpace)，这种情况下，Pandora加载的外部图片需要做相应调整
        /// </summary>
        public static bool UseLinearColorSpace { set; get; }
        /// <summary>
        /// 设置PixUI使用的资源格式
        /// </summary>
        public static bool UseZipResForPixUI
        {
            get
            {
                return _useZipResForPixUI;
            }
            set
            {
                _useZipResForPixUI = value;
            }
        }

        public static CGIVersion RequestCGIVersion
        {
            get { return _cgiVersion; }
            set
            {
                _cgiVersion = value;
            }

        }
    }
    public enum PlatformType
    {
        InternalPlatform = 0, //内部平台
        OpenPlatformWithWebsocket = 1, //使用Websocket连接的开放平台
        OpenPlatformWithSocket = 2, //使用Socket连接的开放平台
    }

    public enum CGIVersion
    {
        Internal = 0, //老版本cgi
        Open = 1, //开放平台版本cgi
        Uniform = 2, //精细化和开放平台统一版本cgi
    }
}
