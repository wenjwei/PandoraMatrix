using System.Net;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Packet = System.Collections.Generic.Dictionary<string, object>;
using System;

namespace com.tencent.pandora
{
    /// <summary>
    /// 使用该Socket获取RemoteConfig，用后即关
    /// </summary>
    public class RemoteConfigBrokerSocket : BrokerSocket
    {
        public const int C2S_CGI_MATCH = 7000;
        /// <summary>
        /// 为避免登录后过快执行Lua虚拟机优化引起的卡顿，建立延迟执行获取RemoteConfig后的回调函数机制
        /// </summary>
        private const int DELAYED_FRAME_COUNT = 30;
        private Action<RemoteConfig> _loadRemoteConfigCallback;
        //连接10秒后没有收到RemoteConfig的话，则认为失败，进入重连
        private const int TIME_OUT_SPAN = 10;
        private const int MAX_RETRY_COUNT = 5;
        private int _retry = 0;

        private Coroutine _inspectorCoroutine;
        //private Coroutine _delayCoroutine;
        private int _frameCount;
        private float _lastTime;
        private UserData _userData;
        private RemoteConfig _remoteConfig;

        public string Host
        {
            set
            {
                _host = value;
            }
            get
            {
                return _host;
            }
        }

        public int Port
        {
            set
            {
                _port = value;
            }
            get
            {
                return _port;
            }
        }

        public void LoadRemoteConfig(UserData userData, Action<RemoteConfig> callback)
        {
            _userData = userData;
            _retry = 0;
            _lastTime = 0;
            _remoteConfig = null;
            _loadRemoteConfigCallback = callback;
            SafeStartCoroutine(ref _inspectorCoroutine, DaemonInspect());
            LoadRemoteConfig();
        }

        private void LoadRemoteConfig()
        {
            _lastTime = Time.realtimeSinceStartup;
            Connect(_host, _port, null);
        }

        private IEnumerator DaemonInspect()
        {
            while (true)
            {
                if (_remoteConfig != null)
                {
                    yield break;
                }
                if (_lastTime != 0 && (Time.realtimeSinceStartup - _lastTime) >= TIME_OUT_SPAN)
                {
                    _retry++;
                    if (_retry < MAX_RETRY_COUNT)
                    {
                        LoadRemoteConfig();
                    }
                    else
                    {
                        CleanBeforeClose();
                        Close();
                        string error = string.Format("请求Cgi数据失败，且超过最大重试次数, Host:{0}, Address:{1}:{2}, Reason:{3}", _host, _address, _port.ToString(), GetFailedDescription(_failedReason));
                        Logger.LogError(error, PandoraSettings.SDKTag);
                        DelegateAggregator.ReportError(error, ErrorCode.CGI_TIMEOUT);
                        DelegateAggregator.Report(error, ErrorCode.CGI_TIMEOUT_DETAIL, 2);
                        if (PandoraSettings.LoginPlatform == PlatformType.InternalPlatform)
                        {
                            DelegateAggregator.ReportError(error, ErrorCode.CGI_TIMEOUT);
                            if (_loadRemoteConfigCallback != null)
                            {
                                if (PandoraSettings.RequestCGIVersion == CGIVersion.Internal)
                                {
                                    _loadRemoteConfigCallback(new InternalPlatformRemoteConfig("{}"));
                                }
                                if (PandoraSettings.RequestCGIVersion == CGIVersion.Uniform)
                                {
                                    _loadRemoteConfigCallback(new UniformRemoteConfig("{}"));
                                }
                                _loadRemoteConfigCallback = null;
                            }
                        }
                    }
                }
                yield return null;
            }
        }

        protected override void OnConnected()
        {
            string msg = string.Format("RemoteConfigBrokerSocket 连接成功: {0} {1}:{2}", _host, _address, _port.ToString());
            Logger.LogInfo(msg, PandoraSettings.SDKTag);
            if (_userData != null)
            {
                SendGetRemoteConfig();
            }
        }

        private void SendGetRemoteConfig()
        {
            string request = string.Format("{{\"head\":{0}, \"body\":{1}}}", GenerateGetRemoteConfigRequestHeader(), GenerateGetRemoteConfigRequestBody());

            if (PandoraSettings.RequestCGIVersion == CGIVersion.Open)
            {
                //开放平台的在此不处理，需要合入开放平台的sdk处理。
            }
            else
            {
                //uniform和internal版本，外层cmd_id都是7000
                //Send(0, request, 7003);
                Send(0, request, 7000);
            }
        }

        private string GenerateGetRemoteConfigRequestHeader()
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header["seq_id"] = "0";
            header["msg_type"] = "1";
            header["sdk_version"] = _userData.sSdkVersion;
            header["game_app_id"] = _userData.sAppId;
            header["channel_id"] = "";
            header["plat_id"] = _userData.sPlatID;
            header["area_id"] = _userData.sArea;
            header["patition_id"] = _userData.sPartition;
            header["open_id"] = _userData.sOpenId;
            header["role_id"] = _userData.sRoleId;
            header["timestamp"] = TimeHelper.NowMilliseconds().ToString();
            header["access_token"] = _userData.sAccessToken;
            header["acc_type"] = _userData.sAcountType;
            if (PandoraSettings.RequestCGIVersion == CGIVersion.Internal)
            {
                header["cmd_id"] = "10014";
            }
            else if (PandoraSettings.RequestCGIVersion == CGIVersion.Uniform)
            {
                header["cmd_id"] = "7003";
            }
            else
            {
                //开放平台的在此不处理，需要合入开放平台的sdk处理。
            }
            return MiniJSON.Json.Serialize(header);
        }

