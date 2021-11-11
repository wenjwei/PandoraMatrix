using System.Collections.Generic;

namespace PPMUtils
{
    public class ResponseStatus
    {
        public string content;
        public int ret;
        public string msg;
    }

    #region Package

    public class ServerInfo
    {
        public string server;
    }

    /// <summary>
    /// method=getAllPackages
    /// </summary>
    public class DataPackageList
    {
        public int page;
        public int pageSize;
        public int totalNum;
        public int totalPage;
        public string orderFiled;
        public string order;
        public List<Package> packages;
    }

    public class Package
    {
        public string name;
        public string version;
        public string url;
        public string author;
        public string type;
        public string createTime;
        public string description;
        public float score;
        public List<string> tag;
        public List<DependencePackageItem> dependence;
    }

    public class DependencePackageItem
    {
        public string packageName;
        public string version;
        public string url;
    }

    public class PackageItem
    {
        public string version;
        public string url;
        public string changeLog;
        // 1=发布，0=未发布
        public int publishState;
    }

    /// <summary>
    /// method=getSpecifiedPackage
    /// </summary>
    public class DataPackageDetail
    {
        public string type;
        public List<string> tag;
        public List<DependencePackageItem> dependence;
        public List<PackageItem> packages;
    }

    /// <summary>
    /// method=getOwnPackages
    /// </summary>
    public class DataOwnPackages
    {
        public string type;
        public List<Plugin> plugins;
    }

    public class Plugin
    {
        public string name;
        public string type;
        public string description;
        public float score;
        public List<string> tag;
        public List<PackageItem> packages;
    }

    #endregion
}