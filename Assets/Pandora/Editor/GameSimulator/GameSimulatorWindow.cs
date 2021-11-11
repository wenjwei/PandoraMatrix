using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System;
using UnityEngine.UI;


namespace com.tencent.pandora
{
    public class GameSimulatorWindow : EditorWindow
    {
        #region 变量
        Dictionary<string, string> _protocolDict = new Dictionary<string, string>();
        Dictionary<string, string> _functionDict = new Dictionary<string, string>();

        List<string> _receivedMessageList = new List<string>();
        List<string> _sendedMessageList = new List<string>();
        public static List<string> currentDisplayMessageList = new List<string>();
        string[] _tabNames = new string[] { "发出的消息", "接收到的消息", "清空" };
        int _selectedTab = 0;

        const string CONFIG_FILE_PATH = "Pandora/Editor/GameSimulator/simulatorConfig.txt";
        const string CLOSE_TEXTURE_PATH = "Assets/Pandora/Editor/GameSimulator/closeTexture.png";
        const string WINDOW_CONFIG_AREA_TITLE = "配置";
        const string PROTOCOL_BUTTON_LIST_TITLE = "协议测试";
        const string FUNCTION_BUTTON_LIST_TITLE = "函数测试";
        const string ADD_NEW_PROTOCOL_AREA_TITLE = "新增协议";
        const string ADD_NEW_FUNCTION_AREA_TITLE = "新增函数";
        const string MESSAGE_AREA_TITLE = "消息区";
        string _newProtocolName = string.Empty;
        string _newProtocolContent = string.Empty;

        string _newFunctionButtonName = string.Empty;
        string _newFunctionName = string.Empty;

        Texture closeTexture;
        Vector2 _protocolButtonScrollPosition = Vector2.zero;
        Vector2 _functionButtonScrollPosition = Vector2.zero;
        Vector2 _currentMessageScrollPosition = Vector2.zero;
        Vector2 _totalScrollPosition = Vector2.zero;

        GameObject rootGameObject;
        bool _isSetGameCallback = false;
        bool _isHorizontalLayout = true;
        bool _autoOpen = true;
        #endregion

        [MenuItem("PandoraTools/SDKHelpTools/GameSimulator")]
        private static void Init()
        {
            GameSimulatorWindow simulatorWindow = (GameSimulatorWindow)EditorWindow.GetWindow(typeof(GameSimulatorWindow), false, "GameSimulator");
            simulatorWindow.Show(true);
        }

        //脚本重新编译就会触发这个，所以这里可以读取存储的数据，以防数据丢失
        private void OnEnable()
        {
            ReadConfig();
        }

        private void ReadConfig()
        {
            string configPath = Path.Combine(Application.dataPath, CONFIG_FILE_PATH);
            if (!File.Exists(configPath))
            {
                return;
            }
            string configJson = File.ReadAllText(configPath);
            Dictionary<string, System.Object> configDict = MiniJSON.Json.Deserialize(configJson) as Dictionary<string, System.Object>;
            Dictionary<string, string> targetDict = new Dictionary<string, string>();
            Dictionary<string, System.Object> tmpDict;
            foreach (var item in configDict)
            {
                switch (item.Key)
                {
                    case "_protocolDict":
                        targetDict = _protocolDict;
                        break;
                    case "_functionDict":
                        targetDict = _functionDict;
                        break;
                    default:
                        break;
                }
                tmpDict = item.Value as Dictionary<string, System.Object>;
                foreach (var innerItem in tmpDict)
                {
                    targetDict[innerItem.Key] = innerItem.Value as string;
                }

            }
        }

        private void OnFocus()
        {
            if (!Application.isPlaying)
            {
                _isSetGameCallback = false;
                return;
            }
            if (Application.isPlaying && _isSetGameCallback == false)
            {
                Debug.Log("GameSimulator设置gamecallback");
                _isSetGameCallback = true;
                Pandora.Instance.SetJsonGameCallback(OnJsonPandoraEvent);
            }
        }

