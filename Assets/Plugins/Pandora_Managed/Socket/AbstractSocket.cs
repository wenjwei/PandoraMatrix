using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Packet = System.Collections.Generic.Dictionary<string, object>;

namespace com.tencent.pandora
{
    /// <summary>
    /// Socket连接步骤
    /// 1.Dns查询IPAddress
    /// 2.连接Socket
    /// 数据结构：
    /// 1.4字节的包头，内容为包体长度值
    /// 2.包体内容
    /// </summary>
    public abstract class AbstractTcpClient : MonoBehaviour
    {
        //包头长度size(uint)
        protected const int HEADER_SIZE = 4;
        protected const int SEND_BUFFER_SIZE = 256 * 1024;
        protected static Packet EMPTY_PACKET = new Packet();
        protected const int REASON_DNS_FAILED = 1;
        protected const int REASON_SOCKET_FAILED = 2;
        protected const int DNS_RESOLVE_TIMEOUT = 2;
        protected const int SOCKET_CONNECT_TIMEOUT = 2;

        private object _lockObj = new object();

        protected Socket _socket;
        protected string _host;
        protected string _address;
        protected IPAddress _ipAddress;
        protected int _port;
        protected int _failedReason;
        //接受最大包的字节长度为4M
        protected const int MAX_SHARED_BUFFER_LENGTH = 4 * 1024 * 1024;
        private byte[] _sharedReceiveBuffer = new byte[1024];
        private byte[] _sharedSendBuffer = new byte[1024];
        private byte[] _bodyBuffer;
        private int _bodyLength = 0;
        private int _readBodyLength = 0;
        private byte[] _headerBuffer = new byte[HEADER_SIZE];

        private Queue<Packet> _sendQueue;
        private Coroutine _waitCoroutine;
        private Coroutine _inspectCoroutine;
        private bool _canDaemonSend;
        private bool _canDaemonReceive;

        volatile private SocketState _state = SocketState.Disconnect;
        private Action _callback; //不管socket成功失败都给上层一个回调
        /// <summary>
        /// 当Broker域名解析失败或IP连接不上时，使用替代IP地址
        /// </summary>
        public string AlternateIp1 { get; set; }
        public string AlternateIp2 { get; set; }
        private bool _canConnectAlternateIp;
        
        private int _dnsResolveStartTime;
        private int _socketConnectStartTime;

        public void Reconnect()
        {
            Connect(_host, _port, null);
        }

        public void Connect(string host, int port, Action callback)
        {
            if (this.State == SocketState.Connecting)
            {
                return;
            }

            Close();
            _host = host;
            _port = port;
            _callback = callback;
            if (_sendQueue == null)
            {
                _sendQueue = new Queue<Packet>();
            }
            this.State = SocketState.Connecting;
            _canConnectAlternateIp = true;
            _dnsResolveStartTime = 0;
            _socketConnectStartTime = 0;
            Logger.LogInfo("开始连接Socket : " + this.ToString(), PandoraSettings.SDKTag);
            SafeStartCoroutine(ref _waitCoroutine, WaitSocketConnect());
            _ipAddress = DnsCache.Get(_host);
            if(_ipAddress != null)
            {
                BeginConnectSocket(_ipAddress, _port);
            }
            else
            {
                BeginConnectHost(_host);
            }
        }

        private void BeginConnectHost(string host)
        {
            _dnsResolveStartTime = TimeHelper.NowSeconds();
            Dns.BeginGetHostAddresses(host, OnGetHostAddress, host);
        }

        private void OnGetHostAddress(IAsyncResult result)
        {
            IPAddress[] addresses = Dns.EndGetHostAddresses(result);
            if(this.State == SocketState.Connecting)
            {
                try
                {
                    if (addresses != null && addresses.Length != 0)
                    {
                        int index = (new System.Random()).Next(addresses.Length);
                        _ipAddress = addresses[index];
                        DnsCache.Save(_host, _ipAddress);
                        BeginConnectSocket(_ipAddress, _port);
                    }
                    else
                    {
                        InnerHandleSocketConnectFailed(REASON_DNS_FAILED);
                    }
                }
                catch
                {
                    InnerHandleSocketConnectFailed(REASON_DNS_FAILED);
                }
            }
        }

