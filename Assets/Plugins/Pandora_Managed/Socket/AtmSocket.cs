using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using UnityEngine;
using Packet = System.Collections.Generic.Dictionary<string, object>;
using System.Collections;

namespace com.tencent.pandora
{
    /// <summary>
    /// AtmSocket为上报日志所用，重要性略低一点
    /// 为尽量减少对游戏的干扰，所以AtmSocket不做心跳机制
    /// 为避免AtmSocket太长时间（超过15分钟）没有发送信息给服务器而导致链接断开，
    /// 设置为当距离上次成功收到回包时间超过5*60s后发送信息前，自动重连一次服务器。
    /// </summary>
    public class AtmSocket : AbstractTcpClient
    {
        public const int TNM2_TYPE_LITERALS = 2;
        //距离上次发包时间超过该值后，自动重连
        public const int RECONNECT_INTERVAL = 20;

        //上报包字节长度最大为4K
        public const int MAX_PACKET_LENGTH = 4 * 1024;

        public const int C2S_LOG = 5000;
        private int _callId = 0;

        private float _lastReceivedTime;
        //可重用对象
        private static Dictionary<string, System.Object> _bodyDict = new Dictionary<string, object>();
        private static List<Dictionary<string, System.Object>> _extendObjList = new List<Dictionary<string, object>>();
        private static Dictionary<string, System.Object> _idObj = new Dictionary<string, object>();
        private static Dictionary<string, System.Object> _typeObj = new Dictionary<string, object>();
        private static Dictionary<string, System.Object> _valueObj = new Dictionary<string, object>();

        private string _systemInfo;
        private string _prefix;
        private const int MAX_RETRY_COUNT = 5;
        private const int MAX_FAILED_COUNT = 200;
        private const int SVRVER_CALLBACK_TIMEOUT = 10;
        private const int MAX_WAIT_TIME = 120;//120s后重发
        private Dictionary<int, AtmPacket> _sendingDict = new Dictionary<int, AtmPacket>();
        private Dictionary<int, AtmPacket> _failedDict = new Dictionary<int, AtmPacket>();
        private List<int> _transferCallIdList = new List<int>();
       
        /// <summary>
        /// type 为tnm2上报类型值
        /// 0：叠加类型
        /// 1：平均值类型
        /// 2：告警类型
        /// 默认为叠加类型
        /// </summary>
        /// <param name="error"></param>
        /// <param name="id"></param>
        public void Report(string error, int id = 0, int type = 0)
        {
            try
            {
                Logger.Log("上报ATM: " + error + " CallId: " + _callId.ToString(), PandoraSettings.SDKTag);
                string body = GenerateBody(error, id, type);
                Packet packet = GeneratePacket(body, C2S_LOG);
                if (packet == EMPTY_PACKET)
                {
                    return;
                }
                if ((Time.realtimeSinceStartup - _lastReceivedTime) > RECONNECT_INTERVAL)
                {
                    if (this.State == SocketState.Success)
                    {
                        Close();
                    }
                }

                SendPacket(_callId, packet);
            }
            catch (Exception e)
            {
                Logger.LogError("上报流水日志失败：  " + error + " " + e.Message, PandoraSettings.SDKTag);
            }
        }

        private string GenerateBody(string message, int id, int type)
        {
            UserData userData = DelegateAggregator.GetUserData();
            _bodyDict["str_openid"] = userData.sOpenId;
            _bodyDict["spartition"] = userData.sPartition;
            _bodyDict["splatid"] = userData.sPlatID;
            _bodyDict["str_userip"] = "10.0.0.1";
            _bodyDict["str_respara"] = GetPrefix() + message + GetSystemInfo();
            _bodyDict["uint_report_type"] = 2;
            _bodyDict["uint_toreturncode"] = 1;
            _bodyDict["uint_log_level"] = 2;
            _bodyDict["str_openid"] = userData.sOpenId;
            _bodyDict["sarea"] = userData.sArea;
            _bodyDict["str_hardware_os"] = "unity";
            _bodyDict["str_sdk_version"] = userData.sSdkVersion;
            _bodyDict["uint_serialtime"] = TimeHelper.NowSeconds();
            _bodyDict["extend"] = GetExtendObjList(id, type, message);
            return MiniJSON.Json.Serialize(_bodyDict);
        }

