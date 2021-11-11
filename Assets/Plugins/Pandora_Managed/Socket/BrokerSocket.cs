using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Packet = System.Collections.Generic.Dictionary<string, object>;

namespace com.tencent.pandora
{
    public class BrokerSocket : AbstractTcpClient
    {
        /// <summary>
        /// 服务器主动给客户端推送消息的类型
        /// </summary>
        public const int PUSH = 1;
        /// <summary>
        /// 服务器给客户端请求回包的消息类型
        /// </summary>
        public const int PULL = 2;
        /// <summary>
        /// 心跳包时间间隔
        /// </summary>
        public const int HEARTBEAT_INTERVAL = 30;

        /// <summary>
        /// 发送心跳后5s内没收到回包则认为断网，进入重连状态
        /// </summary>
        public const int HEARTEART_TIMEOUT = 5;

        /// <summary>
        /// 为防止服务器雪崩，在重连状态下，最多重连5次，后续走发消息时重连
        /// </summary>
        private int _retryCount = 0;
        /// <summary>
        /// 登录
        /// </summary>
        public const int C2S_LOGIN = 1001;
        public const int S2C_LOGIN = 1001;

        public const int C2S_HEARTBEAT = 1002;
        public const int S2C_HEARTBEAT = 1002;

        /// <summary>
        /// Push消息
        /// </summary>
        public const int S2C_PUSH = 1003;
        public const int C2S_PUSH = 1003;


        public const int S2C_KICK_OUT = 1004;

        /// <summary>
        /// 拉取活动信息
        /// </summary>
        public const int C2S_ACTION = 9000;
        public const int S2C_ACTION = 9000;

        /// <summary>
        /// 社区功能
        /// </summary>
        public const int C2S_SOCIAL = 9001;
        public const int S2C_SOCIAL = 9001;

        /// <summary>
        /// Feed流功能
        /// </summary>
        public const int C2S_FEED = 9002;
        public const int S2C_FEED = 9002;

        /// <summary>
        /// 经分上报
        /// </summary>
        public const int C2S_STATS = 5001;
        public const int S2C_STATS = 5001;

        private bool _hasHeartbeatResponse;
        private uint _heartbeatCallId = 0;
        private Coroutine _heartbeatCoroutine;
        private float _lastHeartbeatTime;
        private float _lastReconnectTime;

        private Coroutine _reconnectCoroutine;

        protected override void OnConnected()
        {
            UserData userData = DelegateAggregator.GetUserData();
            userData.sBrokerHost = _host;
            userData.sBrokerIp = _ipAddress.ToString();
            userData.sBrokerPort = _port.ToString();
            string msg = string.Format("<color=#00ff00>BrokerSocket 连接成功: {0} {1}:{2}</color>", _host, _address, _port.ToString());
            Logger.LogInfo(msg, PandoraSettings.SDKTag);
            SendLogin();
        }

        //注意：
        //此处的outer是指描述Broker此次请求返回的状态描述，可能出错，属于底层的错误。
        //outer的body字段是描述，活动请求返回的内容，可能含有业务层面上的出错信息，比如没有活动，此部分透传给Lua
        protected override void OnReceived(byte[] content, int length)
        {
            try
            {
                string json = Miniz.UnCompressToString(content, length);
                if (string.IsNullOrEmpty(json) == true)
                {
                    Logger.LogError("收到Broker的返回内容为空或解压失败", PandoraSettings.SDKTag);
                    return;
                }

                Dictionary<string, System.Object> outerDict = MiniJSON.Json.Deserialize(json) as Dictionary<string, System.Object>;
                if (outerDict == null || !outerDict.ContainsKey("type") || !outerDict.ContainsKey("cmd_id") || !outerDict.ContainsKey("seq_id"))
                {
                    Logger.LogError("收到Broker错误回包", PandoraSettings.SDKTag);
                    return;
                }
                int type = (int)(long)outerDict["type"];
                int commandId = (int)(long)outerDict["cmd_id"];
                uint callId = (uint)(long)outerDict["seq_id"];
                Logger.Log(GetCmdDesc(commandId, type) + " 　CallId：" + callId.ToString(), PandoraSettings.SDKTag);
                Logger.Log(json, PandoraSettings.SDKTag);
                if (type == PULL)
                {
                    HandlePullMessage(callId, commandId, outerDict["body"] as string);
                }
                else if (type == PUSH)
                {
                    HandlePushMessage(callId, commandId, json, GetPushFeedback(outerDict["body"]));
                }
            }
            catch (Exception e)
            {
                string error = string.Format("Broker处理回包过程中发生异常: {0} {1} {2}", e.GetType().ToString(), e.Message, e.StackTrace);
                Logger.LogError(error, PandoraSettings.SDKTag);
            }
        }