        private void BeginConnectSocket(IPAddress address, int port)
        {
            try
            {
                _socketConnectStartTime = TimeHelper.NowSeconds();
                _address = address.ToString();
                _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.SendTimeout = SOCKET_CONNECT_TIMEOUT * 1000;
                _socket.ReceiveTimeout = SOCKET_CONNECT_TIMEOUT * 1000;
                _socket.SendBufferSize = SEND_BUFFER_SIZE;
                _socket.BeginConnect(address, port, OnConnectSocket, _socket);
            }
            catch
            {
                //left blank
            }
        }

		private void OnConnectSocket(IAsyncResult result)
		{
			if (this.State == SocketState.Connecting || this.State == SocketState.Success)
			{
				HandleSocketConnectSuccess(result);
			}
			else if (this.State == SocketState.Failed)
			{
				HandleSocketConnectFailed(result);
			}
		}

		private void HandleSocketConnectFailed(IAsyncResult result)
		{
			try
			{
				Socket socket = result.AsyncState as Socket;
				socket.EndConnect(result);
			}
			catch
			{
				// left blank
			}
		}

		private void HandleSocketConnectSuccess(IAsyncResult result)
		{
			try
			{
				Socket socket = result.AsyncState as Socket;
				socket.EndConnect(result);
                if (socket.Connected == true)
				{
					this.State = SocketState.Success;
				}
				else
				{
					InnerHandleSocketConnectFailed(REASON_SOCKET_FAILED);
				}
			}
			catch
			{
				InnerHandleSocketConnectFailed(REASON_SOCKET_FAILED);
			}
		}

		private void InnerHandleSocketConnectFailed(int reason)
        {
            _failedReason = reason;
            if (this.CanConnectAlternateIp == true)
            {
                try
                {
                    ConnectAlternateIp();
                }
                catch
                {
                    this.State = SocketState.Failed;
                }
            }
            else
            {
                this.State = SocketState.Failed;
            }
        }

        #region 连接后备Ip
        //一次连接过程只走一次后备ip途径
        private bool CanConnectAlternateIp
        {
            get
            {
                bool result = _canConnectAlternateIp == true &&
                             (string.IsNullOrEmpty(this.AlternateIp1) == false || string.IsNullOrEmpty(this.AlternateIp2) == false);
                _canConnectAlternateIp = false;
                return result;
            }
        }

        private void ConnectAlternateIp()
        {
            _ipAddress = GetTargetIpAddress();
            BeginConnectSocket(_ipAddress, _port);
        }

        private IPAddress GetTargetIpAddress()
        {
            return IPAddress.Parse(SelectAlternateIp());
        }

        private string SelectAlternateIp()
        {
            List<string> ipList = new List<string>();
            if (string.IsNullOrEmpty(this.AlternateIp1) == false)
            {
                ipList.Add(this.AlternateIp1);
            }
            if (string.IsNullOrEmpty(this.AlternateIp2) == false)
            {
                ipList.Add(this.AlternateIp2);
            }
            int index = new System.Random().Next(0, ipList.Count);
            return ipList[index];
        }
        #endregion
        /// <summary>
        /// 因为不能在Socket的异步线程中调用主线程的接口
        /// 所以必须创建一个协程以检查连接成功并初始化相关协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitSocketConnect()
        {
            while(true)
            {
                CheckDnsResolveStatus();
                CheckSocketConnectStatus();
                if (this.State == SocketState.Success)
                {
                    _canDaemonReceive = true;
                    _canDaemonSend = true;
                    OnConnected();
                    if(_callback != null)
                    {
                        _callback();
                    }
                    SafeStartCoroutine(ref _inspectCoroutine, DaemonSvrResponse());
                    yield break;
                }
                if(this.State == SocketState.Failed)
                {
                    OnConnectFailed();
                    if (_callback != null)
                    {
                        _callback();
                    }
                    yield break;
                }
                yield return null;
            }
        }

        private void CheckDnsResolveStatus()
        {
            if(this.State == SocketState.Connecting)
            {
                if (_dnsResolveStartTime != 0 && (TimeHelper.NowSeconds() - _dnsResolveStartTime) > DNS_RESOLVE_TIMEOUT)
                {
                    InnerHandleSocketConnectFailed(REASON_DNS_FAILED);
                }
            }
        }

