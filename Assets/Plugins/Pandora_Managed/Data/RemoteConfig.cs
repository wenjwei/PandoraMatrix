using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace com.tencent.pandora
{
    public abstract class RemoteConfig
    {
        /// <summary>
        /// 总量开关
        /// </summary>
        public bool totalSwitch;
        /// 是否要将Log保存在本地
        /// </summary>
        public bool isDebug;
        /// <summary>
        /// 上报日志的等级
        /// </summary>
        public int logLevel;
        public string brokerHost;
        public int brokerPort;

        /// <summary>
        /// 功能开关字典
        /// </summary>
        public Dictionary<string, bool> functionSwitchDict;
        /// <summary>
        /// 资源配置
        /// </summary>
        public Dictionary<string, List<AssetInfo>> assetInfoListDict;
        public Dictionary<string, AssetInfo> assetInfoDict;
        /// <summary>
        /// 资源组的优先级，如果多次加载失败则也认为是成功
        /// </summary>
        public Dictionary<string, GroupInfo> groupFileDict;

        /// <summary>
        /// 需要删除的热更新文件组
        /// </summary>
        public List<GroupInfo> deleteGroupInfoList = new List<GroupInfo>();
        /// <summary>
        /// 返回数据内容为空
        /// </summary>
        public bool IsEmpty { get; set; }

        public bool IsError { get; set; }

        protected abstract void Parse(string response);

        public bool GetFunctionSwitch(string functionName)
        {
            if (functionSwitchDict != null && functionSwitchDict.ContainsKey(functionName))
            {
                return functionSwitchDict[functionName];
            }
            return false;
        }

        public string GetAssetPath(string assetName)
        {
            if (assetInfoDict.ContainsKey(assetName) == true)
            {
                return assetInfoDict[assetName].url;
            }
            return string.Empty;
        }

        public AssetInfo GetAssetInfo(string name)
        {
            if (assetInfoDict.ContainsKey(name) == true)
            {
                return assetInfoDict[name];
            }
            return null;
        }

        protected void PostProcessAssetInfoListDict(Dictionary<string, List<AssetInfo>> assetInfoListDict)
        {
            int size = IntPtr.Size;//由系统指针的size获得当前运行程序的平台是32位还是64位，size等于4时为32位，size等于8时为64位
            string removeToken = (size == 4) ? "lua64" : "lua32";
            foreach (string key in assetInfoListDict.Keys)
            {
                List<AssetInfo> assetInfoList = assetInfoListDict[key];
                for (int i = assetInfoList.Count - 1; i >= 0; i--)
                {
                    AssetInfo assetInfo = assetInfoList[i];
                    if (assetInfo.url.Contains(removeToken) == true)
                    {
                        assetInfoList.RemoveAt(i);
                    }
                }
            }
        }

        protected int ParseIntField(Dictionary<string, System.Object> content, string fieldName, int defaultValue = -1)
        {
            try
            {
                int result = defaultValue;
                if (content.ContainsKey(fieldName) == true && !string.IsNullOrEmpty(content[fieldName].ToString()))
                {
                    result = Convert.ToInt32(content[fieldName]);
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("{0}解析出错，{1}\n{2}", fieldName, e.Message, e.StackTrace), PandoraSettings.SDKTag);
            }
            return defaultValue;
        }

        public ushort ParseUShortField(Dictionary<string, System.Object> content, string fieldName, ushort defaultValue = 0)
        {
            try
            {
                ushort result = defaultValue;
                if (content.ContainsKey(fieldName) == true)
                {
                    result = Convert.ToUInt16(content[fieldName]);
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("{0}解析出错，{1}\n{2}", fieldName, e.Message, e.StackTrace), PandoraSettings.SDKTag);
            }
            return defaultValue;
        }

        protected string ParseStringField(Dictionary<string, System.Object> content, string fieldName)
        {
            string result = string.Empty;
            if (content.ContainsKey(fieldName) == true)
            {
                result = content[fieldName] as string;
            }
            return result;
        }

        protected Dictionary<string, bool> ParseFuntionSwitch(Dictionary<string, System.Object> content)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            string switchContent = ParseStringField(content, "function_switch");
            if (string.IsNullOrEmpty(switchContent) == false)
            {
                string[] switchItems = switchContent.Split(',');
                for (int i = 0; i < switchItems.Length; i++)
                {
                    string item = switchItems[i];
                    string[] segments = item.Split(':');
                    if (segments.Length == 2)
                    {
                        string key = segments[0];
                        int value = System.Convert.ToInt32(segments[1]);
                        //只把开关状态为打开的加入，方便开放平台多规则开关合并
                        if (value == 1)
                        {
                            result[key] = true;
                        }
                    }
                }
            }
            return result;
        }

        protected Dictionary<string, GroupInfo> ParseGroupFileListDict(Dictionary<string, System.Object> content, out bool hasError)
        {

            hasError = false;
            Dictionary<string, GroupInfo> result = new Dictionary<string, GroupInfo>();
            if (content.ContainsKey("sourcelist") == false)
            {
                Logger.LogError("文件列表配置错误，未发现sourcelist字段", PandoraSettings.SDKTag);
                hasError = true;
                return result;
            }
            Dictionary<string, System.Object> sourceList = content["sourcelist"] as Dictionary<string, System.Object>;
            if (sourceList == null || sourceList.ContainsKey("origin") == false)
            {
                Logger.LogError("文件列表配置错误，sourcelist字段中origin不存在", PandoraSettings.SDKTag);
                hasError = true;
                return result;
            }

            Dictionary<string, System.Object> originDict = sourceList["origin"] as Dictionary<string, System.Object>;
            int i = 0;
            foreach (var item in originDict)
            {
                Dictionary<string, System.Object> groupInfoDict = item.Value as Dictionary<string, System.Object>;
                GroupInfo groupInfo = new GroupInfo();
                groupInfo.name = groupInfoDict["packagename"] as string;

                groupInfo.desc = groupInfoDict["desc"] as string;
                groupInfo.sortIndex = i++;
                int isBase = ParseIntField(groupInfoDict, "base", 0);
                groupInfo.isBase = (bool)(isBase == 1);
                if (groupInfo.isBase == false)
                {
                    groupInfo.order = ParseIntField(groupInfoDict, "order", 100);
                }

                int version = 100;
                if (int.TryParse(item.Key, out version))
                {
                    groupInfo.version = version;
                }
                if (groupInfoDict.ContainsKey("list"))
                {
                    List<System.Object> fileMd5List = groupInfoDict["list"] as List<System.Object>;
                    foreach (var fileMd5item in fileMd5List)
                    {
                        string fileMd5 = fileMd5item as string;
                        //待确认：同一版本的一个文件被包含到两个热更文件夹时，只有一个组加到result中,最终导致冲突的两个组只删除一个
                        if (result.ContainsKey(fileMd5) == false)
                        {
                            result.Add(fileMd5, groupInfo);
                        }
                        if (groupInfo.md5List.Contains(fileMd5) == false)
                        {
                            groupInfo.md5List.Add(fileMd5);
                        }
                    }
                }
            }
            return result;
        }

        protected Dictionary<string, AssetInfo> ParseAssetInfoDict(Dictionary<string, System.Object> content, out bool hasError)
        {
            hasError = false;
            Dictionary<string, AssetInfo> result = new Dictionary<string, AssetInfo>();
            if (content.ContainsKey("sourcelist") == false)
            {
                Logger.LogError("文件列表配置错误，未发现sourcelist字段", PandoraSettings.SDKTag);
                hasError = true;
                return result;
            }
            Dictionary<string, System.Object> sourceList = content["sourcelist"] as Dictionary<string, System.Object>;
            if (sourceList == null || sourceList.ContainsKey("count") == false || sourceList.ContainsKey("list") == false)
            {
                Logger.LogError("文件列表配置错误，sourcelist字段中count或list不存在", PandoraSettings.SDKTag);
                hasError = true;
                return result;
            }

            int fileCount = ParseIntField(sourceList, "count");
            List<System.Object> fileMetaList = sourceList["list"] as List<System.Object>;
            if (fileMetaList == null || fileCount != fileMetaList.Count)
            {
                Logger.LogError("文件列表配置错误，sourceList字段中list为null或sourcelist字段中count值与list内容长度值不一致", PandoraSettings.SDKTag);
                hasError = true;
                return result;
            }
            for (int i = 0; i < fileMetaList.Count; i++)
            {
                System.Object obj = fileMetaList[i];
                Dictionary<string, System.Object> meta = obj as Dictionary<string, System.Object>;
                if (meta.ContainsKey("url") == false || meta.ContainsKey("luacmd5") == false || meta.ContainsKey("size") == false)
                {
                    string error = "文件列表配置错误，sourcelist.list某一个文件的url，luacmd5，size字段不存在";
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.ReportError(error);
                    hasError = true;
                    return result;
                }

                AssetInfo fileInfo = new AssetInfo();
                fileInfo.url = meta["url"] as string;
                string size = meta["size"] as string;
                if (size == null)
                {
                    fileInfo.size = (int)(long)meta["size"];
                }
                else
                {
                    fileInfo.size = int.Parse(size);
                }

                fileInfo.md5 = meta["luacmd5"] as string;
                string name = Path.GetFileName(fileInfo.url);
                if (name.Contains("?"))
                {
                    fileInfo.name = name.Substring(0, name.IndexOf('?'));
                }
                else
                {
                    fileInfo.name = name;
                }

                if (string.IsNullOrEmpty(fileInfo.name) || string.IsNullOrEmpty(fileInfo.md5) || fileInfo.size <= 0 || string.IsNullOrEmpty(fileInfo.url))
                {
                    string error = "文件列表配置错误，sourcelist.list某一个文件的url，luacmd5，size字段内容不正确";
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.ReportError(error);
                    hasError = true;
                    return result;
                }

                if (result.ContainsKey(fileInfo.name) == true)
                {
                    GroupInfo conflictGroupInfo = groupFileDict[result[fileInfo.name].md5];
                    string error = string.Format("文件列表配置错误，sourcelist.list存在同名的资源配置信息, 当前冲突热更新包名称:{0}(version:{1}),被冲突热更新包名称:{2}(version:{3})，文件名:{4}", groupFileDict[fileInfo.md5].desc, groupFileDict[fileInfo.md5].version, conflictGroupInfo.desc, conflictGroupInfo.version, fileInfo.name);
                    DelegateAggregator.ReportError(error, ErrorCode.ASSETBUNDLE_CONFLICT);
                    DelegateAggregator.Report(error, ErrorCode.ASSETBUNDLE_CONFLICT_DETAIL, ErrorCode.TNM2_TYPE_LITERALS);
                    if (deleteGroupInfoList.Contains(groupFileDict[fileInfo.md5]) == false)
                    {
                        deleteGroupInfoList.Add(groupFileDict[fileInfo.md5]);
                    }

                    if (!conflictGroupInfo.isBase && deleteGroupInfoList.Contains(conflictGroupInfo) == false)
                    {
                        deleteGroupInfoList.Add(conflictGroupInfo);
                    }
                }
                else
                {
                    result.Add(fileInfo.name, fileInfo);
                }
            }
            return result;
        }

        protected Dictionary<string, List<AssetInfo>> ParseAssetInfoListDict(Dictionary<string, System.Object> content, Dictionary<string, AssetInfo> fileInfoDict, out bool hasError, Dictionary<string, List<AssetInfo>> localAssetInfoListDict = null)
        {
            hasError = false;
            Dictionary<string, List<AssetInfo>> result = new Dictionary<string, List<AssetInfo>>();
            string dependency = ParseStringField(content, "dependency");
            if (string.IsNullOrEmpty(dependency))
            {
                string error = "依赖文件列表字段dependecy内容不存在或内容为空";
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error);
                hasError = true;
                return result;
            }
            string[] groups = dependency.Split('|');
            for (int i = 0; i < groups.Length; i++)
            {
                string group = groups[i];
                string[] segments = group.Split(':');
                if (segments.Length != 2)
                {
                    string error = "依赖文件列表字段dependecy内容中Group的内容格式配置错误";
                    Logger.LogError(error, PandoraSettings.SDKTag);
                    DelegateAggregator.ReportError(error);
                    hasError = true;
                    return result;
                }
                string groupName = segments[0].ToLower();
                if (deleteGroupInfoList.Find(x => x.name.Equals(groupName)) != null)
                {
                    continue;
                }
                string groupValue = segments[1];
                groupValue = groupValue.TrimEnd(',');
                string[] fileNames = groupValue.Split(',');
                List<AssetInfo> fileInfoList = new List<AssetInfo>();
                for (int j = 0; j < fileNames.Length; j++)
                {
                    string name = fileNames[j];
                    if (name.StartsWith("@")) //依赖其他group
                    {
                        string otherGroupName = name.Substring(1).ToLower();
                        if (result.ContainsKey(otherGroupName) == false)
                        {
                            //如果本地资源中有，则在此加入
                            if (localAssetInfoListDict != null && localAssetInfoListDict.ContainsKey(otherGroupName))
                            {
                                result.Add(otherGroupName, localAssetInfoListDict[otherGroupName]);
                            }
                            else
                            {
                                string error = string.Format("依赖文件列表字段dependecy内容中关于其他Group的依赖配置错误，被依赖的Group {0}需要配置在{1}右边", otherGroupName, groupName);
                                Logger.LogError(error, PandoraSettings.SDKTag);
                                DelegateAggregator.ReportError(error);
                                hasError = true;
                                return result;
                            }
                        }
                        //把依赖项要放到列表前部，执行lua时优先执行
                        fileInfoList.InsertRange(0, result[otherGroupName]);
                    }
                    else
                    {
                        if (fileInfoDict.ContainsKey(name) == false)
                        {
                            string error = string.Format("依赖文件列表字段dependecy内容中的文件名{0}在sourcelist中不存在", name);
                            Logger.LogError(error, PandoraSettings.SDKTag);
                            DelegateAggregator.ReportError(error);
                            hasError = true;
                            return result;
                        }
                        fileInfoList.Add(fileInfoDict[name]);
                    }
                }

                result.Add(groupName, fileInfoList);
            }

            //检查本地资源是否有未加入的项
            if (localAssetInfoListDict != null)
            {
                foreach (var item in localAssetInfoListDict)
                {
                    if (!result.ContainsKey(item.Key))
                    {
                        result.Add(item.Key, item.Value);
                        Logger.Log(string.Format("本地资源组{0}加到云端资源配置中", item.Key), PandoraSettings.SDKTag);
                    }
                }
            }
            return result;
        }
        //管理端的日志级别转换为客户端的类型
        public int ConvertLogLevel(int remoteConfigLogLevel)
        {
            if (remoteConfigLogLevel >= 3)
            {
                return Logger.ERROR;
            }
            else if (remoteConfigLogLevel >= 2)
            {
                return Logger.WARNING;
            }
            else if (remoteConfigLogLevel >= 1)
            {
                return Logger.INFO;
            }
            else
            {
                return Logger.DEBUG;
            }
        }

        //先将本地资源加到资源组中，如果规则中里有配置，则使用配置的，便于紧急更新，本地资源放在StreamingAssets文件夹下。当前情况下，本地资源先加到内部平台处理逻辑中，后续抛弃内部平台时迁移到开放平台逻辑中
        public void ConfigLocalAsset(ref Dictionary<string, AssetInfo> localDict, ref Dictionary<string, List<AssetInfo>> localListDict)
        {
            //确定哪些资源需要放在本地，以下为示例
            Dictionary<string, List<ProgramAsset>> localAsset = new Dictionary<string, List<ProgramAsset>>();
            //List<ProgramAsset> frameList = new List<ProgramAsset>() {
            //    new ProgramAsset(){name = "frame",type=AssetType.Lua},
            //};

            //List<ProgramAsset> pxFrameList = new List<ProgramAsset>() {
            //    new ProgramAsset() { name = "pxframe", type = AssetType.Lua },
            //    new ProgramAsset() { name = "pxframe", type = AssetType.Prefab },
            //    new ProgramAsset() { name = "nguimask", type = AssetType.Prefab },
            //};

            //localAsset.Add("frame", frameList);
            //localAsset.Add("pxFrame", pxFrameList);

            ConfigLocalAsset(localAsset, ref localDict, ref localListDict);
        }

        public void ConfigLocalAsset(Dictionary<string, List<ProgramAsset>> localAsset, ref Dictionary<string, AssetInfo> localDict, ref Dictionary<string, List<AssetInfo>> localListDict)
        {
            foreach (var item in localAsset)
            {
                var assetInfoList = GetLocalAssetInfoList(item.Value);
                foreach (var childItem in assetInfoList)
                {
                    string name = childItem.name;
                    if (!localDict.ContainsKey(name))
                    {
                        localDict.Add(name, childItem);
                    }
                    else
                    {
                        Logger.LogWarning("本地资源配置重复:" + name, PandoraSettings.SDKTag);
                    }
                }

                //组名使用小写形式
                string groupName = item.Key.ToLower();
                if (!localListDict.ContainsKey(groupName))
                {
                    localListDict.Add(groupName, assetInfoList);
                }
                else
                {
                    Logger.LogWarning("本地资源组配置重复：" + groupName, PandoraSettings.SDKTag);
                }
            }
        }

        public List<AssetInfo> GetLocalAssetInfoList(List<ProgramAsset> localProgramList)
        {
            List<AssetInfo> assetInfoList = new List<AssetInfo>();
            foreach (var item in localProgramList)
            {
                string assetPath = GenerateProgramAssetPath(item.name, item.type);
                AssetInfo assetInfo = new AssetInfo();
                assetInfo.name = Path.GetFileName(assetPath);
                assetInfo.url = assetPath;
                assetInfo.isDownloadFromLocal = true;
                assetInfoList.Add(assetInfo);
            }
            return assetInfoList;
        }

        private string GenerateProgramAssetPath(string name, AssetType type)
        {
            int size = IntPtr.Size;//由系统指针的size获得当前运行程序的平台是32位还是64位，size等于4时为32位，size等于8时为64位
            string luaSuffix = (size == 4) ? "_lua32" : "_lua64";
            bool isLua = (type == AssetType.Lua);
            string fileName = string.Format("{0}_{1}{2}.assetbundle", PandoraSettings.GetPlatformDescription(), name.ToLower(), isLua ? luaSuffix : "");
            string filePath = Path.Combine(LocalDirectoryHelper.GetStreamingAssetsUrl(), fileName);

#if UNITY_ANDROID && !UNITY_EDITOR
            //安卓平台下要去除file协议头
           filePath = filePath.Remove(0, PandoraSettings.GetFileProtocolToken().Length);
#endif
            Logger.Log("local program asset path:" + filePath, PandoraSettings.SDKTag);
            return filePath;
        }

        public class ProgramAsset
        {
            public string name;
            public AssetType type;
        }
        /// <summary>
        /// 文件配置信息
        /// </summary>
        public class AssetInfo
        {
            public string name;
            public string url;
            public string md5;
            public int size;
            public bool isDownloaded = false; //标记资源是否已经被加载过，防止重入
            public bool isDownloadFromLocal = false;//标记是否是内置资源

            public override string ToString()
            {
                return name + " " + url;
            }
        }
    }
    public enum GroupStatus
    {
        Not_Load = 0,
        Download_Fail,
        Download_Succ,
        Load_Lua
    }

    /// <summary>
    /// 热更新包信息
    /// </summary>
    public class GroupInfo : IComparable
    {
        public string name;
        public string desc;
        public int order = -1;
        public int version;
        public bool isBase = false;
        public List<string> md5List = new List<string>();
        public int sortIndex;  // 用来进行稳定排序
        public GroupStatus isLoaded = GroupStatus.Not_Load; //标记资源是否已经标记加载，重连多次也认为已加载完成

        public override string ToString()
        {
            return name + " " + desc + " " + order + " " + version;
        }

        public int CompareTo(object obj)
        {
            int result = 1;
            if (obj != null && obj is GroupInfo)
            {
                var groupInfo = (GroupInfo)obj;
                result = this.order.CompareTo(groupInfo.order);

                if (result == 0)
                    result = this.sortIndex.CompareTo(groupInfo.sortIndex);
            }
            return result;
        }
    }
}


