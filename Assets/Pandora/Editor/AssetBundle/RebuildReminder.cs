using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace com.tencent.pandora.tools
{
    /// <summary>
    /// 启动Unity时检查Assets/Actions/Resources目录下的Prefab文件
    /// 所依赖的C#文件是否更新，如果更新的话弹资源需要重打包的提示
    /// </summary>
    [InitializeOnLoad] //启动Unity时运行
    public class RebuildReminder
    {
        private static Regex SCRIPT_GUID_PATTERN = new Regex("  m_Script: {.+guid: (.+), .+}");
        private static string SCRIPT_PREFIX = "  m_Script";
        private static string[] PREFAB_DIR = new string[] { "Assets/Actions/Resources" };

        static RebuildReminder()
        {
            Main();
        }

        //[MenuItem("Assets/RebuildReminder")]
        public static void Main()
        {
            if(SvnHelper.IS_USING_SVN == false)
            {
                return;
            }
            if(SvnHelper.IsSvnAvaliable() == false)
            {
                EditorUtility.DisplayDialog("提示", "请检查SVN是否正确安装", "好的");
                return;
            }
            ForceTextSerializationMode();
            Dictionary<string, HashSet<string>> scriptPrefabDict = ConstructScriptPrefabDict();
            Dictionary<string, HashSet<string>> rebuildScriptPrefabSet = GetRebuildScriptPrefabSet(scriptPrefabDict);
            if (rebuildScriptPrefabSet.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("以下C#文件需要更新并重新打包依赖它的Prefab文件：\n");
                foreach (var kvp in rebuildScriptPrefabSet)
                {
                    sb.Append(kvp.Key);sb.Append(" :\n ");
                    foreach (string s in kvp.Value)
                    {
                        sb.Append("-----"); sb.Append(s); sb.Append(";"); sb.Append("\n");
                    }
                    sb.Append("\n");
                }
                EditorUtility.DisplayDialog("Prefab重打包提示", sb.ToString(), "马上就去~~");
            }
        }

        private static Dictionary<string, HashSet<string>> GetRebuildScriptPrefabSet(Dictionary<string, HashSet<string>> scriptPrefabDict)
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            foreach (var kvp in scriptPrefabDict)
            {
                string localPath = ConvertToFilePath(kvp.Key);
                string localRevision = SvnHelper.GetLocalFileRevision(localPath);
                if(string.IsNullOrEmpty(localRevision) == true)
                {
                    continue;
                }
                string remoteRevision = SvnHelper.GetRemoteFileRevision(localPath);
                if(string.IsNullOrEmpty(remoteRevision) == true)
                {
                    EditorUtility.DisplayDialog("提示", "请检查SVN命令行模式下用户名和密码", "好的");
                    return new Dictionary<string, HashSet<string>>();
                }
                if(remoteRevision != localRevision)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        private static List<string> FilterPrefabAssetPathList()
        {
            string[] prefabPathArr = AssetDatabase.FindAssets("t:Prefab", PREFAB_DIR);
            List<string> result = new List<string>(prefabPathArr.Length);
            foreach(string guid in prefabPathArr)
            {
                result.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            return result;
        }

        //key为ScriptPath,Value为PrefabPath的HashSet
        private static Dictionary<string, HashSet<string>> ConstructScriptPrefabDict()
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            List <string> prefabAssetPathList = FilterPrefabAssetPathList();
            foreach (string prefabPath in prefabAssetPathList)
            {
                HashSet<string> scriptPathSet = GetPrefabReferredScriptPathSet(prefabPath);
                foreach(string scriptPath in scriptPathSet)
                {
                    if(result.ContainsKey(scriptPath) == false)
                    {
                        result.Add(scriptPath, new HashSet<string>());
                    }
                    HashSet<string> prefabPathSet = result[scriptPath];
                    prefabPathSet.Add(prefabPath);
                }
            }
            return result;
        }

        private static HashSet<string> GetPrefabReferredScriptPathSet(string prefabPath)
        {
            HashSet<string> set = new HashSet<string>();
            string[] lineArr = GetFileLines(prefabPath);
            foreach(string line in lineArr)
            {
                if(string.IsNullOrEmpty(line) == false && line.StartsWith(SCRIPT_PREFIX))
                {
                    Match m = SCRIPT_GUID_PATTERN.Match(line);
                    if(m != null && m.Groups != null && m.Groups.Count == 2)
                    {
                        string guid = m.Groups[1].Value;
                        string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                        if(string.IsNullOrEmpty(scriptPath) == false)
                        {
                            set.Add(scriptPath);
                        }
                    }
                }
            }
            return set;
        }

        private static string[] GetFileLines(string assetPath)
        {
            string path = ConvertToFilePath(assetPath);
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }
            return new string[] { };
        }

        //将Unity资源路径转换为系统目录
        private static string ConvertToFilePath(string assetPath)
        {
            return Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
        }

        private static void ForceTextSerializationMode()
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorSettings.serializationMode = SerializationMode.ForceText;
            }
        }
    }
}
