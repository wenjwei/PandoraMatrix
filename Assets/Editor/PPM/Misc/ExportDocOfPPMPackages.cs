using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PPM;
using System.Reflection;
using System;
using System.IO;
using System.Text;

namespace PPMUtils
{
    public class ExportDocOfPPMPackages
    {
        public class PackageInfo
        {
            public string name;
            public string author;
            public string type;
            public string tag;
            public string createTime;
            public string description;
            public List<PackageHistory> versionHistory;

            public PackageInfo(Package package)
            {
                this.name = package.name;
                this.author = package.author;
                this.type = package.type;
                this.createTime = package.createTime;
                this.description = package.description;
                if (package.tag != null && package.tag.Count > 0)
                {
                    this.tag = package.tag[0];
                }
            }
        }

        public class PackageHistory
        {
            public string version;
            public string changeLog;
            public PackageHistory(string version, string changeLog)
            {
                this.version = version;
                this.changeLog = changeLog;
            }
        }


        private static List<PackageInfo> packageInfoList = new List<PackageInfo>();
        private static DataPackageList dataPackageList;
        private static int currentRequestPackageIndex = 0;
        private static int packageNum = 0;

        public static void ExportDoc()
        {
            packageInfoList.Clear();
            Requestor.GetPackageList("DESC", "", 1, 500, OnGetPackageList);
        }

        private static void OnGetPackageList(DataPackageList resp)
        {
            dataPackageList = resp;
            currentRequestPackageIndex = 0;
            packageNum = resp.totalNum;

            packageInfoList.Add(new PackageInfo(dataPackageList.packages[currentRequestPackageIndex]));
            Requestor.GetPackageDetail(dataPackageList.packages[currentRequestPackageIndex].name, OnGetPackageDetail);
        }

        private static void OnGetPackageDetail(DataPackageDetail resp)
        {
            List<PackageHistory> packageHistory = new List<PackageHistory>();
            for (int i = 0; i < resp.packages.Count; i++)
            {
                PackageItem item = resp.packages[i];
                packageHistory.Add(new PackageHistory(item.version, item.changeLog));
            }

            packageInfoList[currentRequestPackageIndex].versionHistory = packageHistory;
            currentRequestPackageIndex++;
            if (currentRequestPackageIndex < packageNum)
            {
                //继续拉下一个包的详情
                packageInfoList.Add(new PackageInfo(dataPackageList.packages[currentRequestPackageIndex]));
                Requestor.GetPackageDetail(dataPackageList.packages[currentRequestPackageIndex].name, OnGetPackageDetail);
            }
            else
            {
                GenerateDoc();
            }
        }

        private static void GenerateDoc()
        {
            StringBuilder sb = new StringBuilder();
            //目录+标题
            sb.Append("[TOC]\n# PPM Pacakages 总览\n");
            PackageInfo info;
            for (int i = 0; i < packageNum; i++)
            {
                info = packageInfoList[i];
                sb.Append(string.Format("## {0}.{1}\n", i + 1, info.name));
                sb.Append(string.Format("- **{0}**\n  {1}\n", "作者", info.author));
                sb.Append(string.Format("- **{0}**\n  {1}\n", "分类", info.type));
                sb.Append(string.Format("- **{0}**\n  {1}\n", "标签", info.tag));
                sb.Append(string.Format("- **{0}**\n  {1}\n", "最新发布时间", info.createTime));
                AppendDescription(ref sb, info.description);
                AppendVersionHistory(ref sb, info.versionHistory);
            }
            WriteToFile(sb.ToString());
        }

        private static void AppendDescription(ref StringBuilder sb, string description)
        {
            sb.Append(string.Format("- **{0}**\n", "描述"));
            AppendLongText(ref sb, description);
        }

        private static void AppendVersionHistory(ref StringBuilder sb, List<PackageHistory> packageHistory)
        {
            sb.Append(string.Format("- **{0}**\n", "版本历史"));
            for (int i = 0; i < packageHistory.Count; i++)
            {
                sb.Append(string.Format("   - {0}\n", packageHistory[i].version));
                AppendLongText(ref sb, packageHistory[i].changeLog);
            }
        }

        private static void AppendLongText(ref StringBuilder sb, string text)
        {
            string[] splited = text.Split('\n');
            for (int i = 0; i < splited.Length; i++)
            {
                sb.Append(string.Format("  {0}\n", splited[i].Trim().Replace("\r", "")));
            }
        }

        private static void WriteToFile(string content)
        {
            string path = Path.Combine(Application.dataPath, "../PPM_Package_Doc.md");
            try
            {
                File.WriteAllText(path, content);
                Debug.Log("Export Doc Of PPMPackages done!");
            }
            catch (Exception e)
            {
                Debug.Log("write file failed!\n stack trace:" + e.StackTrace);
            }
        }

    }
}