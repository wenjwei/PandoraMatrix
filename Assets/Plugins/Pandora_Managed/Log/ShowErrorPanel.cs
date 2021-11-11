using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.tencent.pandora { 
    public class ShowErrorPanel : MonoBehaviour, ILogRecorder
    {
        private const int LOG_MAX_COUNT = 100;
        private const float POS_COF = 0.125f;
        private const float SIZE_COF = 0.75f;

        private Color LOG_TYPE_ERROR_COLOR = Color.white;
        private Rect _windowRect;
        private bool _visible;

        private Log _targetLog;

        protected void Awake()
        {
            _windowRect = new Rect(Screen.width * POS_COF, Screen.height * POS_COF, Screen.width * SIZE_COF, Screen.height * SIZE_COF);
            _targetLog = default(Log);
        }

        protected void OnGUI()
        {
            if (_visible == false)
            {
                return;
            }
            GUI.skin.label.fontSize = 18;
            _windowRect = GUILayout.Window(int.MaxValue, _windowRect, DrawWindowContent, "Console(请将日志截屏发送给对应的开发人员，发生错误时强制显示)"
                , GUILayout.Width(Screen.width * SIZE_COF)
                , GUILayout.Height(Screen.height * SIZE_COF));
        }

        private void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            DrawCloseButton();
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            DrawLogLabel();
            GUILayout.EndVertical();
        }

        private void DrawLogLabel()
        {
            {
                Log log = _targetLog;
                GUI.contentColor = LOG_TYPE_ERROR_COLOR;
                int length = log.message.Length;
                string label = log.message.Substring(0, UnityEngine.Mathf.Min(2000, length));
                GUILayout.Label(label, GUILayout.ExpandHeight(true));
      
            }
            GUI.contentColor = Color.white;
        }

        private void DrawCloseButton()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            if (GUILayout.Button("复制Log到剪贴板"))
            {

                GUIUtility.systemCopyBuffer = _targetLog.message + "\n" + _targetLog.stackTrace;
            }
#endif
            if (GUILayout.Button("关闭LogGUI"))
            {
                _visible = false;
            }
            GUILayout.EndHorizontal();
        }


        public void Add(Log log)
        {
            if (log.level  == Logger.ERROR)
            {
                _visible = true;
                _targetLog = log;
            }
        }

        public void Dispose()
        {
            _targetLog = default(Log);
        }
    }
}