        private string GenerateGetRemoteConfigRequestBody()
        {
            Dictionary<string, string> body = new Dictionary<string, string>();
            body["luaversion"] = "";
            body["gameappversion"] = _userData.sGameVer;
            return MiniJSON.Json.Serialize(body);
        }

        protected override void OnReceived(byte[] content, int length)
        {
            try
            {
                string json = Miniz.UnCompressToString(content, length);
                if (string.IsNullOrEmpty(json) == true)
                {
                    string error = "收到RemoteConfigBroker的内容为空";
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR, 0);
                    DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR_DETAIL, 2);
                    return;
                }
                Logger.Log("RemoteConfigBroker收到回包: ", PandoraSettings.SDKTag);
                Logger.Log(json, PandoraSettings.SDKTag);
                Dictionary<string, System.Object> outerDict = MiniJSON.Json.Deserialize(json) as Dictionary<string, System.Object>;
                if (outerDict == null || outerDict.ContainsKey("type") == false || outerDict.ContainsKey("cmd_id") == false || outerDict.ContainsKey("seq_id") == false)
                {
                    string error = "收到RemoteConfigBroker错误回包";
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR, 0);
                    DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR_DETAIL, 2);
                    return;
                }

                ParseRemoteConfig(outerDict["body"] as string);
            }
            catch (Exception e)
            {
                string error = string.Format("RemoteConfigBroker处理回包过程中发生异常：{0}|{1}|{2} ", e.GetType().ToString(), e.Message, e.StackTrace);
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR, 0);
                DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR_DETAIL, 2);
            }
        }

        private void ParseRemoteConfig(string response)
        {
            Dictionary<string, object> outerDict = MiniJSON.Json.Deserialize(response) as Dictionary<string, object>;
            if (int.Parse(outerDict["ret"].ToString()) == 0)
            {
                CleanBeforeClose();
                Close();
                Dictionary<string, object> innerDict = MiniJSON.Json.Deserialize((string)outerDict["resp"]) as Dictionary<string, object>;
                byte[] base4EncodedBytes = Convert.FromBase64String(innerDict["body"] as string);
                string content = MsdkTea.Decode(base4EncodedBytes);
                Logger.LogInfo("获得RemoteConfig： " + content, PandoraSettings.SDKTag);

                if (PandoraSettings.LoginPlatform == PlatformType.InternalPlatform)
                {
                    if (PandoraSettings.RequestCGIVersion == CGIVersion.Internal)
                    {
                        _remoteConfig = new InternalPlatformRemoteConfig(content);
                    }

                    if (PandoraSettings.RequestCGIVersion == CGIVersion.Uniform)
                    {
                        Logger.LogInfo("获得RemoteConfig：Uniform");
                        _remoteConfig = new UniformRemoteConfig(content);
                    }
                }

                if (_loadRemoteConfigCallback != null)
                {
                    //生成内部平台的remoteconfig并回调
                    _loadRemoteConfigCallback(_remoteConfig);
                    _loadRemoteConfigCallback = null;
                }
            }
            else
            {
                string error = "收到RemoteConfigBroker错误回包: " + response;
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR, 0);
                DelegateAggregator.Report(error, ErrorCode.CGI_CONTENT_ERROR_DETAIL, 2);
            }
        }

        private IEnumerator DelayExecute(RemoteConfig remoteConfig)
        {
            while (true)
            {
                _frameCount += 1;
                if (_frameCount > DELAYED_FRAME_COUNT)
                {
                    _frameCount = 0;
                    if (_loadRemoteConfigCallback != null)
                    {
                        _loadRemoteConfigCallback(remoteConfig);
                        _loadRemoteConfigCallback = null;
                    }
                    yield break;
                }
                yield return null;
            }
        }

        public override void CleanBeforeClose()
        {
            base.CleanBeforeClose();
            SafeStopCoroutine(ref _inspectorCoroutine);
        }

        private string GetFailedDescription(int failedReason)
        {
            if (failedReason == REASON_DNS_FAILED)
            {
                return "DNS解析失败";
            }
            if (failedReason == REASON_SOCKET_FAILED)
            {
                return "Socket连接失败";
            }
            return "连接超时";
        }

        protected override string GetCmdDesc(int commandId, int type = 1)
        {
            if (commandId == C2S_CGI_MATCH)
            {
                return string.Format("客户端请求, cmd_id = {0} <color=#00ff00>(匹配CGI规则)</color>", commandId.ToString());
            }
            return base.GetCmdDesc(commandId, type);
        }
    }
}