        private string GetPushFeedback(System.Object outerBody)
        {
            string result = string.Empty;
            Dictionary<string, System.Object> bodyDict = outerBody as Dictionary<string, System.Object>;
            if(bodyDict != null)
            {
                result = MiniJSON.Json.Serialize(bodyDict);
            }
            return result;
        }

        private void HandlePullMessage(uint callId, int commandId, string message)
        {
            _hasHeartbeatResponse = true; //收到任何服务器回包都算心跳有返回
            switch (commandId)
            {
                case S2C_LOGIN:
                    EnterHeartbeatState();
                    break;
                case S2C_HEARTBEAT:
                    break;
                case S2C_ACTION:
                    DelegateAggregator.ExecuteLuaCallback(callId, message);
                    break;
                case S2C_SOCIAL:
                    DelegateAggregator.ExecuteLuaCallback(callId, message);
                    break;
                case S2C_FEED:
                    DelegateAggregator.ExecuteLuaCallback(callId, message);
                    break;
            }
        }

        private void HandlePushMessage(uint callId, int commandId, string json, string body)
        {
            switch (commandId)
            {
                case S2C_KICK_OUT:
                    EnterKickOutState();
                    break;
                case S2C_PUSH:
                    //收到服务器Push消息后立刻回一个包给服务器，表示这个消息正常收到了
                    SendPushFeedback(body);
                    DelegateAggregator.ExecuteLuaCallback(0, json);
                    break;
            }
        }

        private void EnterKickOutState()
        {
            SafeStopCoroutine(ref _reconnectCoroutine);
            SafeStopCoroutine(ref _heartbeatCoroutine);
            Close();
        }

        private void EnterHeartbeatState()
        {
            SafeStopCoroutine(ref _reconnectCoroutine);
            _lastHeartbeatTime = Time.realtimeSinceStartup;
            SafeStartCoroutine(ref _heartbeatCoroutine, DaemonHeartbeat());
        }

        private void EnterReconnectState()
        {
            _retryCount = 0;
            SafeStopCoroutine(ref _heartbeatCoroutine);
            SafeStartCoroutine(ref _reconnectCoroutine, DaemonReconnect());
        }

        public void SendLogin()
        {
            string body = GenerateBody(C2S_LOGIN);
            Packet packet = GeneratePacket(_heartbeatCallId++, body, C2S_LOGIN);
            if (packet == EMPTY_PACKET)
            {
                return;
            }
            Logger.Log(GetCmdDesc(C2S_LOGIN) + " 内容：", PandoraSettings.SDKTag);
            Logger.Log(body, PandoraSettings.SDKTag);
            Send(packet);
        }

        public void SendHeartbeat()
        {
            string body = GenerateBody();
            Packet packet = GeneratePacket(_heartbeatCallId++, body, C2S_HEARTBEAT);
            if (packet == EMPTY_PACKET)
            {
                return;
            }
            Logger.Log(GetCmdDesc(C2S_HEARTBEAT) + " 内容：", PandoraSettings.SDKTag);
            Logger.Log(body, PandoraSettings.SDKTag);
            Send(packet);
        }

        public void SendPushFeedback(string response)
        {
            string body = GeneratePushFeedbackBody(response);
            Packet packet = GeneratePacket(_heartbeatCallId++, body, C2S_PUSH, 2, 9000);
            if (packet == EMPTY_PACKET)
            {
                return;
            }
            Logger.Log(GetCmdDesc(C2S_PUSH) + " 内容：", PandoraSettings.SDKTag);
            Logger.Log(body, PandoraSettings.SDKTag);
            Send(packet);
        }

        private string GeneratePushFeedbackBody(string response)
        {
            UserData userData = DelegateAggregator.GetUserData();
            if (userData == null)
            {
                return string.Empty;
            }

            Dictionary<string, System.Object> dict = new Dictionary<string, System.Object>();
            dict["ret"] = 0;
            dict["err_msg"] = "";
            dict["resp"] = response;
            return MiniJSON.Json.Serialize(dict);
        }

        private string GenerateBody(int commandId = C2S_HEARTBEAT)
        {
            UserData userData = DelegateAggregator.GetUserData();
            if (userData == null)
            {
                return string.Empty;
            }

            Dictionary<string, System.Object> dict = new Dictionary<string, System.Object>();
            dict["open_id"] = userData.sOpenId;
            dict["app_id"] = userData.sAppId;
            dict["sarea"] = userData.sArea;
            dict["splatid"] = userData.sPlatID;
            dict["spartition"] = userData.sPartition;
            dict["access_token"] = userData.sAccessToken;
            dict["acctype"] = userData.sAcountType;
            dict["sroleid"] = userData.sRoleId;
            return MiniJSON.Json.Serialize(dict);
        }

