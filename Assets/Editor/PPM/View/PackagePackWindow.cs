using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Text;

using cPackage.Tools;
using Semver;

namespace PPM
{
    public class PackagePackWindow : EditorWindow
    {
        Rect _background = new Rect(0, 0, 1024, 720);
        Color _backgroundColor = new Color(45 / 255f, 45 / 255f, 48 / 255f);

        private GUIStyle _textFieldStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _toggleStyle;

        private static string _title = "插件制作";

        public static void OpenWindow()
        {
            PackagePackWindow window = GetWindow<PackagePackWindow>(true);
            window.minSize = new Vector2(650, 450);
            window.Init();
            window.ShowUtility();
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent(_title);
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = _title;
#else
            PPMHelper.LogError("this feature is not implemented in current unity");
#endif
        }

        private PPMConfiguration _configuration;

        private Dictionary<string, bool> _tagToggleDict = new Dictionary<string, bool>();

        private void Init()
        {
            InitGUIStyle();
            InitPPMConfiguration();
            InitToggleDict();
        }

        private void InitToggleDict()
        {
            if (_tagToggleDict.Count == 0)
            {
                _tagToggleDict.Add("SDK", false);
                _tagToggleDict.Add("工具", false);
                _tagToggleDict.Add("模版", false);
            }
        }

        private void InitPPMConfiguration()
        {
            if (_configuration == null)
            {
                _configuration = new PPMConfiguration();
                _configuration.PackageType = "Unity";
            }
        }

        private void InitGUIStyle()
        {
            Color _guiColorBlack = new Color(86 / 255f, 86 / 255f, 90 / 255f);

            _labelStyle = new GUIStyle(PPMHelper.IsProSkin() ? EditorStyles.label : EditorStyles.whiteLabel)
            {
                fontSize = 12
            };

            _textFieldStyle = new GUIStyle(EditorStyles.textField);
            _textFieldStyle.normal.background = PPMHelper.MakeTex(10, 10, _guiColorBlack);
            _textFieldStyle.hover.background = PPMHelper.MakeTex(10, 10, _guiColorBlack);
            _textFieldStyle.focused.background = PPMHelper.MakeTex(10, 10, _guiColorBlack);
            if (!PPMHelper.IsProSkin())
            {
                _textFieldStyle.normal.textColor = Color.white;
                _textFieldStyle.hover.textColor = Color.white;
                _textFieldStyle.focused.textColor = Color.white;
            }
            _textFieldStyle.fontSize = 12;
            _textFieldStyle.wordWrap = true;

            _toggleStyle = new GUIStyle(EditorStyles.toggle);
            if (!PPMHelper.IsProSkin())
            {
                _toggleStyle.normal.textColor = Color.white;
                _toggleStyle.focused.textColor = Color.white;
                _toggleStyle.hover.textColor = Color.white;
                _toggleStyle.onHover.textColor = Color.white;
                _toggleStyle.onNormal.textColor = Color.white;
                _toggleStyle.onFocused.textColor = Color.white;
                _toggleStyle.onActive.textColor = Color.white;
            }
        }

        void OnGUI()
        {
            EditorGUI.DrawRect(_background, _backgroundColor);
            DrawUI();
        }

