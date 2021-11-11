using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace com.tencent.pandora
{
    /// <summary>
    /// 将Log显示在屏幕GUI上
    /// </summary>
    public class LogViewerWindow : ILogRecorder, IConsoleChildWindow
    {
        private const int LOG_MAX_COUNT = 100;
        private static readonly Dictionary<int, Color> LOG_TYPE_COLOR_DICT = new Dictionary<int, Color>
        {
            {Logger.DEBUG, Color.white },
            {Logger.INFO, Color.white },
            {Logger.WARNING, Color.yellow },
            {Logger.ERROR, Color.red }
        };
        private static LogViewerWindow _instance;

        private float _padding = 10;
        private float _toggleSize = 35;
        private float _logListScrollViewHeight;
        private float _selectedLogScrollViewHeight;

        private Vector2 _windowMaxSize;
        private Vector2 _logListScrollPosition;
        private Vector2 _selectedLogScrollPosition;
        private Rect _windowRect;
        private bool _showDebug = true;
        private bool _showInfo = true;
        private bool _showWarning = true;
        private bool _showError = true;

        private Log _selectedLog;
        private List<Log> _logList = new List<Log>(LOG_MAX_COUNT);
        private FileUploader _fileUploader;

        public static LogViewerWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogViewerWindow();
                }
                return _instance;
            }
        }

        public void Init(float ratio, Vector2 windowCenter, Vector2 windowMaxSize)
        {
            _windowMaxSize = windowMaxSize;
            InitControlSize(ratio);
            InitRect(windowCenter, windowMaxSize);
        }

        private void InitControlSize(float ratio)
        {
            _padding *= ratio;
            _toggleSize *= ratio;
        }

        private void InitRect(Vector2 windowCenter, Vector2 windowMaxSize)
        {
            _windowRect = new Rect(0, 0, windowMaxSize.x, windowMaxSize.y);
            _windowRect.center = windowCenter;
        }

        public void Draw()
        {
            _windowRect = GUILayout.Window(1001, _windowRect, DrawWindow, "日志查看");
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.Space(_padding);
            AdjustScrollViewHeight();
            DrawLogList();
            DrawSelectedLog();
            DrawLogLevelSettings();
            DrawToolbar();
        }

        private void AdjustScrollViewHeight()
        {
            //GUILayout布局的元素在初始窗口（即windowRect)不足以容纳它时会自动扩大windowRect，
            //需要动态调整log 区域的scrollview高度,从而保证所有元素都能布局在窗口内
            float yRatio = _windowMaxSize.y / _windowRect.size.y;
            _logListScrollViewHeight = _windowMaxSize.y * 0.48f * yRatio;
            _selectedLogScrollViewHeight = _windowMaxSize.y * 0.3f * yRatio;
        }

        private void DrawLogList()
        {
            _logListScrollPosition = GUILayout.BeginScrollView(_logListScrollPosition, GUILayout.Height(_logListScrollViewHeight));
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < _logList.Count; i++)
            {
                Log log = _logList[i];
                if (_showDebug == false && log.level == Logger.DEBUG)
                {
                    continue;
                }
                if (_showInfo == false && log.level == Logger.INFO)
                {
                    continue;
                }
                if (_showWarning == false && log.level == Logger.WARNING)
                {
                    continue;
                }
                if (_showError == false && log.level == Logger.ERROR)
                {
                    continue;
                }
                string message = log.message;
                string label = message;
                if (message.IndexOf("\n") != -1)
                {
                    label = message.Substring(0, message.IndexOf("\n"));
                }
                label = label.Substring(0, Mathf.Min(150, label.Length));

                GUI.contentColor = LOG_TYPE_COLOR_DICT[log.level];
                if (GUILayout.Button(label))
                {
                    _selectedLog = log;
                }
            }
            GUI.contentColor = Color.white;
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            GUILayout.EndScrollView();
        }

        private void DrawSelectedLog()
        {
            _selectedLogScrollPosition = GUILayout.BeginScrollView(_selectedLogScrollPosition, GUILayout.Height(_selectedLogScrollViewHeight));
            GUI.contentColor = LOG_TYPE_COLOR_DICT[_selectedLog.level];
            GUILayout.Label(_selectedLog.message + "\n" + _selectedLog.stackTrace);
            GUI.contentColor = Color.white;
            GUILayout.EndScrollView();
        }

        private void DrawLogLevelSettings()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Log显示选项：");
            DrawToggle("Debug", ref _showDebug);
            DrawToggle("Info", ref _showInfo);
            DrawToggle("Warning", ref _showWarning);
            DrawToggle("Error", ref _showError);
            GUILayout.EndHorizontal();
        }

        private void DrawToggle(string label, ref bool state)
        {
            state = GUILayout.Toggle(state, "", "logLevelSettingStyle", GUILayout.Width(_toggleSize), GUILayout.Height(_toggleSize));
            GUILayout.Label(label);
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            if (GUILayout.Button("复制Log到剪贴板"))
            {
                GUIUtility.systemCopyBuffer = _selectedLog.level.ToString() + "\n" + _selectedLog.message + "\n" + _selectedLog.stackTrace;
            }
#endif
            if (GUILayout.Button("清空显示"))
            {
                _logList.Clear();
                _selectedLog = default(Log);
            }
            if (GUILayout.Button("删除cookie"))
            {
                LocalDirectoryHelper.DeleteCookies();
            }
            if (GUILayout.Button("删除日志"))
            {
                LocalDirectoryHelper.DeleteLogs();
            }
            if (GUILayout.Button("上传日志"))
            {
                if (_fileUploader == null)
                {
                    _fileUploader = new FileUploader();
                }
                _fileUploader.Upload(GetLogPath());
            }
            GUILayout.EndHorizontal();
        }

        private string GetLogPath()
        {
            string logName = string.Format("log-{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            string path = Path.Combine(LocalDirectoryHelper.GetLogFolderPath(), logName);
            return path;
        }

        public void Add(Log log)
        {
            if (_logList.Count >= LOG_MAX_COUNT)
            {
                _logList.RemoveAt(0);
            }
            _logList.Add(log);
        }

        public void Dispose()
        {
            _logList.Clear();
        }
    }
}