        private IEnumerator DaemonHeartbeat()
        {
            while (true)
            {
                float delta = Time.realtimeSinceStartup - _lastHeartbeatTime;
                if (_hasHeartbeatResponse == false && delta >= HEARTEART_TIMEOUT)
                {
                    EnterReconnectState();
                    yield break;
                }
                else if (delta >= HEARTBEAT_INTERVAL)
                {
                    _lastHeartbeatTime = Time.realtimeSinceStartup;
                    _hasHeartbeatResponse = false;
                    if (PandoraSettings.PauseSocketSending == false)
                    {
                        SendHeartbeat();
                    }
                    else
                    {
                        _hasHeartbeatResponse = true;
                    }
                }
                yield return null;
            }
        }

        private IEnumerator DaemonReconnect()
        {
            while (true)
            {
                float delta = Time.realtimeSinceStartup - _lastReconnectTime;
                if (delta > GetReconnectInterval(_retryCount))
                {
                    _retryCount = (_retryCount + 1) % 4;
                    _lastReconnectTime = Time.realtimeSinceStartup;
                    Reconnect();
                }
                yield return null;
            }
        }

        private int GetReconnectInterval(int retryCount)
        {
            int baseValue = (1 << (retryCount + 3));
            return baseValue + UnityEngine.Random.Range(0, baseValue);
        }

        public void Send(uint callId, string requestBody, int commandId, int type = 1, int moduleId = 10)
        {
            Packet packet = GeneratePacket(callId, requestBody, commandId, type, moduleId);
            if (packet == EMPTY_PACKET)
            {
                return;
            }
            Logger.Log(GetCmdDesc(commandId, type) + " CallId：" + callId.ToString(), PandoraSettings.SDKTag);
            Logger.Log(requestBody, PandoraSettings.SDKTag);
            Send(packet);
        }

        private Packet GeneratePacket(uint callId, string body, int commandId, int type = 1, int moduleId = 10)
        {
            UserData userData = DelegateAggregator.GetUserData();
            if (string.IsNullOrEmpty(userData.sOpenId) == true)
            {
                return EMPTY_PACKET;
            }
            Packet dict = new Packet();
            dict["seq_id"] = callId;                        // 请求的序列号
            dict["cmd_id"] = commandId;                     // 发送broker的命令字，目前有两类: 5001 经分上报  9000 lua请求
            dict["type"] = type;                            // 1 表示请求类型, 2 表示响应类型，默认为请求
            dict["from_ip"] = "10.0.0.108";                 // 来源IP
            dict["process_id"] = 1;                         // 来源进程
            dict["mod_id"] = moduleId;                      // 来源模块编号 10 表示sdk模块
            dict["version"] = userData.sSdkVersion;         // 版本号
            dict["body"] = body;                            // 要下发的json数据
            dict["app_id"] = userData.sAppId;               // 游戏appid
            dict["network_type"] = GetNetworkType();        //网络类型
            //dict["send_timestamp"] = TimeHelper.NowMilliseconds().ToString(); //发送时间戳，调整到发送时刻赋值
            return dict;
        }

        public override void CleanBeforeClose()
        {
            base.CleanBeforeClose();
            SafeStopCoroutine(ref _reconnectCoroutine);
            SafeStopCoroutine(ref _heartbeatCoroutine);
        }

        protected virtual string GetCmdDesc(int commandId, int type = 1)
        {
            string result = string.Empty;
            switch (commandId)
            {
                case C2S_LOGIN:
                    result = "登录";
                    break;
                case C2S_HEARTBEAT:
                    result = "心跳";
                    break;
                case S2C_PUSH:
                    result = "后台推送";
                    break;
                case C2S_ACTION:
                    result = "拉取活动";
                    break;
                case C2S_STATS:
                    result = "经分上报";
                    break;
                case S2C_KICK_OUT:
                    result = "踢下线";
                    break;
                case C2S_SOCIAL:
                    result = "社区";
                    break;
                case C2S_FEED:
                    result = "Feed流";
                    break;
                default:
                    result = "未知命令";
                    break;
            }
            if(type == 1)
            {
                return string.Format("客户端请求, cmd_id = {0} <color=#00ff00>({1})</color>", commandId.ToString(), result);
            }
            return string.Format("服务器响应, cmd_id = {0} <color=#00ff00>({1})</color>", commandId.ToString(), result);
        }

    }
}
