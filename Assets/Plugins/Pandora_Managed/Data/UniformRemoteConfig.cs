using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using AssetInfo = com.tencent.pandora.RemoteConfig.AssetInfo;

namespace com.tencent.pandora
{
    public class UniformRemoteConfig : RemoteConfig
    {
        public string brokerAlternateIp1;
        public string brokerAlternateIp2;
        public List<FunctionSwitchInfo> switchInfoList = new List<FunctionSwitchInfo>();
        //拉取到的潘多拉内部管理端的规则（v1）
        private InternalPlatformRemoteConfig _internalPlatformConfig;
        //uniform版本规则(v2)
        public UniformConfig uniformConfig;
        //非法资源组
        private List<AssetInfo> _conflictAssetInfo = new List<AssetInfo>();
        private List<ConflictGroupInfo> _conflictGroupInfo = new List<ConflictGroupInfo>();
        private bool _isInternalCofnigValid = true;
        private bool _isUniformCofnigValid = true;
        private Dictionary<string, LogConfig> _logConfigDict = new Dictionary<string, LogConfig>();
        private Dictionary<string, string> _assetNameLogTagMap = new Dictionary<string, string>();
        //开关名称列表
        private static string[] FUNCTION_SWITCH_NAME_ARRAY = new string[]
        {
            "totalSwitch",                                          //与管理端 isAll 对应
            "test",                                                 //与管理端 isTest 对应
            "loadAssetFromLocal",                                   //与管理端 LoadAssetFromLocal 对应
            "loadAssetFirstFromLocal",                              //与管理端 LoadAssetFirstFromLocal 对应
            "fullLogOutput",                                        //与管理端 isFullLogOutput 对应
            "turnOffVerification",                                  //与管理端 TrunOffVerification 对应
            "console",                                              //与管理端 Console 对应
            "stressTesting",                                        //与管理端 StressTesting 对应
            "errorPanel",                                           //与管理端 ErrorPanel 对应
            "isDebug",                                              //与管理端 OpenLog 对应
        };

        //1 活动类型，2 活动，3 系统组件，4普通组件
        private static Dictionary<string, string> _relationTypeDict = new Dictionary<string, string>()
        {
            {"1","活动类型规则" },
            {"2","具体活动规则" },
            {"3","系统组件规则" },
            {"4","普通组件规则" },
        };

        private readonly string INTERNAL_LOG_TAG = "Internal";

