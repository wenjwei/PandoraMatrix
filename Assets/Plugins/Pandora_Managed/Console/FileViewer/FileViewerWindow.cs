using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{
    public class FileViewerWindow : IConsoleChildWindow
    {
        #region 变量区
        private const int MAX_LENGTH_OF_TEXT_SHOW = 16382;
        private float _topPadding = 20;
        private float _pathWidth = 320;
        private float _foldIconWidth = 30;
        private float _rowIndent = 15;
        private float _rowHeight = 40;
        private float _directoryTreeWidth = 200;
        private float _filePropertyWidth = 180;
        private bool _textWindowVisible = false;
        private bool _confirmWindowVisible = false;
        private bool _messageWindowVisible = false;
        private bool _needRefreshDirectoryContentList = false;

        private Vector2 _closeBtnSize = new Vector2(30, 42);
        private Vector2 _confirmWindowSize = new Vector2(300, 200);
        private Vector2 _messageWindowSize = new Vector2(300, 150);
        private Vector2 _textWindowCenterOffset = new Vector2(105, 35);
        private Vector2 _directoryScrollPosition = Vector2.zero;
        private Vector2 _textScrollPosition = Vector2.zero;
        private Vector2 _contentAreaScrollPosition = Vector2.zero;
        private Vector2 _contentBodyScrollPositon = Vector2.zero;

        private Rect _homeWindowRect;
        private Rect _textWindowRect;
        private Rect _closeBtnRect;
        private Rect _messageWindowRect;
        private Rect _confirmWindowRect;

        private string _fileContent;
        private Folder _directoryRoot;
        private Folder _currentDirectory;
        private GUIStyle _entryStyle;
        private FileSystemEntry _currentEntry;
        private List<Folder> _directoryList = new List<Folder>();
        private List<FileSystemEntry> _directoryContentList = new List<FileSystemEntry>();

        //按钮双击事件
        private int _lastClickedId;
        private float _lastClickedTime;
        private bool _entryClicked;

        //提示窗口展示时间
        private float _messageStartDisplayTime;
        private string _messageToShow;
        private string _pathToDisplay = "";

        private FileUploader _fileUploader;
        #endregion

        public void Init(float ratio, Vector2 windowCenter, Vector2 windowMaxSize)
        {
            FileViewer.ProductName = Application.productName;
            InitControlSize(ratio);
            InitRect(windowCenter, windowMaxSize);
            InitDirectoryTree();

        }
        private void InitControlSize(float ratio)
        {
            _topPadding *= ratio;
            _pathWidth *= ratio;
            _foldIconWidth *= ratio;
            _rowHeight *= ratio;
            _rowIndent *= ratio;
            _directoryTreeWidth *= ratio;
            _filePropertyWidth *= ratio;
            _closeBtnSize *= ratio;
            _confirmWindowSize *= ratio;
            _messageWindowSize *= ratio;
            _textWindowCenterOffset *= ratio;
        }

        private void InitRect(Vector2 windowCenter, Vector2 windowMaxSize)
        {
            _homeWindowRect = new Rect(0, 0, windowMaxSize.x, windowMaxSize.y);
            _textWindowRect = new Rect(0, 0, windowMaxSize.x * 0.75f, windowMaxSize.y * 0.86f);
            _confirmWindowRect = new Rect(0, 0, _confirmWindowSize.x, _confirmWindowSize.y);
            _messageWindowRect = new Rect(0, 0, _messageWindowSize.x, _messageWindowSize.y);
            _closeBtnRect = new Rect(_textWindowRect.size.x - _closeBtnSize.x * 1.3f, 0, _closeBtnSize.x, _closeBtnSize.y);
            _homeWindowRect.center = windowCenter;
            _textWindowRect.center = windowCenter + _textWindowCenterOffset;
            _confirmWindowRect.center = windowCenter + new Vector2(_directoryTreeWidth / 2, 0);
            _messageWindowRect.center = windowCenter + new Vector2(_directoryTreeWidth / 2, 0);
        }

        private void InitDirectoryTree()
        {
            _directoryRoot = new FileViewer().GenerateDirectoryTree();
            TreeUtility.TreeToList<Folder>(_directoryRoot, _directoryList);
        }

        public void Draw()
        {
            SwitchWindow();
            UpdateFlag();
        }

        private void SwitchWindow()
        {
            if (_messageWindowVisible)
            {
                _messageWindowRect = GUI.ModalWindow(1004, _messageWindowRect, DrawMessageWindow, "提示", "popWindowStyle");
            }

            if (_confirmWindowVisible)
            {
                _confirmWindowRect = GUI.ModalWindow(1003, _confirmWindowRect, DrawConfirmWindow, "确认", "popWindowStyle");
            }

            if (_textWindowVisible)
            {
                _textWindowRect = GUI.ModalWindow(1002, _textWindowRect, DrawTextWindow, "文件内容");
            }
            _homeWindowRect = GUILayout.Window(1001, _homeWindowRect, DrawHomeWindow, "文件查看");
        }

        private void DrawHomeWindow(int windowId)
        {
            DrawHeader();
            GUILayout.BeginHorizontal();
            DrawDirectoryTree();
            DrawContentArea();
            GUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            GUILayout.Space(_topPadding);
            GUILayout.BeginHorizontal();
            GUILayout.TextArea(_pathToDisplay, GUILayout.Width(_pathWidth));
            if (GUILayout.Button("查看"))
            {
                OnEntryDoubleClick();
            }

            if (GUILayout.Button("上传"))
            {
                Upload();
            }

            if (GUILayout.Button("删除"))
            {
                Delete();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDirectoryTree()
        {
            _directoryScrollPosition = GUILayout.BeginScrollView(_directoryScrollPosition, GUILayout.Width(_directoryTreeWidth));
            int index = 0;
            DrawDirectoryItem(_directoryRoot, ref index);
            GUILayout.Space(index * _rowHeight);
            GUILayout.EndScrollView();
        }

        //绘制单个目录，递归绘制
        private void DrawDirectoryItem(Folder folder, ref int index)
        {
            DrawDirectoryBackground(folder, index);
            DrawFoldIcon(folder, index);
            DrawDirectoryButton(folder, index);
            index++;
            if (folder.IsFolded || folder.HasChildren == false)
            {
                return;
            }

            for (int i = 0; i < folder.Children.Count; i++)
            {
                DrawDirectoryItem((Folder)folder.Children[i], ref index);
            }
        }

        //绘制被选中项的背景
        private void DrawDirectoryBackground(Folder folder, int index)
        {
            if (_currentDirectory != null && _currentDirectory == folder)
            {
                Rect backgroundRect = new Rect(0, index * _rowHeight, _directoryTreeWidth, _rowHeight);
                GUI.Label(backgroundRect, "", "entrySelectedStyle");
            }
        }

        //绘制折叠图标
        private void DrawFoldIcon(Folder folder, int index)
        {
            float xPos = folder.Depth * _rowIndent;
            float yPos = index * _rowHeight;
            if (folder.HasChildren)
            {
                Rect iconRect = new Rect(xPos, yPos, _foldIconWidth, _rowHeight);
                //有子文件夹时绘制图标
                string icon = "\u25BA";
                if (folder.IsFolded == false)
                {
                    icon = "\u25BC";
                }
                folder.IsFolded = GUI.Toggle(iconRect, folder.IsFolded, icon, "foldIconStyle");
            }
        }

        private void DrawDirectoryButton(Folder folder, int index)
        {
            float xPos = folder.Depth * _rowIndent;
            float yPos = index * _rowHeight;
            Rect buttonRect = new Rect(xPos + _foldIconWidth, yPos, _directoryTreeWidth, _rowHeight);
            if (GUI.Button(buttonRect, folder.Name, "entryOddRowStyle"))
            {
                _currentDirectory = folder;
                _pathToDisplay = _currentDirectory.Path;
                RefreshDirectoryContent();
            }
        }

        private void DrawContentArea()
        {
            _contentAreaScrollPosition = GUILayout.BeginScrollView(_contentAreaScrollPosition);
            GUILayout.BeginVertical();
            DrawContentHead();
            DrawContentBody();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawContentHead()
        {
            GUILayout.BeginHorizontal();
            Rect nameRect = new Rect(_rowIndent, 0, _filePropertyWidth * 1.5f, _rowHeight);
            Rect sizeRect = new Rect(_rowIndent + _filePropertyWidth * 1.5f, 0, _filePropertyWidth * 0.75f, _rowHeight);
            Rect typeRect = new Rect(_rowIndent + _filePropertyWidth * 2.25f, 0, _filePropertyWidth * 0.75f, _rowHeight);
            Rect timestampRect = new Rect(_rowIndent + _filePropertyWidth * 3, 0, _filePropertyWidth, _rowHeight);

            GUI.Label(nameRect, "名称", "contentHeadStyle");
            GUI.Label(sizeRect, "大小", "contentHeadStyle");
            GUI.Label(typeRect, "类型", "contentHeadStyle");
            GUI.Label(timestampRect, "时间", "contentHeadStyle");
            GUILayout.Space(_filePropertyWidth * 4.2f);
            GUILayout.EndHorizontal();
        }

        private void DrawContentBody()
        {
            GUILayout.Space(_rowHeight);
            if (_currentDirectory == null)
            {
                return;
            }
            RefreshDirectoryContentList();
            _contentBodyScrollPositon = GUILayout.BeginScrollView(_contentBodyScrollPositon);
            for (int i = 0; i < _directoryContentList.Count; i++)
            {
                DrawEntryClickArea(i);
                DrawEntry(i);
            }
            GUILayout.Space(_rowHeight * _directoryContentList.Count);
            GUILayout.EndScrollView();
        }

        private void RefreshDirectoryContentList()
        {
            if (_needRefreshDirectoryContentList == false)
            {
                return;
            }
            _needRefreshDirectoryContentList = false;
            _directoryContentList.Clear();
            //最外层文件夹特殊处理，因为没有操作权限
            if (_currentDirectory.Name == FileViewer.ProductName)
            {
#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR)
                string persistentPath = Application.persistentDataPath;
                string cachePath = Application.temporaryCachePath;
                string[] pathArray = new string[] { persistentPath, cachePath };
                _directoryContentList.AddRange(FileViewer.GetFolderInfoList(pathArray, _currentDirectory.Path));
                return;
#endif
            }

            try
            {
                string[] subDirectoryPaths = Directory.GetDirectories(_currentDirectory.Path, "*", SearchOption.TopDirectoryOnly);
                string[] filePaths = Directory.GetFiles(_currentDirectory.Path, "*", SearchOption.TopDirectoryOnly);
                _directoryContentList.AddRange(FileViewer.GetFolderInfoList(subDirectoryPaths, _currentDirectory.Path));
                _directoryContentList.AddRange(FileViewer.GetFileInfoList(filePaths, _currentDirectory.Path));
            }
            catch (System.Exception e)
            {
                Logger.LogError("获取文件夹或文件信息异常：" + e.Message);
            }
        }

        private void DrawEntryClickArea(int index)
        {
            Rect buttonRect = new Rect(_rowIndent, _rowHeight * index, _filePropertyWidth * 4, _rowHeight);
            SetEntryStyle(index);
            if (GUI.Button(buttonRect, "", _entryStyle))
            {
                _currentEntry = _directoryContentList[index];
                _pathToDisplay = _currentEntry.Path;
                _lastClickedTime = Time.time;
                if (_entryClicked && _lastClickedId == _currentEntry.Id)
                {
                    //双击事件
                    _lastClickedId = -1;
                    _entryClicked = false;
                    OnEntryDoubleClick();
                }
                else
                {
                    _lastClickedId = _currentEntry.Id;
                    _entryClicked = true;
                }
            }
        }

        private void SetEntryStyle(int index)
        {
            if (_currentEntry == _directoryContentList[index])
            {
                _entryStyle = GUI.skin.GetStyle("entrySelectedStyle");
            }
            else if (index % 2 == 0)
            {
                _entryStyle = GUI.skin.GetStyle("entryEvenRowStyle");
            }
            else
            {
                _entryStyle = GUI.skin.GetStyle("entryOddRowStyle");
            }
        }

        private void OnEntryDoubleClick()
        {
            if (_currentEntry == null)
            {
                ShowMessage("未选中任何文件，不能执行操作！");
                return;
            }

            if (_currentEntry.Type == "文件夹")
            {
                for (int i = 0; i < _directoryList.Count; i++)
                {
                    Folder item = _directoryList[i];
                    if (_currentEntry.Path == item.Path)
                    {
                        _currentDirectory = item;
                        RefreshDirectoryContent();
                        (_currentDirectory.Parent as Folder).IsFolded = false;
                        break;
                    }
                }
                return;
            }
            //查看文件
            try
            {
                _fileContent = File.ReadAllText(_currentEntry.Path);
                if (_fileContent.Length > MAX_LENGTH_OF_TEXT_SHOW)
                {
                    ShowMessage("文件过大，请上传后查看！");
                    return;
                }
                _textWindowVisible = true;
            }
            catch (System.Exception e)
            {
                ShowMessage(e.Message);
            }
        }

        private void DrawEntry(int index)
        {
            string size = "";
            if (_directoryContentList[index].Type != "文件夹")
            {
                size = string.Format("{0:f2} KB", _directoryContentList[index].Length / 1024f);
            }
            float yPos = _rowHeight * index;
            Rect nameRect = new Rect(_rowIndent, yPos, _filePropertyWidth * 1.5f, _rowHeight);
            Rect sizeRect = new Rect(_rowIndent + _filePropertyWidth * 1.5f, yPos, _filePropertyWidth * 0.75f, _rowHeight);
            Rect typeRect = new Rect(_rowIndent + _filePropertyWidth * 2.25f, yPos, _filePropertyWidth * 0.75f, _rowHeight);
            Rect timestampRect = new Rect(_rowIndent + _filePropertyWidth * 3, yPos, _filePropertyWidth, _rowHeight);
            GUI.Label(nameRect, _directoryContentList[index].Name);
            GUI.Label(sizeRect, size);
            GUI.Label(typeRect, _directoryContentList[index].Type);
            GUI.Label(timestampRect, FileViewer.FormatDateTime(_directoryContentList[index].LastWriteTime));
        }

        private void DrawTextWindow(int windowId)
        {
            GUILayout.BeginVertical();
            if (GUI.Button(_closeBtnRect, "", "closeBtnStyle"))
            {
                _textWindowVisible = false;
            }
            GUILayout.Space(_topPadding);
            _textScrollPosition = GUILayout.BeginScrollView(_textScrollPosition);
            GUILayout.Label(_fileContent, "textAreaStyle");
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void Upload()
        {
            if (_currentEntry == null)
            {
                ShowMessage("没有选中任何对象，无法执行上传！");
                return;
            }

            if (_currentEntry.Type == "文件夹")
            {
                ShowMessage("只能上传文件，当前选中对象为文件夹，请重新选择！");
                return;
            }

            if (_fileUploader == null)
            {
                _fileUploader = new FileUploader();
            }
            _fileUploader.Upload(_currentEntry.Path);
        }

        private void RefreshDirectoryContent()
        {
            _needRefreshDirectoryContentList = true;
            if (_currentEntry != null && _currentEntry.ParentPath != _currentDirectory.Path)
            {
                _currentEntry = null;
            }
        }

        private void Delete()
        {
            if (_currentEntry == null)
            {
                ShowMessage("没有选中任何对象，无法执行删除！");
                return;
            }
            _confirmWindowVisible = true;
        }

        private void RefreshDirectory()
        {
            TreeUtility.TreeToList<Folder>(_directoryRoot, _directoryList);
            RefreshDirectoryContent();
        }

        private void DrawConfirmWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(_confirmWindowSize.y * 0.3f);
            GUILayout.Label("确定删除?", "messageAreaStyle");
            GUILayout.Space(_confirmWindowSize.y * 0.2f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("是"))
            {
                ExecuteDelete();
                _confirmWindowVisible = false;
            }

            if (GUILayout.Button("否"))
            {
                _confirmWindowVisible = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void ExecuteDelete()
        {
            try
            {
                FileViewer.Delete(_currentDirectory, _currentEntry, RefreshDirectory);
            }
            catch (System.Exception e)
            {
                ShowMessage(e.Message);
            }
        }

        private void ShowMessage(string message)
        {
            _messageStartDisplayTime = Time.time;
            _messageToShow = message;
            _messageWindowVisible = true;
        }

        private void DrawMessageWindow(int windowId)
        {
            GUILayout.Space(0.3f * _messageWindowSize.y);
            GUILayout.Label(_messageToShow, "messageAreaStyle");
        }

        private void UpdateFlag()
        {
            if (_lastClickedTime + 0.5 < Time.time)
            {
                _lastClickedId = -1;
                _entryClicked = false;
            }

            if (_messageStartDisplayTime + 3 < Time.time)
            {
                _messageWindowVisible = false;
            }
        }
    }
}