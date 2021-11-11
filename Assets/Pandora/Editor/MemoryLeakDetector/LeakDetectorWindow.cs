using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace com.tencent.pandora.tools
{
    public class LeakDetectorWindow : EditorWindow
    {
        #region 参数
        private const int INSTRUCTION_HEIGHT = 80;
        private const int DEFAULT_PADDING = 4;
        private const int BUTTON_HEIGHT = 30;
        private const int TITLE_HEIGHT = 20;
        private const int INFO_LINE_HEIGHT = 30;

        private Vector2 _windowSize = Vector2.zero;
        private bool _windowResize = false;
        private Vector2 _csharpAreaScrollPosition = Vector2.zero;
        private Vector2 _luaAreaScrollPosition = Vector2.zero;
        private Vector2 _detailAreaScrollPosition = Vector2.zero;

        private float _csharpAreaScrollViewHeight;
        private float _luaAreaScrollViewHeight;
        private float _detailAreaScrollViewHeight;

        private int _selectedArea = -1;
        private int _selectedInfoIndex = -1;
        private int _newSelectedArea = -1;
        private int _newSelectedInfoIndex = -1;

        GUIStyle instructionStyle;
        private GUIStyle _briefInfoStyle;
        private const int BRIEF_INFO_FONT_SIZE = 12;
        private GUIStyle _detailInfoStyle;
        private const int DETAIL_INFO_FONT_SIZE = 12;
        private string _detailInfo = "";

        private Rect _segmentLineRect1 = new Rect();
        private Rect _segmentLineRect2 = new Rect();
        float yPostionOfSegmentLine1;
        float yPostionOfSegmentLine2;

        private Texture2D _segmentingLineTexture;
        private Texture2D _oddLineBackgroundTexture;
        private Texture2D _evenLineBackgroundTexture;
        private Texture2D _selectedLineBackoundTexture;

        private List<string> _totalLeakInfoList = new List<string>();
        private List<string> _csharpObjectLeakInfoList = new List<string>();
        private List<string> _luaObjectLeakInfoList = new List<string>();
        private List<string> _filtedCSharpObjectLeakInfoList = new List<string>();
        private List<string> _filtedLuaObjectLeakInfoList = new List<string>();

        private const string INSTRUCTION = "说明：\n1.运行工程，点击'打开活动面板前-记录'。\n2.打开待检测活动面板，对面板做全面交互操作。 \n3.点击'打开活动面板后-记录',对内存中对象做第一次快照。\n4.关闭活动面板，但保持工程处于运行中，点击'关闭活动面板后-检查'，显示区显示的即为未释放对象。";
        private const string CSHARP_LEAK_TITLE = "以下是c#泄漏项：";
        private const string LUA_LEAK_TITLE = "以下是lua泄漏项：";
        private const string NO_LEAK_TIPS = "恭喜，没有发现泄漏\n";
        private const string DEFAULT_FILTER_TIP = "输入关键字搜索";

        private bool _isPrepareButtonClicked = false;
        private bool _isRecordButtonClicked = false;
        private bool _isDetectButtonClicked = false;

        private string _csharpFilter = DEFAULT_FILTER_TIP;
        private string _luaFilter = DEFAULT_FILTER_TIP;

        #endregion

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
            InitTextures();
        }

        private void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
        }

        private void OnPlaymodeStateChanged()
        {
            if (EditorApplication.isPlaying == false)
            {
                _isPrepareButtonClicked = false;
                _isRecordButtonClicked = false;
                _isDetectButtonClicked = false;
            }
        }

        [MenuItem("PandoraTools/MemoryLeakDetector")]
        public static void ShowWindow()
        {
            GetWindow<LeakDetectorWindow>(false, "LeakDetector", true);
        }

        private void InitTextures()
        {
            Color oddColor = EditorGUIUtility.isProSkin ? new Color(55.0f / 255.0f, 55.0f / 255.0f, 55.0f / 255.0f) : new Color(216.0f / 255.0f, 216.0f / 255.0f, 216.0f / 255.0f);
            Color evenColor = EditorGUIUtility.isProSkin ? new Color(60.0f / 255.0f, 60.0f / 255.0f, 60.0f / 255.0f) : new Color(222.0f / 255.0f, 222.0f / 255.0f, 222.0f / 255.0f);
            Color selectedColor = EditorGUIUtility.isProSkin ? new Color(92.0f / 255.0f, 125.0f / 255.0f, 180.0f / 255.0f) : new Color(150.0f / 255.0f, 168.0f / 255.0f, 230.0f / 255.0f);

            _oddLineBackgroundTexture = CreateTexture(Vector2.one, oddColor);
            _evenLineBackgroundTexture = CreateTexture(Vector2.one, evenColor);
            _selectedLineBackoundTexture = CreateTexture(Vector2.one, selectedColor);
            _segmentingLineTexture = CreateTexture(Vector2.one, Color.black);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            RefreshWindowSize();
            RefreshScrollViewArea();
            ShowInstruction();
            GUILayout.Space(INSTRUCTION_HEIGHT + DEFAULT_PADDING);
            DrawButtons();
            //DrawOperateTips();
            DrawScharpArea();
            DrawSegmentingLine();
            DrawLuaArea();
            DrawDetailArea();
            OnInfoClick();
            EditorGUILayout.EndVertical();
        }
        private void RefreshWindowSize()
        {
            if (_windowSize.x != Screen.width || _windowSize.y != Screen.height)
            {
                _windowSize.x = Screen.width;
                _windowSize.y = Screen.height;
                _windowResize = true;
            }
            else
            {
                _windowResize = false;
            }
        }

        private void RefreshScrollViewArea()
        {
            if (_windowResize == true)
            {
                SetScrollViewArea();
                Repaint();
            }
        }

        private void SetScrollViewArea()
        {
            float usedSpace = INSTRUCTION_HEIGHT + BUTTON_HEIGHT + DEFAULT_PADDING;
            float remainedSpace = _windowSize.y - usedSpace;
            float oneFifthSpace = remainedSpace / 5.0f;

            _csharpAreaScrollViewHeight = oneFifthSpace * 2.0f - TITLE_HEIGHT;
            _luaAreaScrollViewHeight = _csharpAreaScrollViewHeight;
            _detailAreaScrollViewHeight = oneFifthSpace;

            yPostionOfSegmentLine1 = usedSpace + oneFifthSpace * 2.0f;
            yPostionOfSegmentLine2 = yPostionOfSegmentLine1 + oneFifthSpace * 2.0f;
            _segmentLineRect1.Set(0, yPostionOfSegmentLine1, Screen.width, 1f);
            _segmentLineRect2.Set(0, yPostionOfSegmentLine2, Screen.width, 1f);
        }
        private void ShowInstruction()
        {
            SetInstructionStyle();
            GUI.Box(new Rect(DEFAULT_PADDING, 0, _windowSize.x - 2 * DEFAULT_PADDING, INSTRUCTION_HEIGHT), INSTRUCTION, instructionStyle);
        }
        private void SetInstructionStyle()
        {
            instructionStyle = new GUIStyle("flow node 0");
            instructionStyle.alignment = TextAnchor.UpperLeft;
            instructionStyle.wordWrap = false;
            instructionStyle.padding = new RectOffset(10, 10, 30, 0);
            instructionStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            instructionStyle.fontSize = DETAIL_INFO_FONT_SIZE;
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            DrawPrepareButton();
            DrawRecordButton();
            DrawDetectButton();
            DrawClearButton();
            GUILayout.EndHorizontal();
        }

        //预处理Lua Table
        private void DrawPrepareButton()
        {
            bool isPlaying = EditorApplication.isPlaying;
            Color origin = GUI.backgroundColor;

            EditorGUI.BeginDisabledGroup(isPlaying == true && _isPrepareButtonClicked == true); // && _isRecordButtonClicked == false && _isDetectButtonClicked == false
            if (isPlaying == true && _isPrepareButtonClicked == false)
            {
                GUI.backgroundColor = Color.cyan;
            }

            if (GUILayout.Button("打开活动面板前-记录", GUILayout.Height(BUTTON_HEIGHT)))
            {
                _isPrepareButtonClicked = true;
                LeakDetector.Instance.GC();
                //在GC后，延时0.5s去记录
                DelayedCall.Instance().Add(0.5f, LuaSnapshot.PrepareToInsertSentry);
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = origin;
        }

        private void DrawRecordButton()
        {
            bool isPlaying = EditorApplication.isPlaying;
            Color origin = GUI.backgroundColor;

            EditorGUI.BeginDisabledGroup(isPlaying == true && (_isPrepareButtonClicked == false || _isRecordButtonClicked == true));
            if (isPlaying == true && _isPrepareButtonClicked == true && _isRecordButtonClicked == false)
            {
                GUI.backgroundColor = Color.cyan;
            }

            if (GUILayout.Button("打开活动面板后-记录", GUILayout.Height(BUTTON_HEIGHT)))
            {
                _isRecordButtonClicked = true;
                LeakDetector.Instance.GC();
                //在GC后，延时0.5s去记录
                DelayedCall.Instance().Add(0.5f, LuaSnapshot.ExecuteInsertSentry);
                DelayedCall.Instance().Add(0.5f, LeakDetector.Instance.RecordWhenPanelOpened);
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = origin;
        }

        private void DrawDetectButton()
        {
            bool isPlaying = EditorApplication.isPlaying;
            Color origin = GUI.backgroundColor;

            EditorGUI.BeginDisabledGroup(isPlaying == true && (_isRecordButtonClicked == false || _isPrepareButtonClicked == false || _isDetectButtonClicked ==  true));
            if (isPlaying == true && _isPrepareButtonClicked == true && _isRecordButtonClicked == true && _isDetectButtonClicked == false)
            {
                GUI.backgroundColor = Color.cyan;
            }

            if (GUILayout.Button("关闭活动面板后-检查", GUILayout.Height(BUTTON_HEIGHT)))
            {
                _isDetectButtonClicked = true;
                LeakDetector.Instance.GC();
                //在GC后，延时0.5s去检查泄漏
                DelayedCall.Instance().Add(0.5f, DetectLeak);
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = origin;
        }

        private void DrawClearButton()
        {
            if (GUILayout.Button("清空显示", GUILayout.Height(BUTTON_HEIGHT)))
            {
                _totalLeakInfoList.Clear();
                _csharpObjectLeakInfoList.Clear();
                _luaObjectLeakInfoList.Clear();
                _detailInfo = "";
                Repaint();
            }
        }

        //绘制指引提示
        private void DrawOperateTips()
        {
            if (EditorApplication.isPlaying == false)
            {
                _isPrepareButtonClicked = false;
                _isRecordButtonClicked = false;
                _isDetectButtonClicked = false;
                return;
            }

            //tips的位置
            float x;
            float y = INSTRUCTION_HEIGHT - 20;
            float width = 80f;
            float height = 20f;

            GUIStyle tipsStyle = new GUIStyle("CN EntryInfo");
            tipsStyle.normal.textColor = Color.cyan;

            if (_isPrepareButtonClicked == false)
            {
                x = _windowSize.x / 6 - 30;
                Rect rect = new Rect(x, y, width, height);
                GUI.Label(rect, "先点我", tipsStyle);
            }

            if (_isPrepareButtonClicked == true && _isRecordButtonClicked == false)
            {
                x = _windowSize.x / 6 - 30;
                Rect rect = new Rect(x, y, width, height);
                GUI.Label(rect, "再点我", tipsStyle);
            }

            if (_isRecordButtonClicked == true && _isDetectButtonClicked == false)
            {
                x = (_windowSize.x / 6) * 3 - 30;
                Rect rect = new Rect(x, y, width, height);
                GUI.Label(rect, "最后点我", tipsStyle);
            }
        }

        private void DetectLeak()
        {
            _totalLeakInfoList = LeakDetector.Instance.GetLeakInfoList();
            SeperateLeakInfo();
            Repaint();
        }

        private void SeperateLeakInfo()
        {
            _csharpObjectLeakInfoList.Clear();
            _luaObjectLeakInfoList.Clear();

            List<string> gameobjectList = new List<string>();
            List<string> transformList = new List<string>();
            List<string> componentList = new List<string>();
            List<string> otherList = new List<string>();
            List<string> nullList = new List<string>();
            foreach (var item in _totalLeakInfoList)
            {
                if (item.Contains("C# GameObject"))
                {
                    gameobjectList.Add(item);
                    continue;
                }

                if (item.Contains("C# Transform"))
                {
                    transformList.Add(item);
                    continue;
                }

                if (item.Contains("C# Component"))
                {
                    componentList.Add(item);
                    continue;
                }

                if (item.Contains("C# Other"))
                {
                    otherList.Add(item);
                    continue;
                }

                if (item.Contains("C# null"))
                {
                    nullList.Add(item);
                    continue;
                }
                _luaObjectLeakInfoList.Add(item);
            }

            _csharpObjectLeakInfoList.AddRange(gameobjectList);
            _csharpObjectLeakInfoList.AddRange(transformList);
            _csharpObjectLeakInfoList.AddRange(componentList);
            _csharpObjectLeakInfoList.AddRange(otherList);
            _csharpObjectLeakInfoList.AddRange(nullList);

            if (_csharpObjectLeakInfoList.Count == 0)
            {
                _csharpObjectLeakInfoList.Add(NO_LEAK_TIPS);
            }

            if (_luaObjectLeakInfoList.Count == 0)
            {
                _luaObjectLeakInfoList.Add(NO_LEAK_TIPS);
            }
        }

        private void DrawScharpArea()
        {
            int leakNum = 0;
            if (_csharpObjectLeakInfoList.Count > 0 && _csharpObjectLeakInfoList[0] != NO_LEAK_TIPS)
            {
                leakNum = _csharpObjectLeakInfoList.Count;
            }

            _filtedCSharpObjectLeakInfoList = FiltLeakInfo(_csharpFilter, _csharpObjectLeakInfoList);
            DrawBriefInfoArea(CSHARP_LEAK_TITLE + leakNum, ref _csharpFilter, ref _csharpAreaScrollPosition, _csharpAreaScrollViewHeight, _filtedCSharpObjectLeakInfoList, 1);
        }

        private void DrawLuaArea()
        {
            int leakNum = 0;
            if (_luaObjectLeakInfoList.Count > 0 && _luaObjectLeakInfoList[0] != NO_LEAK_TIPS)
            {
                leakNum = _luaObjectLeakInfoList.Count;
            }
            _filtedLuaObjectLeakInfoList = FiltLeakInfo(_luaFilter, _luaObjectLeakInfoList);
            DrawBriefInfoArea(LUA_LEAK_TITLE + leakNum, ref _luaFilter, ref _luaAreaScrollPosition, _luaAreaScrollViewHeight, _filtedLuaObjectLeakInfoList, 2);
        }

        private List<string> FiltLeakInfo(string keyword, List<string> source)
        {
            if (keyword == DEFAULT_FILTER_TIP || string.IsNullOrEmpty(keyword))
            {
                return source;
            }

            List<string> result = new List<string>();
            string item;
            keyword = keyword.ToLower();
            for (int i = 0; i < source.Count; i++)
            {
                item = source[i].ToLower();
                if (item.Contains(keyword))
                {
                    result.Add(source[i]);
                }
            }
            return result;
        }

        private void DrawBriefInfoArea(string title, ref string filter, ref Vector2 scrollPosition, float viewHeight, List<string> infoList, int area)
        {
            DrawTitle(title, ref filter);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(viewHeight));
            int length = infoList.Count;
            for (int i = 0; i < length; i++)
            {
                DrawInfo(area, i, infoList[i].Substring(0, infoList[i].IndexOf("\n")));
            }
            GUILayout.Space(length * INFO_LINE_HEIGHT);
            EditorGUILayout.EndScrollView();
        }

        private void DrawTitle(string content, ref string filter)
        {
            SetDetailInfoStyle();
            Color originalColor = _detailInfoStyle.normal.textColor;
            _detailInfoStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 0f, 1f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Label(content, _detailInfoStyle, GUILayout.Height(TITLE_HEIGHT));
            //GUI.changed = false;
            filter = GUILayout.TextField(filter);
            if (string.IsNullOrEmpty(filter))
            {
                filter = DEFAULT_FILTER_TIP;
            }

            GUILayout.EndHorizontal();
            _detailInfoStyle.normal.textColor = originalColor;
        }
        private void SetDetailInfoStyle()
        {
            _detailInfoStyle = new GUIStyle();
            _detailInfoStyle.alignment = TextAnchor.UpperLeft;
            _detailInfoStyle.padding = new RectOffset(2, 2, 2, 2);
            _detailInfoStyle.wordWrap = true;
            _detailInfoStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            _detailInfoStyle.fontSize = DETAIL_INFO_FONT_SIZE;
        }

        private void DrawInfo(int area, int index, string message)
        {
            Rect rect = new Rect(0, index * INFO_LINE_HEIGHT, Screen.width - 18f, INFO_LINE_HEIGHT);
            DrawInfoBackground(area, index, rect);
            //使用内置的GUIStyle，初始化必须放在OnGUI内调用，不能放在OnEnable中，否则会报错。
            bool isInfoMessage = NO_LEAK_TIPS.Contains(message);
            string styleName = isInfoMessage ? "CN EntryInfo" : "CN EntryWarn";
            SetBriefInfoStyle(styleName);

            GUI.Label(rect, message, _briefInfoStyle);
            //添加cursorRect
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Text);
        }
        private void SetBriefInfoStyle(string styleName)
        {
            _briefInfoStyle = new GUIStyle(styleName);
            _briefInfoStyle.alignment = TextAnchor.UpperLeft;
            _briefInfoStyle.clipping = TextClipping.Clip;
            _briefInfoStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            _briefInfoStyle.fontSize = BRIEF_INFO_FONT_SIZE;
        }

        private void DrawInfoBackground(int area, int index, Rect rect)
        {
            if (area == _selectedArea && index == _selectedInfoIndex)
            {
                GUI.DrawTexture(rect, _selectedLineBackoundTexture);
            }
            else if (index % 2 == 0)
            {
                GUI.DrawTexture(rect, _evenLineBackgroundTexture);
            }
            else
            {
                GUI.DrawTexture(rect, _oddLineBackgroundTexture);
            }
        }

        private void DrawSegmentingLine()
        {
            GUI.DrawTexture(_segmentLineRect1, _segmentingLineTexture);
            GUI.DrawTexture(_segmentLineRect2, _segmentingLineTexture);
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
        private void DrawDetailArea()
        {
            if (_selectedArea == -1)
            {
                return;
            }
            _detailAreaScrollPosition = EditorGUILayout.BeginScrollView(_detailAreaScrollPosition, GUILayout.Height(_detailAreaScrollViewHeight));
            _detailInfo = "";

            if (_selectedArea == 1)
            {
                if (-1 < _selectedInfoIndex && _selectedInfoIndex < _filtedCSharpObjectLeakInfoList.Count)
                {
                    _detailInfo = _filtedCSharpObjectLeakInfoList[_selectedInfoIndex];
                }
            }
            else if (_selectedArea == 2)
            {
                if (-1 < _selectedInfoIndex && _selectedInfoIndex < _filtedLuaObjectLeakInfoList.Count)
                {
                    _detailInfo = _filtedLuaObjectLeakInfoList[_selectedInfoIndex];
                }
            }

            if (_detailInfoStyle == null)
            {
                SetDetailInfoStyle();
            }
            EditorGUILayout.TextArea(_detailInfo, _detailInfoStyle);

            //加两个空行
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndScrollView();
        }

        private void OnInfoClick()
        {
            CalculateSelectedAreaAndIndex();
            if (_newSelectedArea == -1 || _newSelectedInfoIndex == -1)
            {
                return;
            }
            if (_newSelectedInfoIndex != _selectedInfoIndex || _newSelectedArea != _selectedArea)
            {
                _selectedInfoIndex = _newSelectedInfoIndex;
                _selectedArea = _newSelectedArea;

                //如果选中了详情区的TextArea，需要移除焦点，否则更新不了TextArea
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                Repaint();
            }
        }

        private void CalculateSelectedAreaAndIndex()
        {
            if (Event.current.type != EventType.MouseUp)
            {
                return;
            }

            //鼠标弹起代表一次点击
            Vector2 originalMousePosition = Event.current.mousePosition;
            Vector2 currentMousePositionInCsharpArea = Event.current.mousePosition + _csharpAreaScrollPosition;
            Vector2 currentMousePositionInLuaArea = Event.current.mousePosition + _luaAreaScrollPosition;

            float csharpAreaMaxHeight = yPostionOfSegmentLine1 + _csharpAreaScrollPosition.y;
            float luaAreaMaxHeight = yPostionOfSegmentLine2 + _luaAreaScrollPosition.y;

            float headSpaceForCsharpArea = INSTRUCTION_HEIGHT + BUTTON_HEIGHT + DEFAULT_PADDING + TITLE_HEIGHT;
            float headSpaceForLuaArea = yPostionOfSegmentLine1 + TITLE_HEIGHT;

            if (originalMousePosition.y <= headSpaceForCsharpArea)
            {
                _newSelectedArea = -1;
                _newSelectedInfoIndex = -1;
            }
            else if (originalMousePosition.y <= yPostionOfSegmentLine1)
            {
                _newSelectedArea = 1;
                _newSelectedInfoIndex = GetSelectedIndex(headSpaceForCsharpArea, csharpAreaMaxHeight, INFO_LINE_HEIGHT, currentMousePositionInCsharpArea);
            }
            else if (originalMousePosition.y <= yPostionOfSegmentLine2)
            {
                _newSelectedArea = 2;
                _newSelectedInfoIndex = GetSelectedIndex(headSpaceForLuaArea, luaAreaMaxHeight, INFO_LINE_HEIGHT, currentMousePositionInLuaArea);
            }
            else
            {
                _newSelectedArea = -1;
                _newSelectedInfoIndex = -1;
            }
        }

        private int GetSelectedIndex(float min, float max, float increase, Vector2 mousePosition)
        {
            int index = -1;
            Rect currentRect;
            for (int y = (int)min; y < (int)max; y += (int)increase)
            {
                index++;
                currentRect = new Rect(0, y, Screen.width - 18, increase);
                if (currentRect.Contains(mousePosition) == true)
                {
                    return index;
                }
            }
            return -1;
        }

        private void Update()
        {
            DelayedCall.Instance().Tick();
        }
    }

    class DelayedCall
    {
        private static DelayedCall _instance;
        private List<TimeFuncMap> _store = new List<TimeFuncMap>();
        //已经触发过的，需要移除的
        private List<TimeFuncMap> _needRemove = new List<TimeFuncMap>();

        public static DelayedCall Instance()
        {
            if (_instance == null)
            {
                _instance = new DelayedCall();
            }
            return _instance;
        }

        //私有构造函数，不允许外部创建实例
        private DelayedCall()
        {

        }

        public void Add(float delayInSecond, Action action)
        {
            float time = Time.realtimeSinceStartup + delayInSecond;
            _store.Add(new TimeFuncMap(time, action));
        }

        //执行
        public void Tick()
        {
            _needRemove.Clear();
            foreach (var item in _store)
            {
                if (Time.realtimeSinceStartup >= item.triggerTime)
                {
                    if (item.action != null)
                    {
                        item.action();
                    }
                    _needRemove.Add(item);
                }
            }
            foreach (var item in _needRemove)
            {
                _store.Remove(item);
            }
        }
    }

    class TimeFuncMap
    {
        public float triggerTime;
        public Action action;
        public TimeFuncMap(float time, Action action)
        {
            this.triggerTime = time;
            this.action = action;
        }
    }
}