        private void DrawUI()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("*选择文件:", _labelStyle);
            string btnName = "选择...";
            if (!string.IsNullOrEmpty(_configuration.ToPackPackagePath))
            {
                GUILayout.Label(Path.GetFileName(_configuration.ToPackPackagePath), _labelStyle);
            }
            if (GUILayout.Button(new GUIContent(btnName), GUILayout.Width(50)))
            {
                // string[] filters = { "cPackage files", "cpackage", "Unity package files", "unitypackage" };
                string[] filters = { "cPackage files", "cpackage" };
                string selectedPath = EditorUtility.OpenFilePanelWithFilters("file to pack", "", filters);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _configuration.ToPackPackagePath = selectedPath;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("*插件名称:", _labelStyle);
            _configuration.PackageName = TextField(_configuration.PackageName, _textFieldStyle, GUILayout.Width(500));
            if (EditorGUI.EndChangeCheck())
            {
                AutoFill(_configuration.PackageName);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("*插件版本:", _labelStyle);
            _configuration.PackageVersion = TextField(_configuration.PackageVersion, _textFieldStyle, GUILayout.Width(500));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 关闭更新通知:", _labelStyle);
            _configuration.DisableUpdateNotify = GUILayout.Toggle(_configuration.DisableUpdateNotify, "", _toggleStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 启用手动更新:", _labelStyle);
            _configuration.DisableAutoUpdate = GUILayout.Toggle(_configuration.DisableAutoUpdate, "", _toggleStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 插件描述:", _labelStyle);
            _configuration.PackageDescription = TextField(_configuration.PackageDescription, _textFieldStyle, GUILayout.Width(500), GUILayout.Height(80));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 标签信息:", _labelStyle);
            List<string> keys = new List<string>(_tagToggleDict.Keys);
            foreach (var key in keys)
            {
                _tagToggleDict[key] = GUILayout.Toggle(_tagToggleDict[key], key, _toggleStyle);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 依赖信息:", _labelStyle);
            GUILayout.BeginHorizontal(GUILayout.Width(500));
            _configuration.PackageDependencies = GUILayout.TextField(_configuration.PackageDependencies, _textFieldStyle, GUILayout.Width(400));
            if (GUILayout.Button("刷新包列表"))
            {
                // TO-DO 打开带分页包列表界面
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 版本日志:", _labelStyle);
            _configuration.PackageReleaseNote = TextField(_configuration.PackageReleaseNote, _textFieldStyle, GUILayout.Width(500), GUILayout.Height(80));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("打包", GUILayout.Height(30)))
            {
                string outputDir = Path.GetDirectoryName(_configuration.ToPackPackagePath);
                string outputPath = string.Format("{0}/{1}_v{2}.ppm", outputDir, _configuration.PackageName, _configuration.PackageVersion);
                if (Pack(outputPath))
                {
                    SavePackInfo(_configuration.PackageName);
                    PackageUploadWindow.OpenPackageUploadWindow(outputPath, false);
                    Close();
                }
            }
        }

        private void AutoFill(string packageName)
        {
            string ppmCacheKey = string.Format("ppm_auto_cache_{0}", packageName);
            string saveValue = PlayerPrefs.GetString(ppmCacheKey);
            if (!string.IsNullOrEmpty(saveValue))
            {
                PPMConfiguration conf = JsonConvert.DeserializeObject<PPMConfiguration>(saveValue);

                _configuration.PackageVersion = conf.PackageVersion;
                _configuration.DisableUpdateNotify = conf.DisableUpdateNotify;
                _configuration.DisableAutoUpdate = conf.DisableAutoUpdate;
                _configuration.PackageDescription = conf.PackageDescription;
                _configuration.PackageTags = conf.PackageTags;
                _configuration.PackageDependencies = conf.PackageDependencies;
                _configuration.PackageReleaseNote = conf.PackageReleaseNote;

                string[] tags = conf.PackageTags.Split(';');
                foreach (var tag in tags)
                {
                    if (_tagToggleDict.ContainsKey(tag))
                        _tagToggleDict[tag] = true;
                }
            }
        }

        private void SavePackInfo(string packageName)
        {
            string ppmCacheKey = string.Format("ppm_auto_cache_{0}", packageName);
            PlayerPrefs.SetString(ppmCacheKey, JsonConvert.SerializeObject(_configuration));
            PlayerPrefs.Save();
        }

        private bool IsVersionFormatValid()
        {
            SemVersion parsedVersion;
            SemVersion.TryParse(_configuration.PackageVersion, out parsedVersion, true);
            return parsedVersion != null;
        }

        private bool Pack(string outputPath)
        {
            if (!_configuration.IsDataReady())
            {
                EditorUtility.DisplayDialog("错误", "打包信息填写不完整，检查重填", "ok");
                return false;
            }

            if (!IsVersionFormatValid())
            {
                EditorUtility.DisplayDialog("错误", "版本号命名需要提供三位数，如 x.y.z", "ok");
                return false;
            }

            string tmpDir = Path.GetDirectoryName(_configuration.ToPackPackagePath) + "/ppm_package/";
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
            Directory.CreateDirectory(tmpDir);

            if (PPMHelper.IscPackage(_configuration.ToPackPackagePath))
                CollectPackageFileList(_configuration.ToPackPackagePath);
            CreateConfigurationFile(tmpDir + "ppm_package.json");
            File.Copy(_configuration.ToPackPackagePath, tmpDir + Path.GetFileName(_configuration.ToPackPackagePath));
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            cPackageHelper.Tar(tmpDir, outputPath);
            Directory.Delete(tmpDir, true);
            PPMHelper.Log(string.Format("插件制作成功: {0}", outputPath));
            return true;
        }

        private void CollectPackageFileList(string toPackPackagePath)
        {
            string tmpDir = string.Format("{0}/{1}/", Path.GetDirectoryName(toPackPackagePath), Path.GetFileNameWithoutExtension(toPackPackagePath));
            cPackageHelper.Decompress(toPackPackagePath, tmpDir);
            cPackageHelper.Untar(tmpDir + "temp.tar", tmpDir);
            string contentDir = tmpDir + "cPackage";
            foreach (var d in Directory.GetDirectories(contentDir))
            {
                PPMConfiguration.ResourceItem item = new PPMConfiguration.ResourceItem();
                item.GUID = Path.GetFileName(d);
                item.Path = File.ReadAllText(d + "/pathname");
                item.ContentMd5 = File.Exists(d + "/asset") ? cPackageHelper.CalculateMD5(d + "/asset") : "";
                item.MetaMd5 = cPackageHelper.CalculateMD5(d + "/asset.meta");
                _configuration.ResourceItemList.Add(item);
            }

            Directory.Delete(tmpDir, true);
        }

        private void CreateConfigurationFile(string outputFile)
        {
            _configuration.PackageTags = "";
            foreach (var tagToggle in _tagToggleDict)
            {
                if (tagToggle.Value)
                {
                    _configuration.PackageTags = string.IsNullOrEmpty(_configuration.PackageTags) ?
                        tagToggle.Key : string.Format("{0};{1}", _configuration.PackageTags, tagToggle.Key);
                }
            }

            _configuration.PackageAuthor = PackageManager.Instance.UserName;

            string content = JsonConvert.SerializeObject(_configuration, Formatting.Indented);
            File.WriteAllText(outputFile, content, Encoding.UTF8);
        }

        public static string HandleCopyPaste(int controlID)
        {
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			if (controlID == GUIUtility.keyboardControl)
			{
				if (Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
				{
					if (Event.current.keyCode == KeyCode.C)
					{
						Event.current.Use();
						TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						editor.Copy();
					}
					else if (Event.current.keyCode == KeyCode.V)
					{
						Event.current.Use();
						TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						editor.Paste();
						return editor.text;
					}
				}
			}
#endif
            return null;
        }

        public static string TextField(string value, GUIStyle style, params GUILayoutOption[] options)
        {
            int textFieldID = GUIUtility.GetControlID("TextArea".GetHashCode(), FocusType.Keyboard) + 1;
            if (textFieldID == 0)
                return value;

            // Handle custom copy-paste
            value = HandleCopyPaste(textFieldID) ?? value;

            return GUILayout.TextArea(value, style, options);
        }
    }
}