        private void OnJsonPandoraEvent(string jsonMessage)
        {
            Logger.Log("接收收到的Pandora消息：" + jsonMessage);
            _receivedMessageList.Add(jsonMessage + "\r\n");
            AdjustMessageListCapacity(ref _receivedMessageList);
        }

        private void OnGUI()
        {
            _isHorizontalLayout = PlayerPrefs.GetString("isHorizontalLayout") == "False" ? false : true;
            if (_isHorizontalLayout)
            {
                HorizontalLayout();
            }
            else
            {
                VerticalLayout();
            }
        }

        private void HorizontalLayout()
        {
            EditorGUILayout.BeginVertical();
            _totalScrollPosition = EditorGUILayout.BeginScrollView(_totalScrollPosition);
            EditorGUILayout.BeginHorizontal();
            DrawWindowConfigArea();
            DrawProtocolButtonList();
            DrawFunctionButtonList();
            DrawAddNewProtocolArea();
            DrawAddNewFunctionArea();
            EditorGUILayout.EndHorizontal();
            DrawSpaceLine(3);
            DrawMessageArea();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void VerticalLayout()
        {
            EditorGUILayout.BeginVertical();
            _totalScrollPosition = EditorGUILayout.BeginScrollView(_totalScrollPosition);
            DrawWindowConfigArea();
            DrawSpaceLine(3);
            DrawProtocolButtonList();
            DrawSpaceLine(3);
            DrawFunctionButtonList();
            DrawSpaceLine(3);
            DrawAddNewProtocolArea();
            DrawSpaceLine(3);
            DrawAddNewFunctionArea();
            DrawSpaceLine(3);
            DrawMessageArea();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawWindowConfigArea()
        {
            EditorGUILayout.BeginVertical();
            DrawTitle(WINDOW_CONFIG_AREA_TITLE);

            _isHorizontalLayout = EditorGUILayout.Toggle("HorizontalLayout", _isHorizontalLayout);
            PlayerPrefs.SetString("isHorizontalLayout", _isHorizontalLayout.ToString());

            //自动打开窗口配置
            _autoOpen = PlayerPrefs.GetString("autoOpen") == "False" ? false : true;
            _autoOpen = EditorGUILayout.Toggle("AutoOpenWindow", _autoOpen);
            PlayerPrefs.SetString("autoOpen", _autoOpen.ToString());

            if (GUILayout.Button("GenerateScene", GUILayout.Height(40)))
            {
                GenerateScence();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawProtocolButtonList()
        {
            DrawButtonList(PROTOCOL_BUTTON_LIST_TITLE, ref _protocolButtonScrollPosition, ref _protocolDict);
        }

        private void DrawFunctionButtonList()
        {
            DrawButtonList(FUNCTION_BUTTON_LIST_TITLE, ref _functionButtonScrollPosition, ref _functionDict);
        }

        private void DrawButtonList(string title, ref Vector2 scrollPosition, ref Dictionary<string, string> configDict)
        {
            if (closeTexture == null)
            {
                closeTexture = (Texture)AssetDatabase.LoadAssetAtPath(CLOSE_TEXTURE_PATH, typeof(Texture));
                if (closeTexture == null)
                {
                    DisplayWarningDialog("closeTexture 加载失败");
                    return;
                }
            }
            EditorGUILayout.BeginVertical();
            DrawTitle(title);
            if (_isHorizontalLayout)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
            }
            List<string> configKeysList = new List<string>(configDict.Keys);
            foreach (var item in configKeysList)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item, GUILayout.Height(40)))
                {
                    if (title == PROTOCOL_BUTTON_LIST_TITLE)
                    {
                        OnProtocolButtonClick(item);
                    }
                    else
                    {
                        OnFunctionButtonClick(item);
                    }
                }
                if (GUILayout.Button(closeTexture, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    OnDeleteButton(item, ref configDict);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTitle(string title)
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField(title + "：", titleStyle);
            DrawSpaceLine(2);
        }

        private void OnProtocolButtonClick(string buttonName)
        {
            string protocol = _protocolDict[buttonName];
            Pandora.Instance.DoJson(protocol);
            _sendedMessageList.Add(protocol + "\r\n");
            AdjustMessageListCapacity(ref _sendedMessageList);
        }

        private void OnFunctionButtonClick(string buttonName)
        {
            string functionName = _functionDict[buttonName];
            GetRootGameObject();
            //sendMessage方法
            if (rootGameObject == null)
            {
                return;
            }
            rootGameObject.SendMessage(functionName);
        }

        private void GetRootGameObject()
        {
            if (rootGameObject == null)
            {
#if USING_NGUI
                rootGameObject = GameObject.Find("UI Root");
                if (rootGameObject == null)
                {
                    DisplayWarningDialog("场景中没有UI Root，请检查");
                }
#endif
#if USING_UGUI
                rootGameObject = GameObject.Find("Canvas");
                if (rootGameObject == null)
                {
                    DisplayWarningDialog("场景中没有Canvas，请检查");
                }
#endif
            }
        }

        private void OnDeleteButton(string buttonName, ref Dictionary<string, string> configDict)
        {
            configDict.Remove(buttonName);
            //写入
            WriteSimulatorConfig();
        }

        private void WriteSimulatorConfig()
        {
            Dictionary<string, System.Object> configDict = new Dictionary<string, object>();
            configDict.Add("_protocolDict", _protocolDict);
            configDict.Add("_functionDict", _functionDict);
            string configJson = MiniJSON.Json.Serialize(configDict);
            string configPath = Path.Combine(Application.dataPath, CONFIG_FILE_PATH);
            File.WriteAllText(configPath, configJson);
            AssetDatabase.Refresh();
        }

        private void DrawAddNewProtocolArea()
        {
            EditorGUILayout.BeginVertical();
            DrawTitle(ADD_NEW_PROTOCOL_AREA_TITLE);
            DrawTextArea("协议名称:(例:Open PandoraToolBox)", ref _newProtocolName, 25);
            DrawTextArea("协议内容:(例:type:open;content:pandoraToolBox)", ref _newProtocolContent, 100);
            if (GUILayout.Button("新增", GUILayout.Width(50)))
            {
                AddNewProtocol();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTextArea(string title, ref string content, float textAreaHeight)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title);
            content = EditorGUILayout.TextArea(content, GUILayout.Height(textAreaHeight));
            EditorGUILayout.EndVertical();
        }

        private void AddNewProtocol()
        {
            if (_newProtocolName == "")
            {
                DisplayWarningDialog("协议名称没有填写，请填写");
                return;
            }

            if (_newProtocolContent == "")
            {
                DisplayWarningDialog("协议内容没有填写，请填写");
                return;
            }
            Dictionary<string, string> newProtocol = new Dictionary<string, string>();
            string[] splitInput = _newProtocolContent.Split(new char[] { ';' });
            foreach (var item in splitInput)
            {
                //针对value值为json串的做个特殊处理
                int index = item.IndexOf(":");
                if (index == -1)
                {
                    DisplayWarningDialog("协议输入格式有误，请修正。");
                    return;
                }
                string key = item.Substring(0, index);
                string value = item.Substring(index + 1);
                value = value.Replace("\r", "");

                if (newProtocol.ContainsKey(key))
                {
                    DisplayWarningDialog("协议中键值有重复，请修正。" + key);
                    return;
                }
                newProtocol.Add(key, value);
            }
            string jsonProtocol = MiniJSON.Json.Serialize(newProtocol);
            if (_protocolDict.ContainsKey(_newProtocolName))
            {
                DisplayWarningDialog("该协议已经添加过，不能重复添加 " + _newProtocolName);
                return;
            }
            _protocolDict.Add(_newProtocolName, jsonProtocol);
            WriteSimulatorConfig();
        }

        private void DrawAddNewFunctionArea()
        {
            EditorGUILayout.BeginVertical();
            DrawTitle(ADD_NEW_FUNCTION_AREA_TITLE);
            DrawTextArea("按钮名称:(例:Logout)", ref _newFunctionButtonName, 25);
            DrawTextArea("函数名:(例:Logout)", ref _newFunctionName, 25);
            if (GUILayout.Button("新增", GUILayout.Width(50)))
            {
                AddNewFunction();
            }
            EditorGUILayout.EndVertical();
        }

        private void AddNewFunction()
        {
            if (_newFunctionButtonName == "")
            {
                DisplayWarningDialog("按钮名称没有填写，请填写");
                return;
            }

            if (_newFunctionName == "")
            {
                DisplayWarningDialog("函数名没有填写，请填写");
                return;
            }

            if (_functionDict.ContainsKey(_newFunctionButtonName))
            {
                DisplayWarningDialog("该按钮已经添加过，不能重复添加 " + _newFunctionButtonName);
                return;
            }
            _functionDict.Add(_newFunctionButtonName, _newFunctionName);
            WriteSimulatorConfig();

        }

        private void DrawMessageArea()
        {
            GUILayout.BeginVertical();
            DrawTitle(MESSAGE_AREA_TITLE);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            switch (_selectedTab)
            {
                case 0:
                    currentDisplayMessageList = _sendedMessageList;
                    break;
                case 1:
                    currentDisplayMessageList = _receivedMessageList;
                    break;
                case 2:
                    _receivedMessageList.Clear();
                    _sendedMessageList.Clear();
                    currentDisplayMessageList.Clear();
                    break;
                default:
                    break;
            }
            DrawScrollTextArea(ref currentDisplayMessageList, ref _currentMessageScrollPosition, 80);
            GUILayout.EndVertical();
        }

        private void DrawScrollTextArea(ref List<string> content, ref Vector2 scroolPosition, float height)
        {
            scroolPosition = EditorGUILayout.BeginScrollView(scroolPosition, GUILayout.Height(height));
            EditorGUILayout.TextArea(ConcatenateStringFromList(content));

            EditorGUILayout.EndScrollView();
        }

        private void DrawSpaceLine(int rows)
        {
            for (int i = 0; i < rows; i++)
            {
                EditorGUILayout.Space();
            }
        }



        private void AdjustMessageListCapacity(ref List<string> messageList)
        {
            if (messageList.Count > 20)
            {
                messageList.RemoveAt(0);
            }
        }

        private string ConcatenateStringFromList(List<string> content)
        {
            string result = string.Empty;
            for (int i = 0; i < content.Count; i++)
            {
                result += content[i];
            }
            return result;
        }

        #region 创建场景

        private void GenerateScence()
        {
            CreateSceen();
            SaveScene();
        }

        private void CreateSceen()
        {
#if USING_NGUI

            if (!GameObject.Find("UI Root"))
            {
                //使用ngui的函数，创建UIRoot
                UICreateNewUIWizard.CreateNewUI(UICreateNewUIWizard.CameraType.Simple2D);
                GameObject rootGameobject = GameObject.Find("UI Root");
                rootGameobject.AddComponent<GameSimulator>();

                //如果已经使用Autoconfig 配置了参数，则使用配置好的参数。
                Dictionary<string, string> pandoraConfig = ReadPandoraConfig();
                if (pandoraConfig != null)
                {
                    string layer = pandoraConfig["UI"];
                    AddLayer(layer);
                    rootGameobject.layer = LayerMask.NameToLayer(layer);

                    //相机
                    GameObject cameraGameobject = rootGameobject.transform.Find("Camera").gameObject;
                    cameraGameobject.layer = LayerMask.NameToLayer(layer);
                    Camera camera = cameraGameobject.GetComponent<Camera>();
                    //这里必须使用GetMask函数
                    camera.cullingMask = LayerMask.GetMask(layer);

                    UICamera uiCamera = cameraGameobject.GetComponent<UICamera>();
                    if (System.Enum.IsDefined(typeof(UICamera.EventType), pandoraConfig["UI_3D"]))
                    {
                        uiCamera.eventType = (UICamera.EventType)System.Enum.Parse(typeof(UICamera.EventType), pandoraConfig["UI_3D"]);
                    }
                }
            }
#endif
#if USING_UGUI
            if (!GameObject.Find("Canvas"))
            {
                GameObject rootGameObject = new GameObject("Canvas");
                Canvas canvas = rootGameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = rootGameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1136, 640);
                rootGameObject.AddComponent<GraphicRaycaster>();
                rootGameObject.AddComponent<GameSimulator>();

                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
#endif

        }

#if USING_NGUI
        private Dictionary<string, string> ReadPandoraConfig()
        {
            string configPath = Path.Combine(Application.dataPath, "Pandora/Editor/Autoconfig/config.txt");
            if (!File.Exists(configPath))
            {
                return null;
            }
            string configJson = File.ReadAllText(configPath);
            Dictionary<string, System.Object> configDict = MiniJSON.Json.Deserialize(configJson) as Dictionary<string, System.Object>;
            Dictionary<string, System.Object> tmpDict = configDict["_pandoraConfigDict"] as Dictionary<string, System.Object>;
            Dictionary<string, string> pandoraConfig = new Dictionary<string, string>();
            foreach (var innerItem in tmpDict)
            {
                if (!pandoraConfig.ContainsKey(innerItem.Key))
                {
                    pandoraConfig[innerItem.Key] = innerItem.Value as string;
                }
            }
            return pandoraConfig;
        }

        private void AddLayer(string layerName)
        {
            if (!IsHasLayer(layerName))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty iter = tagManager.GetIterator();
                while (iter.NextVisible(true))
                {
                    if (iter.name == "layers")
                    {
                        SerializedProperty item;
                        for (int i = 8; i < iter.arraySize; i++)
                        {
                            item = iter.GetArrayElementAtIndex(i);
                            if (string.IsNullOrEmpty(item.stringValue))
                            {
                                item.stringValue = layerName;
                                tagManager.ApplyModifiedProperties();
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool IsHasLayer(string layerName)
        {
            string[] layerNames = UnityEditorInternal.InternalEditorUtility.layers;
            foreach (var item in layerNames)
            {
                if (item == layerName)
                {
                    return true;
                }
            }
            return false;
        }
#endif



        private void SaveScene()
        {
            string directoryPath = Path.Combine(Application.dataPath, "Scene");
            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }
            string savePath = Path.Combine(directoryPath, "GameSimulator.unity");
#if UNITY_4_6 || UNITY_4_7
            EditorApplication.SaveScene(savePath);
#else
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), savePath);
#endif

            AssetDatabase.Refresh();
#if USING_NGUI
            DisplayWarningDialog("场景已自动生成和保存，请手动配置UIRoot的scallingStyle参数");
#endif
#if USING_UGUI
            DisplayWarningDialog("场景已自动生成和保存，Canvas Scaler的默认屏幕参考分辨率为1136*640，如不符合业务的设定，请自行更改。");
#endif
        }
        #endregion

        private void DisplayWarningDialog(string message, string title = "")
        {
            EditorUtility.DisplayDialog(title, message, "我知道了");
        }

    }

    [InitializeOnLoad]
    public static class PlayModeStateChangedNotice
    {
        static PlayModeStateChangedNotice()
        {
#if UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_2017_1
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
#elif UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
#endif
        }

#if UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_2017_1
        private static void OnPlaymodeStateChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                bool autoOpenGameSimulator = PlayerPrefs.GetString("autoOpen") == "False" ? false : true;
                if (autoOpenGameSimulator == true)
                {
                    EditorWindow.FocusWindowIfItsOpen(typeof(com.tencent.pandora.GameSimulatorWindow));
                }
                GameSimulatorWindow.currentDisplayMessageList.Clear();
            }
        }
#elif UNITY_2017_2_OR_NEWER
         private static void OnPlaymodeStateChanged(PlayModeStateChange mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                bool autoOpenGameSimulator = PlayerPrefs.GetString("autoOpen") == "False" ? false : true;
                if (autoOpenGameSimulator == true)
                {
                    EditorWindow.FocusWindowIfItsOpen(typeof(com.tencent.pandora.GameSimulatorWindow));
                }
                GameSimulatorWindow.currentDisplayMessageList.Clear();
            }
        }
#endif

    }

}