        public UniformRemoteConfig(string response)
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
                //解析内部平台旧版规则
                ParseInternalPlatformConfig(response);
                //解析升级后的统一版本规则
                ParseUniformConfig(configDict);
                //合并规则
                MergeConfig();
            }
            catch (Exception e)
            {
                string error = string.Format("解析UniformRemoteConfig发生错误：{0}\n{1} ", e.Message, e.StackTrace);
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error);
                IsError = true;
            }
        }

        private void ParseInternalPlatformConfig(string response)
        {
            _internalPlatformConfig = new InternalPlatformRemoteConfig(response);
        }

        private void ParseUniformConfig(Dictionary<string, System.Object> configDict)
        {
            if (!IsUniformConfig(configDict))
            {
                _isUniformCofnigValid = false;
                return;
            }
            uniformConfig = new UniformConfig();
            Dictionary<string, System.Object> systemSource = configDict["system_source"] as Dictionary<string, System.Object>;
            if (!ParseSystemSource(systemSource))
            {
                _isUniformCofnigValid = false;
                return;
            }

            Dictionary<string, System.Object> appSource = configDict["app_source"] as Dictionary<string, System.Object>;
            if (!ParseAppSource(appSource))
            {
                _isUniformCofnigValid = false;
                return;
            }

            uniformConfig.loadSort = ParseIntListField(configDict, "load_sort");
            SortGroupList();
        }

        //强校验Uniform版本配置的字段
        private bool IsUniformConfig(Dictionary<string, System.Object> configDict)
        {
            if (!configDict.ContainsKey("system_source") || configDict["system_source"] == null)
            {
                return false;
            }

            if (!configDict.ContainsKey("app_source") || configDict["app_source"] == null)
            {
                return false;
            }

            if (!configDict.ContainsKey("load_sort") || configDict["load_sort"] == null)
            {
                return false;
            }
            return true;
        }

        private bool ParseSystemSource(Dictionary<string, System.Object> source)
        {
            if (source != null && source.ContainsKey("list") && source["list"] != null)
            {
                List<System.Object> list = source["list"] as List<System.Object>;
                if (list == null)
                {
                    return false;
                }
                foreach (var item in list)
                {
                    Dictionary<string, System.Object> configDict = item as Dictionary<string, System.Object>;

                    if (configDict == null)
                    {
                        continue;
                    }

                    SystemRuleInfo systemRuleInfo = new SystemRuleInfo();
                    systemRuleInfo.ruleId = ParseIntField(configDict, "ruleid", 0);
                    systemRuleInfo.id = ParseIntField(configDict, "id", 0);
                    systemRuleInfo.name = UnityWebRequest.UnEscapeURL(ParseStringField(configDict, "name"));
                    //感觉这块判断逻辑可能用不上，待前后端联调时确认
                    if (systemRuleInfo.ruleId == 0)
                    {
                        int ret = ParseIntField(configDict, "ret");
                        string errMsg = ParseStringField(configDict, "errmsg");
                        Logger.LogWarning(string.Format("<color=#ff0000>name:{0}, id:{1} 没有匹配到任何规则.ret:{2},errMsg:{3}</color>", systemRuleInfo.name, systemRuleInfo.id, ret, errMsg), PandoraSettings.SDKTag);
                        continue;
                    }
                    systemRuleInfo.remark = UnityWebRequest.UnEscapeURL(ParseStringField(configDict, "remark"));
                    Logger.LogInfo(string.Format("<color=#00ff00>name:{0},id:{1} 匹配到规则.ruleId:{2},remark:{3}</color>", systemRuleInfo.name, systemRuleInfo.id, systemRuleInfo.ruleId, systemRuleInfo.remark), PandoraSettings.SDKTag);
                    systemRuleInfo.serviceKey = ParseStringField(configDict, "service_key");
                    systemRuleInfo.relationType = ParseStringField(configDict, "relation_type");
                    systemRuleInfo.functionSwitch = ParseFunctionSwitch(ParseStringField(configDict, "switch_list"), systemRuleInfo.remark);
                    if (systemRuleInfo.functionSwitch.ContainsKey("isDebug") && systemRuleInfo.functionSwitch["isDebug"] == true)
                    {
                        systemRuleInfo.logLevel = ConvertLogLevel(ParseIntField(configDict, "log_level", 0));
                    }
                    systemRuleInfo.ip = ParseStringField(configDict, "ip");
                    systemRuleInfo.ipv6 = ParseStringField(configDict, "ipv6_ip");
                    systemRuleInfo.port = ParseIntField(configDict, "port");
                    systemRuleInfo.backupIp = ParseStringField(configDict, "backup_ip");
                    systemRuleInfo.capIp1 = ParseStringField(configDict, "cap_ip1");
                    systemRuleInfo.capIp2 = ParseStringField(configDict, "cap_ip2");
                    systemRuleInfo.capIpV6_1 = ParseStringField(configDict, "cap_v6ip1");
                    systemRuleInfo.capIpV6_2 = ParseStringField(configDict, "cap_v6ip2");
                    systemRuleInfo.cdnPrefix = UnityWebRequest.UnEscapeURL(ParseStringField(configDict, "cdn_prefix"));
                    systemRuleInfo.dirId = ParseIntField(configDict, "dir_id");
                    systemRuleInfo.assetInfoList = ParseSourceListField(configDict, "sourcelist", systemRuleInfo.cdnPrefix);
                    uniformConfig.systemRuleInfoList.Add(systemRuleInfo);
                    FillGroupList(systemRuleInfo, typeof(SystemRuleInfo).Name);
                }
                return IsSystemRuleInfoValid();
            }

            return false;
        }

        private bool IsSystemRuleInfoValid()
        {
            //系统规则只能有一个
            if (uniformConfig.systemRuleInfoList.Count != 1)
            {
                string errorMsg = string.Format("V2版本系统规则必须存在，并且只能有一条，当前有{0}条", uniformConfig.systemRuleInfoList.Count);
                Logger.LogError(errorMsg, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(errorMsg);
                return false;
            }

            bool isFrameConfiged = false;
            foreach (var item in uniformConfig.systemRuleInfoList)
            {
                //系统工具
                if (item.relationType == "3")
                {
                    if (item.serviceKey.ToLower() != "frame")
                    {
                        string errorMsg = string.Format("系统工具配置的不是frame。ruleId:{0},remark:{1}", item.ruleId, item.remark);
                        Logger.LogError(errorMsg, PandoraSettings.SDKTag);
                        DelegateAggregator.ReportError(errorMsg);
                        return false;
                    }
                    isFrameConfiged = true;
                    //校验全量开关
                    if (!item.functionSwitch.ContainsKey("totalSwitch"))
                    {
                        string warnMsg = string.Format("系统工具规则中全量开关关闭，潘多拉无法运行。ruleId:{0},remark:{1}", item.ruleId, item.remark);
                        Logger.LogWarning(warnMsg, PandoraSettings.SDKTag);
                        DelegateAggregator.ReportError(warnMsg);
                        return false;
                    }

                    //校验域名配置
                    if ((string.IsNullOrEmpty(item.ip) || item.port == 0))
                    {
                        string errorMsg = string.Format("系统工具规则中Broker域名或端口配置错误。ruleId:{0},remark:{1}", item.ruleId, item.remark);
                        Logger.LogError(errorMsg, PandoraSettings.SDKTag);
                        DelegateAggregator.ReportError(errorMsg);
                        return false;
                    }

                    //校验备份ip
                    if ((string.IsNullOrEmpty(item.capIp1) || string.IsNullOrEmpty(item.capIp2)))
                    {
                        string warnMsg = string.Format("<color=#ffff00>系统工具规则中备选IP1或IP2没有配置。ruleId:{0},remark:{1}</color>", item.ruleId, item.remark);
                        Logger.Log(warnMsg, PandoraSettings.SDKTag);
                    }
                }
            }

            if (!isFrameConfiged)
            {
                string errorMsg = "Uniform 版本规则中没有配置系统工具";
                Logger.LogError(errorMsg, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(errorMsg);
                return false;
            }
            return true;
        }

        private bool ParseAppSource(Dictionary<string, System.Object> source)
        {
            if (source != null && source.ContainsKey("list") && source["list"] != null)
            {
                List<System.Object> list = source["list"] as List<System.Object>;
                if (list == null)
                {
                    return false;
                }
                foreach (var item in list)
                {
                    Dictionary<string, System.Object> configDict = item as Dictionary<string, System.Object>;

                    if (configDict == null)
                    {
                        continue;
                    }

                    AppRuleInfo appRuleInfo = new AppRuleInfo();
                    appRuleInfo.ruleId = ParseIntField(configDict, "ruleid", 0);
                    appRuleInfo.id = ParseIntField(configDict, "id", 0);
                    appRuleInfo.name = UnityWebRequest.UnEscapeURL(ParseStringField(configDict, "name"));
                    if (appRuleInfo.ruleId == 0)
                    {
                        int ret = ParseIntField(configDict, "ret");
                        string errMsg = ParseStringField(configDict, "errmsg");
                        Logger.LogWarning(string.Format("<color=#ff0000>name:{0},id:{1} 没有匹配到任何规则.ret:{2},errMsg:{3}</color>", appRuleInfo.name, appRuleInfo.id, ret, errMsg), PandoraSettings.SDKTag);
                        continue;
                    }
                    appRuleInfo.remark = UnityWebRequest.UnEscapeURL(ParseStringField(configDict, "remark"));
                    Logger.LogInfo(string.Format("<color=#00ff00>name:{0},id:{1} 匹配到规则. ruleId:{2},remark:{3}</color>", appRuleInfo.name, appRuleInfo.id, appRuleInfo.ruleId, appRuleInfo.remark), PandoraSettings.SDKTag);
                    appRuleInfo.relationType = ParseStringField(configDict, "relation_type");
                    appRuleInfo.functionSwitch = ParseFunctionSwitch(ParseStringField(configDict, "switch_list"), appRuleInfo.remark);
                    if (appRuleInfo.functionSwitch.ContainsKey("isDebug") && appRuleInfo.functionSwitch["isDebug"] == true)
                    {
                        appRuleInfo.logLevel = ConvertLogLevel(ParseIntField(configDict, "log_level", 0));
                    }
                    appRuleInfo.cdnPrefix = UnityWebRequest.UnEscapeURL(ParseStringField(configDict, "cdn_prefix"));
                    appRuleInfo.dirId = ParseIntField(configDict, "dir_id");
                    appRuleInfo.assetInfoList = ParseSourceListField(configDict, "sourcelist", appRuleInfo.cdnPrefix);
                    uniformConfig.appRuleInfoList.Add(appRuleInfo);
                    FillGroupList(appRuleInfo, typeof(AppRuleInfo).Name);
                }
                return true;
            }
            return false;
        }

        //开关解析无需区分是活动还是组件
        private Dictionary<string, bool> ParseFunctionSwitch(string functionSwitch, string remark)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            char[] arr = functionSwitch.ToCharArray();
            int remoteLength = arr.Length;
            int localLength = FUNCTION_SWITCH_NAME_ARRAY.Length;
            if (remoteLength > localLength)
            {
                Logger.LogWarning(string.Format("{0} 规则中开关位数多于客户端位数，仅解析二者相匹配部分", remark), PandoraSettings.SDKTag);
            }
            else if (remoteLength < localLength)
            {
                Logger.LogWarning(string.Format("{0} 规则中开关位数少于客户端位数，仅解析二者相匹配部分", remark), PandoraSettings.SDKTag);
            }

            //取短值
            int length = remoteLength > localLength ? localLength : remoteLength;
            for (int i = 0; i < length; i++)
            {
                bool value = arr[i] == '1' ? true : false;
                if (value == true)
                {
                    result.Add(FUNCTION_SWITCH_NAME_ARRAY[i], value);
                }
            }

            return result;
        }

        private List<AssetInfo> ParseSourceListField(Dictionary<string, System.Object> dict, string key, string cdnPreifx)
        {
            List<AssetInfo> result = new List<AssetInfo>();
            List<System.Object> sourceList = new List<object>();
            if (dict.ContainsKey(key))
            {
                sourceList = dict[key] as List<System.Object>;
            }
            if (sourceList == null || sourceList.Count == 0)
            {
                return result;
            }

            foreach (var item in sourceList)
            {
                Dictionary<string, System.Object> configDict = item as Dictionary<string, System.Object>;
                if (configDict == null)
                {
                    continue;
                }
                string url = ParseStringField(configDict, "url");
                string name = Path.GetFileName(url);
                if (name.Contains("?"))
                {
                    name = name.Substring(0, name.IndexOf('?'));
                }
                AssetInfo assetInfo = new AssetInfo();
                assetInfo.name = name;
                assetInfo.url = cdnPreifx + url;//因为url首位为"/",不能使用Path.Combine拼接
                assetInfo.md5 = ParseStringField(configDict, "luacmd5");
                assetInfo.size = ParseIntField(configDict, "size");
                result.Add(assetInfo);
            }
            return result;
        }

        private List<int> ParseIntListField(Dictionary<string, System.Object> dict, string key)
        {
            List<object> temp = new List<object>();
            if (dict.ContainsKey(key))
            {
                temp = dict[key] as List<object>;
            }
            List<int> result = new List<int>();

            if (temp != null && temp.Count > 0)
            {
                foreach (var item in temp)
                {
                    result.Add(Convert.ToInt32(item));
                }
            }
            return result;
        }

        private void FillGroupList(CommonRuleInfo instance, string typeName)
        {
            UniformGroupInfo group = new UniformGroupInfo();
            group.id = instance.id;
            group.dirId = instance.dirId;//一个规则对应一组热更，只有一个热更包id
            group.assetInfoList = instance.assetInfoList;
            group.typeName = typeName;
            group.ruleInstance = instance;
            uniformConfig.groupList.Add(group);
        }

        private void SortGroupList()
        {
            //管理端因为同步问题可能下发的资源组个数和排序id不一致
            List<UniformGroupInfo> sorted = new List<UniformGroupInfo>();
            List<UniformGroupInfo> sortedSystemRuleInfo = new List<UniformGroupInfo>();
            List<UniformGroupInfo> sortedAppRuleInfo = new List<UniformGroupInfo>();

            for (int i = 0; i < uniformConfig.loadSort.Count; i++)
            {
                for (int j = 0; j < uniformConfig.groupList.Count; j++)
                {
                    if (uniformConfig.loadSort[i] == uniformConfig.groupList[j].dirId)
                    {
                        if (uniformConfig.groupList[j].ruleInstance.relationType == "3")
                        {
                            sortedSystemRuleInfo.Add(uniformConfig.groupList[j]);
                        }
                        else
                        {
                            sortedAppRuleInfo.Add(uniformConfig.groupList[j]);
                        }
                        break;
                    }
                }
            }

            //未加入的进行补录
            for (int i = 0; i < uniformConfig.groupList.Count; i++)
            {
                if (uniformConfig.groupList[i].ruleInstance.relationType == "3")
                {
                    if (!sortedSystemRuleInfo.Contains(uniformConfig.groupList[i]))
                    {
                        sortedSystemRuleInfo.Add(uniformConfig.groupList[i]);
                    }
                }
                else
                {
                    if (!sortedAppRuleInfo.Contains(uniformConfig.groupList[i]))
                    {
                        sortedAppRuleInfo.Add(uniformConfig.groupList[i]);
                    }
                }
            }

            sorted.AddRange(sortedSystemRuleInfo);
            sorted.AddRange(sortedAppRuleInfo);
            uniformConfig.groupList = sorted;
        }

        private void MergeConfig()
        {
            if (_internalPlatformConfig.IsError || _internalPlatformConfig.IsEmpty)
            {
                _isInternalCofnigValid = false;
                //Logger.LogWarning("<color=#ffff00>内部平台旧版规则为空或解析出错，请检查，此告警不影响规则合并</color>", PandoraSettings.SDKTag);
            }

            if (!_isInternalCofnigValid && !_isUniformCofnigValid)
            {
                this.IsError = true;
                Logger.LogError("<color=#ff0000>内部平台旧版规则和Uniform版规则均解析出错，无法合并</color>", PandoraSettings.SDKTag);
                return;
            }
            PrintInternalConfigInfo();
            PrintUniformConfigInfo();
            MergeHostAndPort();
            MergeFunctionSwitch();
            MergeLogConfig();
            MergeAssetInfoDict();
            MergeAssetInfoListDict();
            PostProcessAssetInfoListDict(this.assetInfoListDict);
            PrintMergedConfigInfo();
        }
        //统一版本规则系统工具的配置为第一优先级
        private void MergeHostAndPort()
        {
            if (_isUniformCofnigValid)
            {
                foreach (var item in uniformConfig.systemRuleInfoList)
                {
                    if (item.relationType == "3")
                    {
                        this.brokerHost = item.ip;
                        this.brokerPort = item.port;
                        this.brokerAlternateIp1 = item.capIp1;
                        this.brokerAlternateIp2 = item.capIp2;
                        break;
                    }
                }
                return;
            }

            //域名替换为内部平台设置的域名
            this.brokerHost = _internalPlatformConfig.brokerHost;
            this.brokerPort = _internalPlatformConfig.brokerPort;
            this.brokerAlternateIp1 = _internalPlatformConfig.brokerAlternateIp1;
            this.brokerAlternateIp2 = _internalPlatformConfig.brokerAlternateIp2;
        }

        private void MergeFunctionSwitch()
        {
            this.functionSwitchDict = new Dictionary<string, bool>();

            if (_isInternalCofnigValid)
            {
                this.totalSwitch = _internalPlatformConfig.totalSwitch;
                foreach (var item in _internalPlatformConfig.functionSwitchDict)
                {
                    this.switchInfoList.Add(new FunctionSwitchInfo(item.Key, 0, _internalPlatformConfig.ruleId));
                    this.functionSwitchDict.Add(item.Key, item.Value);
                }
            }

            if (_isUniformCofnigValid)
            {
                this.totalSwitch = this.totalSwitch || uniformConfig.systemRuleInfoList[0].functionSwitch["totalSwitch"];
                foreach (var item in uniformConfig.groupList)
                {
                    foreach (var innerItem in item.ruleInstance.functionSwitch)
                    {
                        this.switchInfoList.Add(new FunctionSwitchInfo(innerItem.Key, item.id, item.ruleInstance.ruleId));
                        if (this.functionSwitchDict.ContainsKey(innerItem.Key) == false)
                        {
                            this.functionSwitchDict.Add(innerItem.Key, innerItem.Value);
                        }
                    }
                }
            }
            PrintFunctionSwitchInfo();
        }

        //输出规则、已打开开关之间的对应关系，方便测试时定位开关相关问题
        private void PrintFunctionSwitchInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("规则中已打开的开关如下：");
            sb.Append("\n");
            foreach (var item in this.switchInfoList)
            {
                sb.Append(string.Format("开关:{0},对应应用Id:{1},对应规则Id:{2}", item.key, item.applicationId, item.ruleId));
                sb.Append("\n");
            }
            Logger.Log(sb.ToString(), PandoraSettings.SDKTag);
        }

        //isDebug和log_level每个规则单独存储和使用
        private void MergeLogConfig()
        {
            if (_isInternalCofnigValid)
            {
                LogConfig config = new LogConfig();
                config.tag = INTERNAL_LOG_TAG;
                config.isDebug = _internalPlatformConfig.isDebug;
                config.logLevel = _internalPlatformConfig.logLevel;
                _logConfigDict.Add(INTERNAL_LOG_TAG, config);
                this.isDebug = config.isDebug;
                this.logLevel = config.logLevel;

                foreach (var item in _internalPlatformConfig.assetInfoDict)
                {
                    string key = item.Key;
                    if (!_assetNameLogTagMap.ContainsKey(key))
                    {
                        _assetNameLogTagMap.Add(key, INTERNAL_LOG_TAG);
                    }
                }
            }

            if (_isUniformCofnigValid)
            {
                foreach (var item in uniformConfig.groupList)
                {
                    LogConfig config = new LogConfig();
                    if (item.typeName.Contains("SystemRuleInfo"))
                    {
                        SystemRuleInfo ruleInfo = item.ruleInstance as SystemRuleInfo;
                        if (ruleInfo.relationType == "3")
                        {
                            config.tag = ruleInfo.serviceKey;
                            this.isDebug = ruleInfo.functionSwitch.ContainsKey("isDebug") ? true : false;
                            this.logLevel = ruleInfo.logLevel;
                        }
                    }
                    else
                    {
                        config.tag = item.ruleInstance.id.ToString();
                    }
                    config.isDebug = item.ruleInstance.functionSwitch.ContainsKey("isDebug") ? true : false;
                    config.logLevel = item.ruleInstance.logLevel;
                    _logConfigDict.Add(config.tag, config);
                    foreach (var assetInfo in item.assetInfoList)
                    {
                        string key = assetInfo.name;
                        if (_assetNameLogTagMap.ContainsKey(key))
                        {
                            //v2规则中的替换v1中的
                            _assetNameLogTagMap[key] = config.tag;
                        }
                        else
                        {
                            _assetNameLogTagMap.Add(key, config.tag);
                        }
                    }
                }
            }
        }

        private void MergeAssetInfoDict()
        {
            this.assetInfoDict = new Dictionary<string, AssetInfo>();
            if (_isInternalCofnigValid)
            {
                foreach (var item in _internalPlatformConfig.assetInfoDict)
                {
                    this.assetInfoDict.Add(item.Key, item.Value);
                }
            }

            if (_isUniformCofnigValid)
            {
                foreach (var item in uniformConfig.groupList)
                {
                    foreach (var assetInfo in item.assetInfoList)
                    {
                        if (this.assetInfoDict.ContainsKey(assetInfo.name))
                        {
                            if (item.typeName.Contains("SystemRuleInfo"))
                            {
                                if (item.ruleInstance.relationType == "3")
                                {
                                    this.assetInfoDict[assetInfo.name] = assetInfo;
                                    continue;
                                }
                            }
                            _conflictAssetInfo.Add(assetInfo);
                            continue;
                        }
                        this.assetInfoDict.Add(assetInfo.name, assetInfo);
                    }
                }
            }
        }

        private void MergeAssetInfoListDict()
        {
            this.assetInfoListDict = new Dictionary<string, List<AssetInfo>>();
            if (_isInternalCofnigValid)
            {
                foreach (var item in _internalPlatformConfig.assetInfoListDict)
                {
                    string conflictAssetName = "";
                    if (IsGroupContainConflictAsset(item.Value, _conflictAssetInfo, out conflictAssetName) && item.Key != "frame")
                    {
                        ConflictGroupInfo conflict = new ConflictGroupInfo();
                        conflict.ruleId = _internalPlatformConfig.ruleId;
                        conflict.ruleVersion = "V1";
                        conflict.groupName = item.Key;
                        conflict.assetName = conflictAssetName;
                        _conflictGroupInfo.Add(conflict);
                    }
                    else
                    {
                        if (_isUniformCofnigValid)
                        {
                            ReplaceFrameGroup(item.Value, uniformConfig.systemRuleInfoList[0].assetInfoList);
                        }
                        this.assetInfoListDict.Add(item.Key.ToLower(), item.Value);
                    }
                }
            }

            if (_isUniformCofnigValid)
            {
                foreach (var item in uniformConfig.groupList)
                {
                    string conflictAssetName = "";
                    if (IsGroupContainConflictAsset(item.assetInfoList, _conflictAssetInfo, out conflictAssetName) && !item.typeName.Contains("SystemRuleInfo"))
                    {
                        ConflictGroupInfo conflict = new ConflictGroupInfo();
                        conflict.ruleId = item.ruleInstance.ruleId;
                        conflict.ruleVersion = "V2";
                        conflict.groupName = item.id.ToString();
                        conflict.assetName = conflictAssetName;
                        conflict.typeName = _relationTypeDict[item.ruleInstance.relationType];
                        _conflictGroupInfo.Add(conflict);
                    }
                    else
                    {
                        //系统组件的key必须为frame,V2版本覆盖V1版本
                        if (item.typeName.Contains("SystemRuleInfo") && item.ruleInstance.relationType == "3")
                        {
                            if (this.assetInfoListDict.ContainsKey("frame"))
                            {
                                this.assetInfoListDict["frame"] = item.assetInfoList;
                            }
                            else
                            {
                                this.assetInfoListDict.Add("frame", item.assetInfoList);
                            }
                            continue;
                        }
                        this.assetInfoListDict.Add(item.id.ToString(), item.assetInfoList);
                    }
                }
            }

            PrintConflictGroupInfo();
        }

        //将v1版本（internal）规则中资源组依赖的frame替换为v2版本（uniform）
        private void ReplaceFrameGroup(List<AssetInfo> internalAssetInfoList, List<AssetInfo> uniformFrameGroup)
        {
            for (int i = 0; i < internalAssetInfoList.Count; i++)
            {
                for (int j = 0; j < uniformFrameGroup.Count; j++)
                {
                    if (internalAssetInfoList[i].name == uniformFrameGroup[j].name)
                    {
                        internalAssetInfoList[i] = uniformFrameGroup[j];
                    }
                }
            }
        }
        private bool IsGroupContainConflictAsset(List<AssetInfo> groupAssetInfoList, List<AssetInfo> conflictAssetInfoList, out string assetName)
        {
            foreach (var item in groupAssetInfoList)
            {
                foreach (var innerItem in conflictAssetInfoList)
                {
                    if (item.name == innerItem.name)
                    {
                        assetName = item.name;
                        return true;
                    }
                }
            }
            assetName = string.Empty;
            return false;
        }

        private void PrintConflictGroupInfo()
        {
            foreach (var item in _conflictGroupInfo)
            {
                string error = string.Empty;
                if (item.ruleVersion == "V1")
                {
                    error = string.Format("{0}版本规则{1}中资源组 {2} 包含冲突资源 {3}，此资源组不会被加载", item.ruleVersion, item.ruleId, item.groupName, item.assetName);
                }
                else
                {
                    error = string.Format("{0}版本 {1} 中 id={2} ruleId={3} 包含冲突资源 {4}，此资源组不会被加载", item.ruleVersion, item.typeName, item.groupName, item.ruleId, item.assetName);
                }
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error, ErrorCode.ASSETBUNDLE_CONFLICT);
                DelegateAggregator.Report(error, ErrorCode.ASSETBUNDLE_CONFLICT_DETAIL, ErrorCode.TNM2_TYPE_LITERALS);
            }
        }

        public InternalPlatformRemoteConfig InterplatformConfig
        {
            get
            {
                return _internalPlatformConfig;
            }
        }

        public Dictionary<string, LogConfig> LogConfigDict
        {
            get
            {
                return _logConfigDict;
            }
        }

        public string GetLogTagForPxUI(string actionName)
        {
            string assetName;
            if (PandoraSettings.UseZipResForPixUI)
            {
                assetName = string.Format("{0}_bin.zip", actionName.ToLower());
            }
            else
            {
                assetName = string.Format("{0}_{1}_bin.assetbundle", PandoraSettings.GetPlatformDescription(), actionName.ToLower());
            }

            if (_assetNameLogTagMap.ContainsKey(assetName))
            {
                return _assetNameLogTagMap[assetName];
            }

            return "Undefined";
        }

        private void PrintInternalConfigInfo()
        {
            if (_isInternalCofnigValid)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("----------------V1 版本规则信息----------------\n");
                AppendString(sb, string.Format("规则id：{0}\n", _internalPlatformConfig.ruleId));
                AppendString(sb, string.Format("规则备注：{0}\n", _internalPlatformConfig.remark));
                AppendCommonRemoteConfig(sb, _internalPlatformConfig, "V1", 0);
                AppendFunctionSwitch(sb, _internalPlatformConfig.functionSwitchDict, 1);
                if (_internalPlatformConfig.assetInfoListDict.Count > 0)
                {
                    AppendString(sb, string.Format("资源组配置：\n"));
                    foreach (var item in _internalPlatformConfig.assetInfoListDict)
                    {
                        AppendString(sb, string.Format("{0}:\n", item.Key), 1);
                        AppendAssetInfoList(sb, item.Value, 2);
                    }
                }
                Logger.Log(sb.ToString(), PandoraSettings.SDKTag);
            }
        }

        private void PrintUniformConfigInfo()
        {
            if (_isUniformCofnigValid)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("----------------V2 版本规则信息----------------\n");
                AppendSystemRuleInfo(sb);
                AppendAppRuleInfo(sb);
                Logger.Log(sb.ToString(), PandoraSettings.SDKTag);
            }
        }

        private void AppendSystemRuleInfo(StringBuilder sb)
        {
            AppendString(sb, string.Format("系统组件配置：\n"));
            var systemRuleInfo = uniformConfig.systemRuleInfoList[0];
            AppendString(sb, string.Format("组件Id：{0}\n", systemRuleInfo.id), 1);
            AppendString(sb, string.Format("组件名称：{0}\n", systemRuleInfo.name), 1);
            AppendString(sb, string.Format("规则Id：{0}\n", systemRuleInfo.ruleId), 1);
            AppendString(sb, string.Format("规则备注：{0}\n", systemRuleInfo.remark), 1);
            AppendString(sb, string.Format("热更包id：{0}\n", systemRuleInfo.dirId), 1);
            bool debug = systemRuleInfo.functionSwitch.ContainsKey("isDebug") ? systemRuleInfo.functionSwitch["isDebug"] : false;
            AppendString(sb, string.Format("日志开关是否打开：{0}\n", debug), 1);
            if (debug)
            {
                AppendString(sb, string.Format("日志级别：{0}\n", systemRuleInfo.logLevel), 1);
            }
            AppendString(sb, string.Format("broker 域名：{0}\n", systemRuleInfo.ip), 1);
            AppendString(sb, string.Format("broker 端口：{0}\n", systemRuleInfo.port), 1);
            AppendString(sb, string.Format("broker 备份Ip1：{0}\n", systemRuleInfo.capIp1), 1);
            AppendString(sb, string.Format("broker 备份Ip2：{0}\n", systemRuleInfo.capIp2), 1);
            AppendFunctionSwitch(sb, systemRuleInfo.functionSwitch, 1);
            if (systemRuleInfo.assetInfoList.Count > 0)
            {
                AppendString(sb, string.Format("资源组配置:\n"), 1);
                AppendAssetInfoList(sb, systemRuleInfo.assetInfoList, 2);
            }
        }

        private void AppendAppRuleInfo(StringBuilder sb)
        {
            AppendString(sb, string.Format("普通组件及活动配置：\n"));
            foreach (var item in uniformConfig.appRuleInfoList)
            {
                AppendString(sb, string.Format("规则类型：{0}\n", _relationTypeDict[item.relationType]), 1);
                AppendString(sb, string.Format("组件或活动Id：{0}\n", item.id), 1);
                AppendString(sb, string.Format("组件或活动名称：{0}\n", item.name), 1);
                AppendString(sb, string.Format("规则Id：{0}\n", item.ruleId), 1);
                AppendString(sb, string.Format("规则备注：{0}\n", item.remark), 1);
                AppendString(sb, string.Format("热更包id：{0}\n", item.dirId), 1);
                bool debug = item.functionSwitch.ContainsKey("isDebug") ? item.functionSwitch["isDebug"] : false;
                AppendString(sb, string.Format("日志开关是否打开：{0}\n", debug), 1);
                if (debug)
                {
                    AppendString(sb, string.Format("日志级别：{0}\n", item.logLevel), 1);
                }
                AppendFunctionSwitch(sb, item.functionSwitch, 1);
                if (item.assetInfoList.Count > 0)
                {
                    AppendString(sb, string.Format("资源组配置:\n"), 1);
                    AppendAssetInfoList(sb, item.assetInfoList, 2);
                }
                AppendString(sb, "------------------------------------------------------------------------------------------------------------\n");
            }
        }

        private void AppendCommonRemoteConfig(StringBuilder sb, RemoteConfig config, string cgiVersion, int indentNum)
        {
            AppendString(sb, string.Format("日志开关是否打开：{0}\n", config.isDebug), indentNum);
            if (config.isDebug)
            {
                AppendString(sb, string.Format("日志级别：{0}\n", config.logLevel), indentNum);
            }
            AppendString(sb, string.Format("broker 域名：{0}\n", config.brokerHost), indentNum);
            AppendString(sb, string.Format("broker 端口：{0}\n", config.brokerPort), indentNum);

            string ip1 = string.Empty;
            string ip2 = string.Empty;
            if (cgiVersion == "V1")
            {
                ip1 = (config as InternalPlatformRemoteConfig).brokerAlternateIp1;
                ip2 = (config as InternalPlatformRemoteConfig).brokerAlternateIp2;
            }
            else
            {
                ip1 = (config as UniformRemoteConfig).brokerAlternateIp1;
                ip2 = (config as UniformRemoteConfig).brokerAlternateIp2;
            }
            AppendString(sb, string.Format("broker 备份Ip1：{0}\n", ip1), indentNum);
            AppendString(sb, string.Format("broker 备份Ip2：{0}\n", ip2), indentNum);
        }

        private void PrintMergedConfigInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("----------------合并后规则信息----------------\n");
            AppendString(sb, string.Format("V1版本规则是否合法：{0}",_isInternalCofnigValid), 0);
            AppendString(sb, string.Format("V2版本规则是否合法：{0}",_isUniformCofnigValid), 0);
            AppendCommonRemoteConfig(sb, this, "V2", 0);
            AppendFunctionSwitch(sb, this.functionSwitchDict, 0);
            if (this.assetInfoListDict.Count > 0)
            {
                AppendString(sb, string.Format("资源组配置：\n"));
                foreach (var item in this.assetInfoListDict)
                {
                    AppendString(sb, string.Format("{0}:\n", item.Key), 1);
                    AppendAssetInfoList(sb, item.Value, 2);
                }
            }
            Logger.Log(sb.ToString(), PandoraSettings.SDKTag);
        }

        private void AppendFunctionSwitch(StringBuilder sb, Dictionary<string, bool> functionSwitch, int indentNum)
        {
            if (functionSwitch.Count > 0)
            {
                AppendString(sb, string.Format("打开的开关：\n"), indentNum);
                foreach (var innerItem in functionSwitch)
                {
                    AppendString(sb, string.Format("{0}\n", innerItem.Key), indentNum + 1);
                }
            }
        }

        private void AppendAssetInfoList(StringBuilder sb, List<AssetInfo> assetInfoList, int indentNum)
        {
            foreach (var internalItem in assetInfoList)
            {
                AppendString(sb, string.Format("name:{0}\n", internalItem.name), indentNum);
                AppendString(sb, string.Format("url:{0}\n", internalItem.url), indentNum);
                AppendString(sb, string.Format("size:{0}\n", internalItem.size), indentNum);
                AppendString(sb, string.Format("md5:{0}\n", internalItem.md5), indentNum);
                AppendString(sb, "\n");
            }
        }

        private void AppendString(StringBuilder sb, string content, int indentNum = 0)
        {
            for (int i = 0; i < indentNum; i++)
            {
                sb.Append("\t");
            }
            sb.Append(content);
        }
    }

    /// <summary>
    /// 记录开关是由哪个应用的哪个规则打开的，方便定位开关打开或关闭引起的问题
    /// </summary>
    public class FunctionSwitchInfo
    {
        public string key;
        public int applicationId;
        public int ruleId;

        public FunctionSwitchInfo(string key, int applicationId, int ruleId)
        {
            this.key = key;
            this.applicationId = applicationId;
            this.ruleId = ruleId;
        }
    }

    public class CommonRuleInfo
    {
        public int ruleId;   // 请求到的规则id
        public string remark; //规则备注信息
        public int id;   //  标识id 
        public string name;   // 工具名称"frame"
        public string relationType;//1 业务，2 活动，3 系统组件，4普通组件
        public Dictionary<string, bool> functionSwitch;  // 功能开关
        public int logLevel;  // 日志等级 (0-ERROR / 1-WARNING / 2-INFO / 3-DEBUG)
        public string cdnPrefix;  // cdn 的前缀，http://down.game.qq.com/pandora/speedm/
        public int dirId; //一个热更包里的文件应该有相同的id
        public List<AssetInfo> assetInfoList = new List<AssetInfo>();
    }

    public class SystemRuleInfo : CommonRuleInfo
    {
        public string serviceKey;//给客户端日志打tag用,一般为frame
        public string ip;   //  broker域名，例"www.qq.com"
        public string ipv6;  //  broker域名，IPV6形式，例"www.qq.com"
        public int port;  //  端口 6688
        public string backupIp;  //  之前为WebSocket添加的备份域名，需确认是否还需要
        public string capIp1;  //  备份broker ip1
        public string capIp2;  //  备份broker ip2
        public string capIpV6_1;  //  备份broker IPV6 ip1
        public string capIpV6_2;  //  备份broker IPV6 ip2 
    }

    public class AppRuleInfo : CommonRuleInfo
    {
    }

    //描述一组资源,使用packageId作为唯一索引
    public class UniformGroupInfo
    {
        public int dirId; //热更包的id，跟sourceList中的dir_id含义一样，用于排序
        public int id; //组id
        public List<AssetInfo> assetInfoList;//热更文件列表，内容为sourceList中的内容
        public string typeName;//类型名,根据类型名，将此对象转化为对应类型
        public CommonRuleInfo ruleInstance;
    }

    public class UniformConfig
    {
        public List<SystemRuleInfo> systemRuleInfoList = new List<SystemRuleInfo>();
        public List<AppRuleInfo> appRuleInfoList = new List<AppRuleInfo>();
        public List<int> loadSort;
        public List<UniformGroupInfo> groupList = new List<UniformGroupInfo>();
    }

    public class ConflictGroupInfo
    {
        public int ruleId;
        public string ruleVersion;
        public string groupName;
        public string assetName;
        public string typeName = "";
    }
}