        private List<Dictionary<string, System.Object>> GetExtendObjList(int id, int type, string content)
        {
            if (_extendObjList.Count == 0)
            {
                _idObj["name"] = "attr_id_0";
                _idObj["value"] = id;
                _extendObjList.Add(_idObj);

                _typeObj["name"] = "attr_type_0";
                _typeObj["value"] = type;
                _extendObjList.Add(_typeObj);

                _valueObj["name"] = "attr_value_0";
                _valueObj["value"] = content;
                _extendObjList.Add(_valueObj);
            }
            _idObj["value"] = id.ToString();
            _typeObj["value"] = type.ToString();

            _valueObj["value"] = "1";
            //类型2是字符型告警内容
            if (type == TNM2_TYPE_LITERALS)
            {
                _valueObj["value"] = GetPrefix() + content + GetSystemInfo();
            }
            return _extendObjList;
        }

        private string GetPrefix()
        {
            if (string.IsNullOrEmpty(_prefix) == true)
            {
                _prefix = PandoraSettings.IsProductEnvironment ? "[PRODUCT]" : "[TEST]";
            }
            return _prefix;
        }

        private string GetLocalIPAddress()
        {
            string localIPAddress = string.Empty;
            if (_socket != null && _socket.Connected)
            {
                localIPAddress = ((IPEndPoint)_socket.LocalEndPoint).Address.ToString();
            }
            return localIPAddress;
        }

        private string GetSystemInfo()
        {
            if (string.IsNullOrEmpty(_systemInfo) == true)
            {
                UserData userData = DelegateAggregator.GetUserData();
                StringBuilder sb = new StringBuilder();
                sb.Append("|PandoraVersion="); sb.Append(userData.sSdkVersion);
                sb.Append("&GameVersion="); sb.Append(userData.sGameVer);
                sb.Append("&OperatingSystem="); sb.Append(SystemInfo.operatingSystem);
                sb.Append("&ProcessorType="); sb.Append(SystemInfo.processorType);
                sb.Append("&SystemMemorySize="); sb.Append(SystemInfo.systemMemorySize);
                sb.Append("&NetworkType="); sb.Append(GetNetworkType());
                sb.Append("&IP="); sb.Append(GetLocalIPAddress());
                _systemInfo = sb.ToString();
            }
            return _systemInfo;
        }

        private Packet GeneratePacket(string body, int commandId)
        {
            UserData userData = DelegateAggregator.GetUserData();
            if (string.IsNullOrEmpty(userData.sOpenId) == true)
            {
                return EMPTY_PACKET;
            }
            Packet packet = new Packet();
            packet["seq_id"] = _callId++;                     // 请求的序列号
            packet["cmd_id"] = commandId;                       // 上报命令字 5000
            packet["type"] = 1;                               // 1 表示请求类型
            packet["from_ip"] = "10.0.0.108";                 // 来源IP
            packet["process_id"] = 1;                         // 来源进程
            packet["mod_id"] = 10;                            // 来源模块编号 10 表示sdk模块
            packet["version"] = userData.sSdkVersion;         // 版本号
            packet["body"] = body;                            // 要上报json数据
            packet["app_id"] = userData.sAppId;               // 游戏appid
            packet["network_type"] = GetNetworkType();
            return packet;
        }

        public void Send(string requestBody, int commandId)
        {
            Packet packet = GeneratePacket(requestBody, commandId);
            if (packet == EMPTY_PACKET)
            {
                return;
            }
            Logger.Log("Broker 发送信息，CommandId： " + commandId.ToString(), PandoraSettings.SDKTag);
            Logger.Log(requestBody, PandoraSettings.SDKTag);
            SendPacket(_callId, packet);
        }

        //可以在第一次连接成功时上报之前记录在本地的上报成功和失败数
        protected override void OnConnected()
        {
            _lastReceivedTime = Time.realtimeSinceStartup;
            string msg = string.Format("<color=#00ff00>AtmSocket 连接成功: {0} {1}:{2}</color>", _host, _address, _port.ToString());
            Logger.LogInfo(msg, PandoraSettings.SDKTag);
        }


