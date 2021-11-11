using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace com.tencent.pandora
{
    public class AutoConfigWindow : EditorWindow
    {
        #region 参数
        //ErrorCodeConfig.cs 的配置
        private Dictionary<string, string> _errorCodeConfigDict = new Dictionary<string, string>()
    {
        {"ASSET_PARSE_FAILED",""},
        {"MD5_VALIDATE_FAILED",""},
        {"FILE_WRITE_FAILED",""},
        {"FILE_WRITE_FAILED_IOS",""},
        {"ASSET_LOAD_FAILED",""},
        {"META_READ_FAILED",""},
        {"META_WRITE_FAILED",""},
        {"META_WRITE_FAILED_IOS",""},
        {"CGI_TIMEOUT",""},
        {"CGI_TIMEOUT_IOS",""},
        {"GAME_2_PANDORA_EXCEPTION",""},
        {"EXECUTE_LUA_CALLBACK_EXCEPTION",""},
        {"LUA_SCRIPT_EXCEPTION",""},
        {"LUA_DO_FILE_EXCEPTION",""},
        {"EXECUTE_ENTRY_LUA",""},
        {"PANDORA_2_GAME_EXCEPTION",""},
        {"SAME_PANEL_EXISTS",""},
        {"PANEL_PARENT_INEXISTS",""},
        {"COOKIE_WRITE_FAILED",""},
        {"COOKIE_READ_FAILED",""},
        {"DELETE_FILE_FAILED",""},
        {"START_RELOAD",""},
        {"CGI_CONTENT_ERROR",""},
        {"LUA_SCRIPT_EXCEPTION_DETAIL",""},
        {"CGI_TIMEOUT_DETAIL",""},
        {"PANDORA_2_GAME_EXCEPTION_DETAIL",""},
        {"CGI_CONTENT_ERROR_DETAIL",""},

    };

        private Dictionary<string, string> _errorCodeConfigDescriptionDict = new Dictionary<string, string>()
    {
       {"ASSET_PARSE_FAILED","资源解析失败"},
        {"MD5_VALIDATE_FAILED","资源Md5码校验失败"},
        {"FILE_WRITE_FAILED","文件写入本地失败"},
        {"FILE_WRITE_FAILED_IOS","文件写入本地失败_ios"},
        {"ASSET_LOAD_FAILED","资源加载失败"},
        {"META_READ_FAILED","资源Meta文件读取失败"},
        {"META_WRITE_FAILED","资源Meta文件更新失败"},
        {"META_WRITE_FAILED_IOS","资源Meta文件更新失败_ios"},
        {"CGI_TIMEOUT","Cgi访问失败且超过最大重试次数"},
        {"CGI_TIMEOUT_IOS","Cgi访问失败且超过最大重试次数_ios"},
        {"GAME_2_PANDORA_EXCEPTION","Lua执行游戏传递的消息失败"},
        {"EXECUTE_LUA_CALLBACK_EXCEPTION","执行Lua回调发生异常"},
        {"LUA_SCRIPT_EXCEPTION","Lua脚本执行发生异常"},
        {"LUA_DO_FILE_EXCEPTION","Lua文件解析异常"},
        {"EXECUTE_ENTRY_LUA","开始执行模块入口Lua文件"},
        {"PANDORA_2_GAME_EXCEPTION","游戏执行Lua消息发生异常"},
        {"SAME_PANEL_EXISTS","创建面板时已存在同名面板"},
        {"PANEL_PARENT_INEXISTS","面板的父节点不存在"},
        {"COOKIE_WRITE_FAILED","Cookie写入失败"},
        {"COOKIE_READ_FAILED","Cookie读取失败"},
        {"DELETE_FILE_FAILED","删除文件失败"},
        {"START_RELOAD","记录用户主动重连请求"},
        {"CGI_CONTENT_ERROR","CGI内容出错"},
        {"LUA_SCRIPT_EXCEPTION_DETAIL","Lua脚本执行异常详情"},
        {"CGI_TIMEOUT_DETAIL","CGI错误详情"},
        {"PANDORA_2_GAME_EXCEPTION_DETAIL","游戏回调执行Pandora消息发生异常详情"},
        {"CGI_CONTENT_ERROR_DETAIL","CGI内容出错详情"},
    };

        //Pandora.cs的配置
        private Dictionary<string, string> _pandoraConfigDict = new Dictionary<string, string>()
    {
#if USING_NGUI
        {"UI",""},
        {"UI_3D",""},
#endif
        {"Font/",""},
        {"test2.broker.tplay.qq.com",""},
        {"10023",""},
        {"speedm.broker.tplay.qq.com",""},
        {"5692",""},
        {"182.254.42.60",""},
        {"182.254.74.52",""},

    };

        private Dictionary<string, string> _pandoraConfigDescriptionDict = new Dictionary<string, string>()
    {
#if USING_NGUI
        {"UI","UIRoot的layer设置"},
        {"UI_3D","UICarema的EventType类型"},
#endif
        {"Font/","字体所在目录相对于Resources的路径"},
        {"test2.broker.tplay.qq.com","BrokerHost-测试"},
        {"10023","BrokerPort-测试"},
        {"speedm.broker.tplay.qq.com","BrokerHost-正式"},
        {"5692","BrokerPort-正式"},
        {"182.254.42.60","BrokerIp1-正式"},
        {"182.254.74.52","BrokerIp2-正式"},
    };

        private Dictionary<string, string> _otherConfigDict = new Dictionary<string, string>()
    {
        {"GameName",""},
    };
        private Dictionary<string, string> _otherConfigDescriptionDict = new Dictionary<string, string>()
    {
        {"GameName","游戏代号"},
    };
        //配置的当前状态
        private Dictionary<string, string> _configStateDict = new Dictionary<string, string>()
    {
        {"hasConfiged","false"},
    };

        private bool _isHorizontalLayout = true;
        private Vector2 _errorCodeConfigScrollPosition = Vector2.zero;
        private Vector2 _pandoraConfigScrollPosition = Vector2.zero;
        private Vector2 _otherConfigScrollPosition = Vector2.zero;
        private Vector2 _totalScrollPosition = Vector2.zero;

        //路径
        private const string ERROR_CODE_CONFIG_PATH = "Pandora/Scripts/ErrorCodeConfig.cs";
        private const string PANDORA_PATH = "Pandora/Scripts/Pandora.cs";
        private const string PACKAGE_EXPORTER_PATH = "Pandora/Editor/PackageExporter/PackageExporter.cs";
        private const string CUSTOM_EXPORT_PATH = "Pandora/Slua/Editor/CustomExport.cs";

        private const string CONFIG_FILE_PATH = "Pandora/Editor/Autoconfig/config.txt";
        private const string FILE_BACKUP_FOLDER_PATH = "Pandora/Editor/Autoconfig/FileBackup";


        #endregion

        #region GUI
        [MenuItem("PandoraTools/SDKHelpTools/AutoConfig")]
        private static void Init()
        {
            AutoConfigWindow autoConfigWindow = (AutoConfigWindow)EditorWindow.GetWindow(typeof(AutoConfigWindow), false, "AutoConfig");
            autoConfigWindow.Show(true);
        }

        //打开AutoConfig和脚本修改时，此函数会运行
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
                    case "_errorCodeConfigDict":
                        targetDict = _errorCodeConfigDict;
                        break;
                    case "_pandoraConfigDict":
                        targetDict = _pandoraConfigDict;
                        break;
                    case "_otherConfigDict":
                        targetDict = _otherConfigDict;
                        break;
                    case "_configStateDict":
                        targetDict = _configStateDict;
                        break;
                    default:
                        break;
                }
                tmpDict = item.Value as Dictionary<string, System.Object>;
                foreach (var innerItem in tmpDict)
                {
                    if (targetDict.ContainsKey(innerItem.Key))
                    {
                        targetDict[innerItem.Key] = innerItem.Value as string;
                    }
                }

            }
        }


        //当操作界面时会被调用
        private void OnGUI()
        {
            SetLayoutDirection();
            if (_isHorizontalLayout)
            {
                HorizontalLayout();
            }
            else
            {
                VerticalLayout();
            }
        }

        private void SetLayoutDirection()
        {
            EditorGUILayout.BeginHorizontal();
            _isHorizontalLayout = EditorGUILayout.Toggle("HorizontalLayout", _isHorizontalLayout);
            EditorGUILayout.EndHorizontal();
        }

        //水平布局
        private void HorizontalLayout()
        {
            EditorGUILayout.BeginVertical();
            _totalScrollPosition = EditorGUILayout.BeginScrollView(_totalScrollPosition);
            EditorGUILayout.BeginHorizontal();
            DrawErrorCodeConfigList();
            DrawPandoraConfigList();
            DrawOtherConfigList();
            EditorGUILayout.EndHorizontal();
            DrawSpaceLine(3);
            DrawConfigButton();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        //竖直布局
        private void VerticalLayout()
        {
            EditorGUILayout.BeginVertical();
            _totalScrollPosition = EditorGUILayout.BeginScrollView(_totalScrollPosition);
            DrawErrorCodeConfigList();
            DrawSpaceLine(3);
            DrawPandoraConfigList();
            DrawOtherConfigList();
            DrawSpaceLine(3);
            DrawConfigButton();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawErrorCodeConfigList()
        {
            EditorGUILayout.BeginVertical();
            DrawConfigList("ErrorCodeConfig.cs 的配置", ref _errorCodeConfigScrollPosition, ref _errorCodeConfigDict, ref _errorCodeConfigDescriptionDict);
            DrawSpaceLine(3);
            EditorGUILayout.LabelField("提示：如果错误码是顺序增长的，只需填充第一个，点击“自动填充”按钮即可");
            DrawAutoFillButton();
            EditorGUILayout.EndVertical();
        }

        private void DrawAutoFillButton()
        {
            if (GUILayout.Button("自动填充"))
            {
                if (_errorCodeConfigDict["ASSET_PARSE_FAILED"].ToString() == "")
                {
                    AutoConfig.DisplayWarningDialog("第一个错误码还没填写，请先填写第一个才能启用自动填充");
                    return;
                }

                int errorCode = int.Parse(_errorCodeConfigDict["ASSET_PARSE_FAILED"]);
                List<string> errorCodeConfigList = new List<string>(_errorCodeConfigDict.Keys);
                int count = errorCodeConfigList.Count;
                for (int i = 0; i < count; i++, errorCode++)
                {
                    _errorCodeConfigDict[errorCodeConfigList[i]] = errorCode.ToString();
                }
            }
        }

        private void DrawPandoraConfigList()
        {
            EditorGUILayout.BeginVertical();
            DrawConfigList("Pandora.cs的配置", ref _pandoraConfigScrollPosition, ref _pandoraConfigDict, ref _pandoraConfigDescriptionDict);
            EditorGUILayout.EndVertical();
        }

        private void DrawOtherConfigList()
        {
            EditorGUILayout.BeginVertical();
            DrawConfigList("其他配置", ref _otherConfigScrollPosition, ref _otherConfigDict, ref _otherConfigDescriptionDict);
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigList(string title, ref Vector2 scrollPosition, ref Dictionary<string, string> configDict, ref Dictionary<string, string> configDescriptionDict)
        {
            DrawTitle(title);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(180));
            List<string> configList = new List<string>(configDict.Keys);
            string key = "";
            int count = configList.Count;
            for (int i = 0; i < count; i++)
            {
                key = configList[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(configDescriptionDict[key] + ":");
                configDict[key] = EditorGUILayout.TextField(configDict[key]).Trim();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTitle(string title)
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField(title + "：", titleStyle);
            DrawSpaceLine(2);
        }

        private void DrawConfigButton()
        {
            Color originBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            string hasConfiged;
            _configStateDict.TryGetValue("hasConfiged", out hasConfiged);
            string buttonName = hasConfiged == "true" ? "重新配置" : "开始配置";
            //开始配置
            if (GUILayout.Button(buttonName, GUILayout.Height(50)))
            {

                Dictionary<string, System.Object> configDict = GetConfigDict();
                if (hasConfiged == "true")
                {
                    AutoConfig.Config(true, configDict);
                }
                else
                {
                    AutoConfig.Config(false, configDict);
                }
            }
            GUI.backgroundColor = originBackgroundColor;
        }

        private void DrawSpaceLine(int rows)
        {
            for (int i = 0; i < rows; i++)
            {
                EditorGUILayout.Space();
            }
        }

        private Dictionary<string, System.Object> GetConfigDict()
        {
            Dictionary<string, System.Object> configDict = new Dictionary<string, object>();
            configDict.Add("_errorCodeConfigDict", _errorCodeConfigDict);
            configDict.Add("_pandoraConfigDict", _pandoraConfigDict);
            configDict.Add("_otherConfigDict", _otherConfigDict);
            configDict.Add("_configStateDict", _configStateDict);
            return configDict;
        }

        #endregion
    }
}
