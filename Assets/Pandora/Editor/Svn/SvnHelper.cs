using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;
using UnityEngine;
using UnityEditor;


namespace com.tencent.pandora.tools
{
    public class SvnHelper
    {
        public static bool IS_USING_SVN = false;
        public const string COMMIT_ERROR = "COMMIT_ERROR";

        private static string ExecuteCommand(string args)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = GetSvnPath();
            info.Arguments = args;
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            //info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForInputIdle(10);
            return output.Trim();
        }

        private static string GetSvnPath()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\TortoiseSVN");
            if(key == null)
            {
                return string.Empty;
            }
            string directory = key.GetValue("Directory") as string;
            if(string.IsNullOrEmpty(directory))
            {
                return string.Empty;
            }
            return Path.Combine(directory, Path.Combine("bin", "svn.exe"));
        }

        public static bool IsSvnAvaliable()
        {
            return string.IsNullOrEmpty(GetSvnPath()) == false;
        }

        public static string GetLocalFileRevision(string localPath)
        {
            return GetFileRevision(localPath);
        }

        public static string GetRemoteFileRevision(string localPath)
        {
            string url = GetRemoteFileUrl(localPath);
            if(string.IsNullOrEmpty(url) == false)
            {
                return GetFileRevision(url);
            }
            return string.Empty;
        }

        //获取本地文件在Svn服务器上对应的URL
        private static string GetRemoteFileUrl(string localPath)
        {
            return GetSvnInfoFieldContent(localPath, "URL:");
        }

        private static string GetFileRevision(string path)
        {
            return GetSvnInfoFieldContent(path, "Last Changed Rev:");
        }

        /// <summary>
        /// 以本地文件Path执行Svn Info命令，提取其中的某些字段值
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static string GetSvnInfoFieldContent(string localPath, string token)
        {
            string args = string.Format("info \"{0}\"", localPath);
            string output = ExecuteCommand(args);
            string[] lineArr = output.Split('\n');
            foreach (string line in lineArr)
            {
                if (line.StartsWith(token) == true)
                {
                    return line.Substring(token.Length).Trim();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 自动提交SVN，可能的三个返回结果：1.提交成功，并返回最新版本号，2.已经是最新，返回空，3.提交错误
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static string Commit(string localPath)
        {
            string args = string.Format("ci -m\"【打包前自动提交】\" {0}", localPath);
            string output = ExecuteCommand(args);
            string[] lineArr = output.Split('\n');
            if(lineArr.Length == 1 && string.IsNullOrEmpty(lineArr[0].Trim()) == true)
            {
                return string.Empty;
            }
            string token = "Committed revision ";
            foreach (string line in lineArr)
            {
                if (line.StartsWith(token) == true)
                {
                    return line.Substring(token.Length).Trim();
                }
            }
            return COMMIT_ERROR;
        }
    }
}