        //{"type":2,"seq_id":0,"cmd_id":5000,"body":"{\"ret\":0,\"err_msg\":\"\"}"}
        //{"type":2,"seq_id":2,"cmd_id":5001,"body":"{\"ret\":0,\"err_msg\":\"\"}"}
        protected override void OnReceived(byte[] content, int length)
        {
            _lastReceivedTime = Time.realtimeSinceStartup;
            string result = Miniz.UnCompressToString(content, length);
            Dictionary<string, System.Object> dict = MiniJSON.Json.Deserialize(result) as Dictionary<string, System.Object>;
            var body = MiniJSON.Json.Deserialize(dict["body"] as string) as Dictionary<string, System.Object>;
            int ret = Convert.ToInt32(body["ret"]);
            int seqId = Convert.ToInt32(dict["seq_id"]);
            if (ret == 0)
            {
                if (_sendingDict.ContainsKey(seqId))
                {
                    _sendingDict.Remove(seqId);
                }
                if (_failedDict.ContainsKey(seqId))
                {
                    _failedDict.Remove(seqId);
                }
            }

            Logger.Log("收到ATM上报回包： " + result, PandoraSettings.SDKTag);
        }

        protected override byte[] SerializePacket(Packet packet, out int packetDataLength)
        {
            byte[] data = base.SerializePacket(packet, out packetDataLength);
            if (data != null && data.Length > MAX_PACKET_LENGTH)
            {
                Logger.LogWarning("上报包字节长度超过4K，当前包长： " + data.Length, PandoraSettings.SDKTag);
            }
            return data;
        }

        private void SendPacket(int callId, Packet packet)
        {
            int key = callId - 1;// callId 减1后才跟packet中的一致
            AtmPacket item = new AtmPacket();
            item.packet = packet;
            item.sendStartTime = TimeHelper.NowSeconds();
            item.retryCount = 0;
            _sendingDict.Add(key, item);
            Send(packet);
        }

        //判断是否超出重试次数，超过就丢弃
        private void ResendPacket(int callId, AtmPacket atmPacket)
        {
            if (atmPacket.retryCount < MAX_RETRY_COUNT)
            {
                atmPacket.sendStartTime = TimeHelper.NowSeconds();
                atmPacket.retryCount++;
                _sendingDict.Add(callId, atmPacket);
                Logger.Log(string.Format("Atm resend,callId:{0}",callId), PandoraSettings.SDKTag);
                Send(atmPacket.packet);
            }
        }

        public override IEnumerator DaemonSvrResponse()
        {
            while (true)
            {
                _transferCallIdList.Clear();
                //判断_sendingDict 中的包是否超时，超时的加入_failedDict
                foreach (var item in _sendingDict)
                {
                    if (TimeHelper.NowSeconds() - item.Value.sendStartTime > SVRVER_CALLBACK_TIMEOUT)
                    {
                        _transferCallIdList.Add(item.Key);
                    }
                }

                foreach (var item in _transferCallIdList)
                {
                    //_failedDict中数量超限丢弃
                    if (_failedDict.Count < MAX_FAILED_COUNT)
                    {
                        _sendingDict[item].waitStartTime = TimeHelper.NowSeconds();
                        _failedDict.Add(item, _sendingDict[item]);
                    }
                    _sendingDict.Remove(item);
                }

                _transferCallIdList.Clear();
                //判断_failedDict中的包是否到了重发时间，到了就重发
                foreach (var item in _failedDict)
                {
                    if (TimeHelper.NowSeconds() - item.Value.waitStartTime > MAX_WAIT_TIME)
                    {
                        _transferCallIdList.Add(item.Key);
                    }
                }

                foreach (var item in _transferCallIdList)
                {
                    var value = _failedDict[item];
                    _failedDict.Remove(item);
                    int random = UnityEngine.Random.Range(1, 5);
                    StartCoroutine(DelayToExecute(random, () => ResendPacket(item, value)));
                }

                yield return null;
            }
        }
    }

    public class AtmPacket
    {
        public Packet packet;
        public int sendStartTime;
        public int retryCount;
        public int waitStartTime;
    }
}