        private void CheckSocketConnectStatus()
        {
            if(this.State == SocketState.Connecting)
            {
                if (_socket != null && _socket.Connected == true)
                {
                    this.State = SocketState.Success;
                }
                else if (_socketConnectStartTime != 0 && (TimeHelper.NowSeconds() - _socketConnectStartTime) > SOCKET_CONNECT_TIMEOUT)
                {
                    InnerHandleSocketConnectFailed(REASON_SOCKET_FAILED);
                }
            }
        }

        protected void DaemonReceive()
        {
            try
            {
                if (_socket == null || _socket.Connected == false)
                {
                    Close();
                    return;
                }
                while (this.State == SocketState.Success && _socket != null && _socket.Connected == true && _socket.Available > 0)
                {
                    if (_bodyLength == 0 && _socket.Available >= HEADER_SIZE)
                    {
                        //读取包头
                        _socket.Receive(_headerBuffer);
                        _bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_headerBuffer, 0));
                        _bodyBuffer = GetSharedBuffer(_sharedReceiveBuffer, _bodyLength);
                        _readBodyLength = 0;
                    }
                    int size = _bodyLength - _readBodyLength;
                    #region 读取Socket中的数据
                    if (size > 0)
                    {
                        try
                        {
                            //读取包体
                            int readLength = _socket.Receive(_bodyBuffer, _readBodyLength, size, SocketFlags.None);
                            if (readLength > 0)
                            {
                                _readBodyLength += readLength;
                            }
                        }
                        catch(Exception e)
                        {
                            string error = string.Format("{0}从Socket中读取数据过程中发生异常， 包体数据长度_bodyLength{1}\n ExceptionMsg:{2}\n StackTrace:{3}", this, _bodyLength,e.Message,e.StackTrace);
                            Logger.LogError(error, PandoraSettings.SDKTag);
                            Close();//发生读取Socket数据发生异常后，主动断开连接
                            return;
                        }
                    }
                    #endregion
                    #region 处理一个完成包
                    if (_readBodyLength == _bodyLength)
                    {
                        try
                        {
                            OnReceived(_bodyBuffer, _bodyLength);
                        }
                        catch (Exception e)
                        {
                            string error = string.Format("{0} 处理回包发生异常: {1} {2} {3}", this.ToString(), e.GetType().ToString(), e.Message, e.StackTrace);
                            Logger.LogError(error, PandoraSettings.SDKTag);
                        }
                        //读完一个完整包
                        _bodyLength = 0;
                        _readBodyLength = 0;
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                string error = string.Format("{0} 读取数据过程中发生异常: {1} {2} {3}", this.ToString(), e.GetType().ToString(), e.Message, e.StackTrace);
                Logger.LogError(error, PandoraSettings.SDKTag);
                Close();
            }
        }
        private byte[] GetSharedBuffer(byte[] sharedBuffer, int bodyLen)
        {
            if(sharedBuffer.Length < bodyLen)
            {
                int len = sharedBuffer.Length;
                while(len < bodyLen)
                {
                    len = len * 2;
                    if(len > MAX_SHARED_BUFFER_LENGTH)
                    {
                        len = MAX_SHARED_BUFFER_LENGTH;
                        break;
                    }
                }
                sharedBuffer = new byte[len];
                return sharedBuffer;
            }
            return sharedBuffer;
        }

        //Update版
        protected void DaemonSend()
        {
            if (_socket == null || _socket.Connected == false)
            {
                Close();
                return;
            }
            if (this.State == SocketState.Success && PandoraSettings.PauseSocketSending == false)
            {
                while (_sendQueue.Count > 0)
                {
                    Packet packet = _sendQueue.Dequeue();
                    try
                    {
                        int packetDataLength = 0;
                        byte[] packetData = SerializePacket(packet, out packetDataLength);
                        if (packetData != null)
                        {
                            _socket.Send(packetData, packetDataLength, SocketFlags.None);
                        }
                    }
                    catch (Exception e)
                    {
                        string error = string.Format("{0} 发送数据过程中发生异常: {1} {2} {3}", this.ToString(), e.GetType().ToString(), e.Message, e.StackTrace);
                        Logger.LogError(error, PandoraSettings.SDKTag);
                        Close();
                        return;
                    }
                }
            }
        }
        
