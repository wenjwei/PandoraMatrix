using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.tencent.pandora
{
    public class ConsoleWindow : MonoBehaviour
    {
        #region 变量区
        private const string SKIN_NAME = "ConsoleSkin";
        private Vector2 REF_SCREEN_RESOLUTION = new Vector2(1136, 640);
        private GUISkin _guiSkin;
        private float _ratio;
        //因为要根据屏幕分辨率对组件的尺寸进行缩放，尺寸不能定义为常量
        private float _entranceWidth = 76;
        private float _entranceHeight = 62;
        private float _offset = 10;
        private float _windowWidth = 1136;
        private float _windowHeight = 640;
        private float _fontSize = 25;
        private float _scrollbarWidth = 15;
        private float _scrollbarHeight = 15;
        private float _tabLeftPadding = 10;
        private float _tabWidth = 200;
        private float _tabHeight = 40;
        private float _tabSpacing = 45;
        private float _closeBtnWidth = 30;
        private float _closeBtnHeight = 42;
        private Vector2 _childWindowCenter = new Vector2(665, 320);
        private Vector2 _childWindowMaxSize = new Vector2(850, 620);

        private int _currentSelectedIndex = 0;
        private bool _windowVisible = false;
        private Vector2 _tabScrollPosition = Vector2.zero;

        private Rect _entranceRect;
        private Rect _windowRect;
        private Rect _closeBtnRect;
        private Rect _tabListRect;
        private Rect _tabListViewRect;
        private Texture2D _windowBackgroundTexture;

        //string 为tab名，IWindow为工具子窗口
        private Dictionary<string, IConsoleChildWindow> _tabDict = new Dictionary<string, IConsoleChildWindow>();
        private List<string> _tabNameList = new List<string>();

        private Vector2 _touchPositon = Vector2.zero;
        private Vector2 _touchDeltaPosition = Vector2.zero;
        private bool _isEntranceDragging = false;

        #endregion
        private void Awake()
        {
            _ratio = Mathf.Min(Screen.width / REF_SCREEN_RESOLUTION.x, Screen.height / REF_SCREEN_RESOLUTION.y);
            _windowBackgroundTexture = CreateTexture(Vector2.one, new Color(55.0f / 255.0f, 55.0f / 255.0f, 55.0f / 255.0f));
            InitControlSize();
            InitRect();
            InitTabInfo();
        }

        private void InitControlSize()
        {
            _entranceWidth *= _ratio;
            _entranceHeight *= _ratio;
            _offset *= _ratio;
            _windowWidth *= _ratio;
            _windowHeight *= _ratio;
            _fontSize *= _ratio;
            _scrollbarWidth *= _ratio;
            _scrollbarHeight *= _ratio;
            _tabLeftPadding *= _ratio;
            _tabWidth *= _ratio;
            _tabHeight *= _ratio;
            _tabSpacing *= _ratio;
            _closeBtnWidth *= _ratio;
            _closeBtnHeight *= _ratio;
            _childWindowCenter *= _ratio;
            _childWindowMaxSize *= _ratio;
        }

        private void InitRect()
        {
            _entranceRect = new Rect(_offset, _offset, _entranceWidth, _entranceHeight);
            _windowRect = new Rect(_offset, _offset, _windowWidth - _offset * 2, _windowHeight - _offset * 2);
            AdjustPosition();
            _closeBtnRect = new Rect(_windowRect.xMax - _closeBtnWidth * 1.3f, _windowRect.yMin, _closeBtnWidth, _closeBtnHeight);
            _tabListRect = new Rect(_windowRect.xMin + _offset, _windowRect.yMin + _offset, _tabLeftPadding + _tabWidth + _scrollbarWidth, _windowHeight - _offset * 4);
            _tabListViewRect = new Rect(0, 0, _tabWidth, _tabHeight * (_tabNameList.Count + 2));
        }

        //为了使窗口居中显示，需要调整参考窗口中心
        private void AdjustPosition()
        {
            Vector2 delta = new Vector2(Screen.width / 2 - _windowRect.center.x, Screen.height / 2 - _windowRect.center.y);
            _windowRect.center = _windowRect.center + delta;
            _childWindowCenter = _childWindowCenter + delta;
        }

        private void InitTabInfo()
        {
            //日志查看器
            LogViewerWindow logViewerWindow = LogViewerWindow.Instance;
            logViewerWindow.Init(_ratio, _childWindowCenter, _childWindowMaxSize);
            _tabDict.Add("日志查看", logViewerWindow);

            //文件查看器
            FileViewerWindow fileViewerWindow = new FileViewerWindow();
            fileViewerWindow.Init(_ratio, _childWindowCenter, _childWindowMaxSize);
            _tabDict.Add("文件查看", fileViewerWindow);

            _tabNameList = _tabDict.Keys.ToList<string>();
        }

        private void OnGUI()
        {
            SetGUISkin();
            if (_windowVisible == false)
            {
                if (_isEntranceDragging == false && GUI.Button(_entranceRect, "", "entranceStyle"))
                {
                    _windowVisible = true;
                }
                return;
            }

            //打开当前选中的tab对应的window
            if (_tabNameList.Count > 0)
            {
                _tabDict[_tabNameList[_currentSelectedIndex]].Draw();
            }
            DrawWindow();
        }

        private void SetGUISkin()
        {
            if (_guiSkin == null)
            {
                _guiSkin = Resources.Load<GUISkin>(SKIN_NAME);
            }
            GUI.skin = _guiSkin;

            var enumrator = GUI.skin.GetEnumerator();
            while (enumrator.MoveNext())
            {
                var current = enumrator.Current as GUIStyle;
                current.fontSize = (int)_fontSize;
            }

            GUI.skin.verticalScrollbar.fixedWidth = _scrollbarWidth;
            GUI.skin.verticalScrollbarThumb.fixedWidth = _scrollbarWidth;
            GUI.skin.horizontalScrollbar.fixedHeight = _scrollbarHeight;
            GUI.skin.horizontalScrollbarThumb.fixedHeight = _scrollbarHeight;
        }

        private void DrawWindow()
        {
            DrawBackground();
            DrawCloseButton();
            DrawTabs();
        }

        private void DrawBackground()
        {
            GUI.DrawTexture(_windowRect, _windowBackgroundTexture);
            GUI.Box(_tabListRect, "", "backgroundBoxStyle");
        }

        private void DrawCloseButton()
        {
            if (GUI.Button(_closeBtnRect, "", "closeBtnStyle"))
            {
                _windowVisible = false;
                _currentSelectedIndex = 0;
            }
        }

        private void DrawTabs()
        {
            _tabScrollPosition = GUI.BeginScrollView(_tabListRect, _tabScrollPosition, _tabListViewRect);
            for (int i = 0; i < _tabNameList.Count; i++)
            {
                Rect tabRect = new Rect(_tabLeftPadding, i * _tabSpacing + _offset, _tabWidth, _tabHeight);
                DrawTabBackground(tabRect, i);
                DrawTabButton(tabRect, _tabNameList[i], i);
            }
            GUILayout.Space(_tabSpacing * _tabNameList.Count);
            GUI.EndScrollView();
        }

        private void DrawTabBackground(Rect rect, int index)
        {
            if (index == _currentSelectedIndex)
            {
                GUI.Label(rect, "", "entrySelectedStyle");
            }
        }

        private void DrawTabButton(Rect position, string name, int index)
        {
            if (GUI.Button(position, name))
            {
                _currentSelectedIndex = index;
            }
        }

        private Texture2D CreateTexture(Vector2 size, Color color)
        {
            Texture2D tex = new Texture2D((int)size.x, (int)size.y);
            tex.hideFlags = HideFlags.DontSave;
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    tex.SetPixel(i, j, color);
                }
            }
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        private void Update()
        {
            if (Input.touchCount == 1)
            {
                DragEntrance(Input.GetTouch(0));
            }
        }

        private void DragEntrance(Touch touch)
        {
            _isEntranceDragging = IsEntanceDragging(touch);

            if (_isEntranceDragging == false)
            {
                return;
            }

            _touchDeltaPosition = touch.deltaPosition;
            _touchDeltaPosition.y = -_touchDeltaPosition.y;
            _entranceRect.position = _entranceRect.position + _touchDeltaPosition;
        }

        private bool IsEntanceDragging(Touch touch)
        {
            if (touch.phase != TouchPhase.Moved)
            {
                return false;
            }

            //转换y坐标
            _touchPositon = touch.position;
            _touchPositon.y = Screen.height - _touchPositon.y;

            if (_entranceRect.Contains(_touchPositon) == false)
            {
                return false;
            }

            return true;
        }
    }
}