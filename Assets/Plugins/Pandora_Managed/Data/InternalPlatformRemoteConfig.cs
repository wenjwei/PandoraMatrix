using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{

    public class InternalPlatformRemoteConfig : RemoteConfig
    {
        //socket 连接备份为ip形式
        public string brokerAlternateIp1;
        public string brokerAlternateIp2;
        public int ruleId;
        public string remark;
        /// <summary>
        /// 是否上报日志到服务器
        /// </summary>
        public bool isNetLog;
        public bool isShowLogo;

        public InternalPlatformRemoteConfig(string response)
        {
            Parse(response);
        }

        protected override void Parse(string response)
        {
            try
            {
                if (response == "{}")
                {
                    IsError = true;
                    return;
                }
                Dictionary<string, System.Object> configDict = MiniJSON.Json.Deserialize(response) as Dictionary<string, System.Object>;
                if (configDict == null)
                {
                    IsError = true;
                    return;
                }
                //1.规则Id
                this.ruleId = ParseIntField(configDict, "id", 0);
                if (this.ruleId == 0)
                {
                    if (PandoraSettings.LoginPlatform != PlatformType.InternalPlatform)
                    {
                        Logger.LogInfo("当前连接开放平台，没有拉取到内部规则，使用默认数值", PandoraSettings.SDKTag);
                        return;
                    }
                    else
                    {
                        //Logger.LogInfo("<color=#ff0000>V1版本没有匹配到任何规则</color>", PandoraSettings.SDKTag);

                        //使用开放平台时允许内部平台不配置规则
                        IsEmpty = true;
                        return;
                    }
                }
                this.remark = ParseStringField(configDict, "remark");
                Logger.LogInfo("<color=#00ff00>匹配到V1版本管理端规则：</color>  " + this.ruleId.ToString() + "  " + this.remark, PandoraSettings.SDKTag);

                //2.总量开关
                this.totalSwitch = (ParseIntField(configDict, "totalSwitch") == 1);
                if (this.totalSwitch == false)
                {
                    Logger.LogInfo("<color=#ff0000>总量开关关闭，规则Id： " + this.ruleId.ToString() + this.remark + "</color>", PandoraSettings.SDKTag);
                    IsEmpty = true;
                    return;
                }

                //3.功能开关
                this.functionSwitchDict = ParseFuntionSwitch(configDict);
                // 4、是否Debug模式，是则日志落地，不是则忽略，默认不是Debug模式
                this.isDebug = (ParseIntField(configDict, "isDebug", 0) == 1);
                // 4.1、是否需要日志上报，默认需要，除非遇到上报异常情况，才取消上报
                this.isNetLog = (ParseIntField(configDict, "isNetLog", 1) == 1);
                this.logLevel = ConvertLogLevel(ParseIntField(configDict, "log_level", 0));
                // 5、broker host, port, cap_ip1, cap_ip2
                this.brokerHost = ParseStringField(configDict, "ip");
                this.brokerPort = ParseIntField(configDict, "port", 0);
                //开启开放平台开关时取WebSocket的配置
                if (PandoraSettings.LoginPlatform == PlatformType.OpenPlatformWithWebsocket)
                {
                    this.brokerHost = ParseStringField(configDict, "ws_ip");
                    this.brokerPort = ParseIntField(configDict, "ws_port", 0);
                }

                if (PandoraSettings.LoginPlatform != PlatformType.OpenPlatformWithWebsocket && (string.IsNullOrEmpty(this.brokerHost) || this.brokerPort == 0))
                {
                    string error = "Broker域名或端口配置错误，规则Id： " + this.ruleId.ToString();
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.ReportError(error);
                    this.IsError = true;
                    return;
                }

                // 5、cap_ip1, cap_ip2  
                this.brokerAlternateIp1 = ParseStringField(configDict, "cap_ip1");
                this.brokerAlternateIp2 = ParseStringField(configDict, "cap_ip2");
                //开启开放平台开关时取WebSocket的配置,因wss协议不能使用ip直连，虽在此赋值，但未使用
                if (PandoraSettings.LoginPlatform == PlatformType.OpenPlatformWithWebsocket)
                {
                    this.brokerAlternateIp1 = ParseStringField(configDict, "ws_cap_ip1");
                    this.brokerAlternateIp2 = ParseStringField(configDict, "ws_cap_ip2");
                }

                if (PandoraSettings.LoginPlatform != PlatformType.OpenPlatformWithWebsocket && (string.IsNullOrEmpty(this.brokerAlternateIp1) || string.IsNullOrEmpty(this.brokerAlternateIp2)))
                {
                    Logger.Log("<color=#ffff00>Broker的备选IP1或IP2没有配置</color>", PandoraSettings.SDKTag);
                }

                // 6、解析资源列表sourcelist，返回key为资源name，Value为FileInfo的dict
                bool hasError;
                this.groupFileDict = ParseGroupFileListDict(configDict, out hasError);
                this.assetInfoDict = ParseAssetInfoDict(configDict, out hasError);
                if (hasError == true)
                {
                    this.IsError = true;
                    return;
                }

                //把本地资源加到列表中
                Dictionary<string, AssetInfo> localAssetInfoDict = new Dictionary<string, AssetInfo>();
                Dictionary<string, List<AssetInfo>> localAssetInfoListDict = new Dictionary<string, List<AssetInfo>>();
                ConfigLocalAsset(ref localAssetInfoDict, ref localAssetInfoListDict);
                //如果云端规则里没有本地预埋的资源，则将本地预埋资源加入列表
                foreach (var item in localAssetInfoDict)
                {
                    if (!assetInfoDict.ContainsKey(item.Key))
                    {
                        assetInfoDict.Add(item.Key, item.Value);
                        Logger.Log(string.Format("本地资源{0}加入到云端资源配置中", item.Key), PandoraSettings.SDKTag);
                    }
                }

                // 7、解析文件分组字典
                int itemCount = localAssetInfoListDict.Count;
                this.assetInfoListDict = ParseAssetInfoListDict(configDict, assetInfoDict, out hasError, itemCount > 0 ? localAssetInfoListDict : null);
                if (hasError == true)
                {
                    this.IsError = true;
                    return;
                }
                PostProcessAssetInfoListDict(this.assetInfoListDict);
            }
            catch (Exception e)
            {
                string error = string.Format("解析InternalPlatformRemoteConfig发生错误：{0}\n{1} ", e.Message, e.StackTrace);
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error);
                IsError = true;
            }
        }
    }
}