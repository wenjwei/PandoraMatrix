using System.IO;
using UnityEngine;
using UnityEditor;

using cPackage.Tools;

namespace PPM
{
    public class PackageUploadWindow : EditorWindow
    {
        Rect _background = new Rect(0, 0, 1027, 720);
        Color _backgroundColor = new Color(45 / 255f, 45 / 255f, 48 / 255f);

		private GUIStyle _textStyle;

		private string _selectedPackagePath;

		private string SelectedPackagePath
		{
			get { return _selectedPackagePath; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					_selectedPackagePath = value;
					LoadPPMConfiguration(value);
				}
			}
		}
		private bool _needShowSelectButton = false;
		private PPMConfiguration _configuration;

		private static string _title = "插件上传界面";

		public static void OpenPackageUploadWindow(string selectedPackagePath, bool needShowSelectButton)
        {
            PackageUploadWindow window = EditorWindow.GetWindow<PackageUploadWindow>(true);
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			window.titleContent = new GUIContent(_title);
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			window.title = _title;
#else
			PPMHelper.LogError("this feature is not implemented in current unity");
#endif
			window.minSize = new Vector2(450, 350);
			window.Init();
            window.Show();

			window.SelectedPackagePath = selectedPackagePath;
			window._needShowSelectButton = needShowSelectButton;
        }

		private void Init()
		{
			_textStyle = new GUIStyle(PPMHelper.IsProSkin() ? EditorStyles.label : EditorStyles.whiteLabel)
			{
				fontSize = 12
			};
		}

        void OnGUI()
        {
			GUILayout.Space(10);
			
			EditorGUI.DrawRect(_background, _backgroundColor);

			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("插件路径: {0}", _selectedPackagePath), _textStyle);
			string btnName = string.IsNullOrEmpty(_selectedPackagePath) ? "选择待上传ppm" : "重新选择";
			if (_needShowSelectButton && GUILayout.Button(new GUIContent(btnName), GUILayout.Width(100)))
			{
				string selectedPath = EditorUtility.OpenFilePanel("ppm to upload", "", "ppm");
				if (!string.IsNullOrEmpty(selectedPath))
				{
					SelectedPackagePath = selectedPath;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);


			GUILayout.Label(string.Format("插件名称: {0}", _configuration != null ? _configuration.PackageName : ""), _textStyle);
			GUILayout.Space(10);

			GUILayout.Label(string.Format("插件版本: {0}", _configuration != null ? _configuration.PackageVersion : ""), _textStyle);
			GUILayout.Space(10);

			GUILayout.Label(string.Format("标签信息: {0}", _configuration != null ? _configuration.PackageTags : ""), _textStyle);
			GUILayout.Space(10);

			GUILayout.TextArea(string.Format("插件描述: {0}", _configuration != null ? _configuration.PackageDescription : ""), _textStyle);
			GUILayout.Space(30);

			GUILayout.TextArea(string.Format("依赖信息: {0}", _configuration != null ? _configuration.PackageDependencies : ""), _textStyle);
			GUILayout.Space(30);

			GUILayout.TextArea(string.Format("版本日志: {0}", _configuration != null ? _configuration.PackageReleaseNote : ""), _textStyle);
			GUILayout.Space(60);

			if (GUILayout.Button("上传", GUILayout.Height(30)))
			{
				if (_configuration == null)
				{
					ShowNotification(new GUIContent("请选择待上传ppm插件"));
					return;
				}
				if (EditorUtility.DisplayDialog("插件上传", string.Format("确认上传插件:{0} ?", _configuration.PackageName), "ok", "cancel"))
				{
					PackageManager.Instance.PackageUpload(
						_configuration.PackageName, "Unity", _configuration.PackageVersion, 
						_configuration.PackageTags, _configuration.PackageDescription, 
						_configuration.PackageDependencies, _selectedPackagePath,
						_configuration.PackageReleaseNote);
				}
			}
		}

        private void OnPackageUploadWindowRefreshed()
        {
            ShowNotification(new GUIContent("上传成功"));
            System.Threading.Thread.Sleep(500);
            Close();
        }

		private void LoadPPMConfiguration(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return;

			if (Directory.Exists(cPackageHelper.TempCPackageFolder))
				Directory.Delete(cPackageHelper.TempCPackageFolder, true);

			cPackageHelper.Untar(filePath, cPackageHelper.TempCPackageFolder);
			string configFile = cPackageHelper.TempCPackageFolder + "ppm_package/ppm_package.json";
			_configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<PPMConfiguration>(File.ReadAllText(configFile));
		}

		protected void OnEnable()
        {
            PackageManager.Instance.OnPackageUploadWindowRefreshed += OnPackageUploadWindowRefreshed;
        }

        protected void OnDisable()
        {
            PackageManager.Instance.OnPackageUploadWindowRefreshed -= OnPackageUploadWindowRefreshed;
        }
    }
}
