using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;

namespace com.tencent.pandora.tools
{
    public class BuilderWindow : EditorWindow
    {
        private static GUIStyle FONT_BOLD_STYLE;
        private static Color COLOR_GREEN = new Color(26.0f / 255.0f, 171.0f / 255.0f, 37.0f / 255.0f);

        private static Vector2 scroll = Vector2.zero;

        private void OnEnable()
        {
            FONT_BOLD_STYLE = new GUIStyle() { fontStyle = FontStyle.Bold };
        }

        [MenuItem("PandoraTools/BuildAssets &#b")]
        public static void Init()
        {
            ActivityManager.Refresh();
            BuilderWindow window = EditorWindow.GetWindow<BuilderWindow>("Builder");
            window.minSize = new Vector2(550, 400);
            window.Show();
        }

        [DidReloadScripts]
        private static void OnScriptReload()
        {
            ActivityManager.Refresh();
        }

        void OnGUI()
        {
            try
            {
                GUILayout.Space(5);
                GUILayout.Label("Please select activities:", FONT_BOLD_STYLE);
                ShowActivityList();
                ShowBuildItem("Build Current & Play", GetBuildTarget(), true);

                ShowBuildItem("Build Android", BuildTarget.Android);
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                ShowBuildItem("Build iOS", BuildTarget.iOS);
#else
                ShowBuildItem("Build iPhone", BuildTarget.iPhone);
#endif
                if(Builder.IS_SUPPORT_PC == true)
                {
                    ShowBuildItem("Build PC", BuildTarget.StandaloneWindows);
                }
                Event e = Event.current;
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B)
                {
                    Build(GetBuildTarget(), true, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static BuildTarget GetBuildTarget()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    if (Builder.IS_SUPPORT_PC == true)
                    {
                        return BuildTarget.StandaloneWindows;
                    }
                    return BuildTarget.Android;
                case BuildTarget.Android:
                    return BuildTarget.Android;

#if UNITY_4_6 || UNITY_4_7
                case BuildTarget.iPhone:
                case BuildTarget.StandaloneOSXUniversal:
                    return BuildTarget.iPhone;
#elif UNITY_5 || UNITY_2017_1 || UNITY_2017_2
                case BuildTarget.iOS:
                case BuildTarget.StandaloneOSXUniversal:
                    return BuildTarget.iOS;
#elif UNITY_2017_3_OR_NEWER
                case BuildTarget.iOS:
                case BuildTarget.StandaloneOSX:
                    return BuildTarget.iOS;
#endif
            }
            return BuildTarget.Android;
        }

        private static void ShowActivityList()
        {
            scroll = GUILayout.BeginScrollView(scroll);
            List<string> nameList = ActivityManager.GetActivityNameList();
            for (int i = 0; i < nameList.Count; i++)
            {
                string name = nameList[i];
                GUILayout.BeginHorizontal();
                bool activity = EditorGUILayout.Toggle(name, ActivityManager.IsActivitySelected(name));
                ActivityManager.ToggleActivity(name, activity);
                bool buildPrefab = EditorGUILayout.Toggle("Build Prefab", ActivityManager.IsActivityBuildPrefab(name));
                ActivityManager.ToggleActivityBuildPrefab(name, buildPrefab);
                if(SvnHelper.IS_USING_SVN == true)
                {
                    bool commitSvn = EditorGUILayout.Toggle("Commit SVN", ActivityManager.IsActivityCommitSvn(name));
                    ActivityManager.ToggleActivityCommitSvn(name, commitSvn);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.Space(5);
        }

        private static void ShowBuildItem(string label, BuildTarget target, bool playAfterBuild = false)
        {
            Color color = GUI.backgroundColor;
            GUI.backgroundColor = COLOR_GREEN;
            if (GUILayout.Button(label, GUILayout.Height(30)))
            {
                Build(target, playAfterBuild, true);
            }
            GUI.backgroundColor = color;
        }

        public static void Build(BuildTarget target, bool playAfterBuild = false, bool showReport = true)
        {
            List<string> selectedList = ActivityManager.GetSelectedActivityNameList();
            Builder.Build(selectedList, target, showReport);
            if (playAfterBuild == true)
            {
                EditorApplication.isPlaying = true;
            }
        }

    }
}
