using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace com.tencent.pandora
{
    /// <summary>
    /// 3种Log
    /// 1.Unity Console
    /// 2.屏幕GUI显示
    /// 3.本地Log文件
    /// </summary>
    public class LogHook : MonoBehaviour
    {
        private const string PANDORA_NAMESPACE = "com.tencent.pandora";
        private List<ILogRecorder> _recorderList = new List<ILogRecorder>();
        private bool _localLogSetting = false;
        private bool _isWriterAdded;
        private bool _isConsoleAdded;
        private bool _isErrorAdded;
        private bool _isHandLogInit;

        protected void Awake()
        {
            ReadSettings();
            if(PandoraSettings.IsProductEnvironment == false)
            {
                InitHandLog();
                AddLogWriter();
            }
        }

        private void InitHandLog()
        {
            if(_isHandLogInit == false)
            {
                _isHandLogInit = true;
                Logger.HandleLog = HandleLog;
            }
        }

        private void AddLogWriter()
        {
            if (_isWriterAdded == false)
            {
                _isWriterAdded = true;
                _recorderList.Add(new LogWriter());
            }
        }

        private void AddLogConsole()
        {
            if (_isConsoleAdded == false)
            {
                if (PandoraSettings.IsDebug == true && DelegateAggregator.GetRemoteConfig() != null && DelegateAggregator.GetRemoteConfig().GetFunctionSwitch("console") == true)
                {
                    _isConsoleAdded = true;
                    _recorderList.Add(LogViewerWindow.Instance);
                    this.gameObject.AddComponent<ConsoleWindow>();
                }
            }
        }

        private void AddErrorPanel()
        {
            if (_isErrorAdded == false)
            {
                if (PandoraSettings.IsDebug == true && DelegateAggregator.GetRemoteConfig() != null
                    && DelegateAggregator.GetRemoteConfig().GetFunctionSwitch("errorPanel") == true
                    )
                {
                    _isErrorAdded = true;
                    _recorderList.Add(this.gameObject.AddComponent<ShowErrorPanel>());
                }
            }
        }

        private void ReadSettings()
        {
            //从本地settings中读取log设置
            try
            {
                string filePath = LocalDirectoryHelper.GetSettingsFolderPath() + "/settings.txt";
                if (File.Exists(filePath) == true)
                {
                    string content = File.ReadAllText(filePath);
                    Dictionary<string, System.Object> dict = MiniJSON.Json.Deserialize(content) as Dictionary<string, System.Object>;
                    if(dict.ContainsKey("log") == true)
                    {
                        _localLogSetting = (dict["log"] as string) == "1";
                    }
                }
            }
            catch
            {
                //left blank
            }
        }

        private void Update()
        {
            if ( Logger.Enable == true
                && (Application.platform == RuntimePlatform.WindowsEditor
                || PandoraSettings.IsProductEnvironment == false
                || (PandoraSettings.IsProductEnvironment == true && PandoraSettings.IsDebug == true)
                || _localLogSetting == true))
            {
                InitHandLog();
                AddLogWriter();
                AddLogConsole();
                AddErrorPanel();
            }
            else
            {
                Logger.HandleLog = null;
            }
        }

        public void HandleLog(string message, string stackTrace, int level)
        {
            if (_recorderList.Count == 0)
            {
                return;
            }

            Log log = new Log
            {
                message = message,
                stackTrace = stackTrace,
                level = level
            };

            for (int i = 0; i < _recorderList.Count; i++)
            {
                _recorderList[i].Add(log);
            }
        }

    }
}

