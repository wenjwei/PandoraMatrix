using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace com.tencent.pandora.tools
{
    public class FontReplacerWindow : EditorWindow
    {
        Font targetFont;

        string targetFolder = "Assets/Actions/Resources/Social/Prefabs";

        static int _buttonWidth = 200;
        static int _fontFieldWidth = 200;

        [MenuItem("PandoraTools/FontReplacer")]
        public static void Init()
        {
            FontReplacerWindow window = (FontReplacerWindow)EditorWindow.GetWindow(typeof(FontReplacerWindow), true, "FontReplacer");
            window.Show(true);
        }

        void OnGUI()
        {
            DrawSelectFolderField();
            DrawSelectFontField();
        }

        private void DrawSelectFolderField()
        {
            EditorGUILayout.LabelField("(出于种种考虑，此工具并不会备份你的prefab文件，请正确使用版本控制工具！)");
            GUILayout.Space(10);
            EditorGUILayout.LabelField("将目标文件夹下所有prefab上的字体替换成目标字体。");
            if (GUILayout.Button("选择文件夹", GUILayout.Width(_buttonWidth)))
            {
                SelectTargetFolder();
            }
            EditorGUILayout.LabelField("目标文件夹：" + targetFolder, GUILayout.MaxWidth(500));
        }

        private void SelectTargetFolder()
        {
            string selectedFolder = EditorUtility.OpenFolderPanel("请选择目标文件夹", Application.dataPath + "/Actions/Resources", "");
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                selectedFolder = selectedFolder.Substring(selectedFolder.IndexOf("Assets"));
                targetFolder = selectedFolder;
            }
        }

        private void DrawSelectFontField()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("目标字体为：");
            targetFont = (Font)EditorGUILayout.ObjectField(targetFont, typeof(Font), true, GUILayout.Width(_fontFieldWidth));

            if (GUILayout.Button("替换字体", GUILayout.Width(_buttonWidth)))
            {
                if (IsFolderAndFontValid() == true)
                {
                    OnClickReplaceFont();
                }
            }
        }

        private void OnClickReplaceFont()
        {
            string[] prefabGuidArray = AssetDatabase.FindAssets("t:Prefab", new string[] { targetFolder });
            int fontReplaceNum = 0;
            for (int i = 0; i < prefabGuidArray.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuidArray[i]);
                EditorUtility.DisplayProgressBar("Hold on", assetPath, (float)(i + 1) / prefabGuidArray.Length);
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                fontReplaceNum += FontReplacer.ReplaceFont(ref gameObject, targetFont);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            Debug.Log(string.Format("替换完成，共替换{0}处。", fontReplaceNum));
            EditorUtility.DisplayDialog("替换完成", string.Format("替换完成，共替换{0}处。", fontReplaceNum), "知道了");
        }

        private bool IsFolderAndFontValid()
        {
            if (IsTargetFolderExists() == false)
            {
                return false;
            }

            if (HasSelectedFont() == false)
            {
                return false;
            }

            return true;
        }

        private bool IsTargetFolderExists()
        {
            if (Directory.Exists(targetFolder) == false)
            {
                Debug.Log(string.Format("所选文件夹{0}不存在！", targetFolder));
                EditorUtility.DisplayDialog("错误", "所选文件夹不存在！", "知道了");
                return false;
            }
            return true;
        }

        private bool HasSelectedFont()
        {
            if (targetFont == null)
            {
                Debug.Log("未选择字体！");
                EditorUtility.DisplayDialog("错误", "请选择字体！", "知道了");
                return false;
            }
            return true;
        }
    }
}