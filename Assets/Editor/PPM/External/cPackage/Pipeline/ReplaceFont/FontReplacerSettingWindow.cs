using System;
using UnityEngine;
using UnityEditor;
using cPackage.Tools;

namespace cPackage.Pipeline.Prefab
{
    public class FontReplacerSettingWindow : EditorWindow
    {
        static int _fontFieldWidth = 200;

        public static void OpenSettingWindow()
        {
            FontReplacerSettingWindow window = (FontReplacerSettingWindow)EditorWindow.GetWindow(typeof(FontReplacerSettingWindow), true, "SelectFont");
            InitFontSettings();
            window.ShowUtility();
        }

        private void OnGUI()
        {
            DrawSelectFontField();
        }

        private void OnDestroy()
        {
            if (ReplaceFont.targetFont != null)
            {
                string fontPath = AssetDatabase.GetAssetPath(ReplaceFont.targetFont);
                string fontGUID = AssetDatabase.AssetPathToGUID(fontPath);
                PlayerPrefs.SetString("cPackage.Pipeline.Prefab.ReplaceFont.fontGUID", fontGUID);
                PlayerPrefs.Save();
            }
        }

        private static void DrawSelectFontField()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Target font:");
            ReplaceFont.targetFont = (Font)EditorGUILayout.ObjectField(ReplaceFont.targetFont, typeof(Font), true, GUILayout.Width(_fontFieldWidth));
        }

        public static void InitFontSettings()
        {
            string fontGUID = PlayerPrefs.GetString("cPackage.Pipeline.Prefab.ReplaceFont.fontGUID");
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGUID);
            Font font = AssetDatabase.LoadAssetAtPath(fontPath, typeof(Font)) as Font;
            if (font != null)
            {
                ReplaceFont.targetFont = font;
            }
        }

        public static bool HasSelectedFont()
        {
            if (ReplaceFont.targetFont == null)
            {
                cPackageHelper.LogError("The \"Replace Prefab Font \" has been checked, but no font has been selected. Please manually replace font after the import is complete.");
                return false;
            }
            return true;
        }
    }
}
