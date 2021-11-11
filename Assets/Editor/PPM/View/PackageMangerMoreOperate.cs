using UnityEditor;
using UnityEngine;

namespace PPM
{
    class PackageMangerMoreOperate : EditorWindow
    {
		Rect _background = new Rect(0, 0, 1024, 720);
		Color _backgroundColor = new Color(45 / 255f, 45 / 255f, 48 / 255f);

		public static void OpenPackageMangerMoreOperateWindow()
        {
            PackageMangerMoreOperate window = EditorWindow.GetWindow<PackageMangerMoreOperate>(true);
            window.titleContent = new GUIContent("更多操作界面");
#if !UNITY_5_5_OR_NEWER
			window.maxSize = new Vector2(410, 60);
            window.minSize = new Vector2(410, 60);
#else
			window.maxSize = new Vector2(310, 60);
            window.minSize = new Vector2(310, 60);
#endif
			window.ShowPopup();
        }

        void OnGUI()
        {
			EditorGUI.DrawRect(_background, _backgroundColor);
			GUILayout.Space(15);

            GUILayout.BeginHorizontal();
			GUILayout.Space(15);
            if (GUILayout.Button("插件制作", GUILayout.Height(30), GUILayout.Width(80)))
            {
                PackagePackWindow.OpenWindow();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("插件上传", GUILayout.Height(30), GUILayout.Width(80)))
            {
                PackageUploadWindow.OpenPackageUploadWindow("", true);
            }
            GUILayout.Space(10);
            if (GUILayout.Button("插件文档导出", GUILayout.Height(30), GUILayout.Width(100)))
            {
                PPMUtils.ExportDocOfPPMPackages.ExportDoc();
            }

#if !UNITY_5_5_OR_NEWER
			GUILayout.Space(10);
			if (GUILayout.Button("本地安装", GUILayout.Height(30), GUILayout.Width(80)))
			{
				string selectedPath = EditorUtility.OpenFilePanel("file to install", "", "*.ppm");
				if (!string.IsNullOrEmpty(selectedPath))
				{
					PackageManager.Instance.ImportPackage(selectedPath);
				}
			}
#endif
            GUILayout.Space(15);
			GUILayout.EndHorizontal();

        }
    }
}

