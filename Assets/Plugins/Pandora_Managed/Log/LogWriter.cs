using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace com.tencent.pandora
{
    /// <summary>
    /// 将Log写入本地
    /// 可以在管理端单配规则，来调试线上发生的问题
    /// </summary>
    public class LogWriter : ILogRecorder
    {
        private string _currentLogPath;
        private Dictionary<int, string> _logLevelTag = new Dictionary<int, string>()
        {
            {0,"Error" },
            {1,"Warning" },
            {2,"Info" },
            {3,"Debug" },
        };

        public void Add(Log log)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_logLevelTag[log.level]);
            sb.Append("\t");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            sb.Append("\t");
            sb.Append(log.message);
            sb.Append("\n");
            sb.Append(log.stackTrace);
            Write(sb.ToString());
        }

        private void Write(string content)
        {
            try
            {
                string path = GetLogFilePath();
                if (path != _currentLogPath)
                {
                    _currentLogPath = path;
                }

                //为测试方便先不对日志加密
                //if (PandoraSettings.IsProductEnvironment)
                //{
                //    content = EncryptionHelper.EncryptDES(content);
                //}
                File.AppendAllText(_currentLogPath, content+"\n");
            }
            catch
            {
                //left blank
            }
        }

        private string GetLogFilePath()
        {
            return LocalDirectoryHelper.GetLogFolderPath() + "/log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        }

        public void Dispose()
        {
        }
        //正式环境落地日志加密处理
    }
}