        protected virtual byte[] SerializePacket(Packet packet, out int packetDataLength)
        {
            packet["send_timestamp"] = TimeHelper.NowMilliseconds().ToString(); //发送时间戳
            packet["broker_vip"] = _address;
            string json = MiniJSON.Json.Serialize(packet);
            // 数据压缩
            int compressedDataLength = 0;
            byte[] compressedData = Miniz.Compress(json, out compressedDataLength);
            if(compressedData == null)
            {
                packetDataLength = 0;
                return null;
            }
            // 加上头4字节包长 包长字节序转换
            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(compressedDataLength));
            // 组包
            packetDataLength = header.Length + compressedDataLength;
            if(packetDataLength > MAX_SHARED_BUFFER_LENGTH)
            {
                Logger.LogError(this.ToString() + " 发送包体积超过最大值： " + packetDataLength, PandoraSettings.SDKTag);
                return null;
            }
            byte[] data = GetSharedBuffer(_sharedSendBuffer, packetDataLength);
            Array.Copy(header, 0, data, 0, header.Length);
            Array.Copy(compressedData, 0, data, header.Length, compressedDataLength);
            return data;
        }

        /// <summary>
        /// 成功连接时的回调
        /// </summary>
        protected abstract void OnConnected();

        /// <summary>
        /// 连接失败时回调
        /// </summary>
        protected virtual void OnConnectFailed()
        {
            _sendQueue.Clear();
            Logger.Log("<color=#ffff00>" + this.ToString() + " 连接失败~</color>", PandoraSettings.SDKTag);
        }
        /// <summary>
        /// 处理回包
        /// </summary>
        /// <param name="content"></param>
        protected abstract void OnReceived(byte[] content, int length);
        
        public void Send(Packet packet)
        {
            if(this.State == SocketState.Disconnect || this.State == SocketState.Failed)
            {
                Reconnect();
            }
            _sendQueue.Enqueue(packet);
        }

        protected SocketState State
        {
            get
            {
                lock(_lockObj)
                {
                    return _state;
                }
            }
            set
            {
                lock(_lockObj)
                {
                    _state = value;
                }
            }
        }

        /// <summary>
        /// 供子类实现，子类关闭之前的清理工作
        /// </summary>
        public virtual void CleanBeforeClose()
        {
        }

        public virtual void Close()
        {
            try
            {
                this.State = SocketState.Disconnect;
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
            }
            catch
            {
                //Left blank
            }
            
            SafeStopCoroutine(ref _waitCoroutine);
            SafeStopCoroutine(ref _inspectCoroutine);
            _canDaemonReceive = false;
            _canDaemonSend = false;
        }

        protected virtual void Update()
        {
            if(_canDaemonReceive == true)
            {
                DaemonReceive();
            }
            if(_canDaemonSend == true)
            {
                DaemonSend();
            }
        }

        protected void SafeStartCoroutine(ref Coroutine coroutineRef, IEnumerator enumerator)
        {
            if(coroutineRef != null)
            {
                StopCoroutine(coroutineRef);
            }
            coroutineRef = StartCoroutine(enumerator);
        }

        protected void SafeStopCoroutine(ref Coroutine coroutineRef)
        {
            if(coroutineRef != null)
            {
                StopCoroutine(coroutineRef);
                coroutineRef = null;
            }
        }

        protected string GetNetworkType()
        {
            switch(Application.internetReachability)
            {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "Mobile";
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "Lan";
            }
            return "None";
        }

        public override string ToString()
        {
            return string.Format(" {0} Host: {1}, Port: {2} ", this.GetType().Name, _host, _port.ToString());
        }

        public enum SocketState
        {
            Disconnect,
            Connecting,
            Success,
            Failed
        }

        public virtual IEnumerator DaemonSvrResponse()
        {
            yield return null;
        }

        public IEnumerator DelayToExecute(int second, Action action)
        {
            yield return new WaitForSeconds(second);
            action();
        }
    }

}