/*Config 示例：
  {
   "accesstoken" : "",
   "acctype" : "qq",
   "act_switch" : "0",
   "appid" : "1105647895",
   "areaid" : "2",
   "buyType" : "0",
   "cap_ip1" : "182.254.42.116",
   "cap_ip2" : "182.254.88.158",
   "dependency" : "core:android_frame_lua.assetbundle,android_pop_lua.assetbundle,android_pop.assetbundle|notice:android_frame_lua.assetbundle,android_notice_lua.assetbundle,android_notice.assetbundle",
   "errmsg" : "totalSwitch Control",
   "fakeLoginInfo" : "",
   "flag" : "0",
   "function_switch" : "patface:1,notice:1",
   "gameappversion" : "1.3.0.0",
   "id" : "759",
   "ip" : "she.broker.tplay.qq.com",
   "isDebug" : "0",
   "isNetLog" : "0",
   "log_level" : "3",
   "lua_cdnurl" : "",
   "lua_newversion" : "",
   "luac_s_md5" : "",
   "luaversion" : "",
   "lucky_switch" : "0",
   "md5" : "",
   "mds_op" : "0",
   "openid" : "ACB1E569C331456C1E93D061CF3383C2",
   "partitionid" : "1",
   "platid" : "1",
   "port" : "15692",
   "private_whitelist" : "",
   "punchface_switch" : "0",
   "rate" : "0",
   "remark" : "",
   "representmap" : "",
   "ret" : "0",
   "sdkversion" : "Snake-Android-V1",
   "serverip" : "10.193.9.117",
   "sort" : "B",
   "sourcelist" : {
      "count" : 5,
      "list" : [
         {
            "extend" : "",
            "luacmd5" : "2CCB1A07D874A04743EB2EE7445681B7",
            "sid" : 1300,
            "size" : 21841,
            "url" : "http://down.qq.com/yxgw/she/20161112185503/android_frame_lua.assetbundle",
            "zipmd5" : ""
         },
         {
            "extend" : "",
            "luacmd5" : "82350EFBEF41B363CADAE7EEFED0DAB3",
            "sid" : 1301,
            "size" : 64172,
            "url" : "http://down.qq.com/yxgw/she/20161115233437/android_notice.assetbundle",
            "zipmd5" : ""
         },
         {
            "extend" : "",
            "luacmd5" : "553653164D64E7AF7D7F9260D2ACF942",
            "sid" : 1302,
            "size" : 8796,
            "url" : "http://down.qq.com/yxgw/she/20161115233437/android_notice_lua.assetbundle",
            "zipmd5" : ""
         },
         {
            "extend" : "",
            "luacmd5" : "F081B5B7D4326CC5CD3871FDDBB821E6",
            "sid" : 1303,
            "size" : 10502,
            "url" : "http://down.qq.com/yxgw/she/20161115233437/android_pop.assetbundle",
            "zipmd5" : ""
         },
         {
            "extend" : "",
            "luacmd5" : "9CA68B1AF591634E309DA69D7DA82D3B",
            "sid" : 1304,
            "size" : 6523,
            "url" : "http://down.qq.com/yxgw/she/20161115233438/android_pop_lua.assetbundle",
            "zipmd5" : ""
         }
      ]
   },
   "totalSwitch" : "1",
   "zip_md5" : ""
}
 * */
