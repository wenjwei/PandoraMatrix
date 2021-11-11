using UnityEditor;
using System.Collections.Generic;

namespace com.tencent.pandora.tools
{
    internal class ActivityManager
    {
        //Key为Activity名称，Value为Activity是否选中
        private static Dictionary<string, bool> _activityDict = new Dictionary<string, bool>();
        //是否打包Prefab设置，Key为Activity名称，Value是否选中
        private static Dictionary<string, bool> _buildPrefabDict = new Dictionary<string, bool>();
        //是否自动提交SVN，key为Activity名称，Value是否选中
        private static Dictionary<string, bool> _commitSvnDict = new Dictionary<string, bool>();
        private static List<string> _activityNameList = new List<string>();
        private static List<string> _selectedActivityNameList = new List<string>();
        

        public static void Refresh()
        {
            RefreshActivityDict();
            RefreshActivitySelection();
        }

        private static void RefreshActivityDict()
        {
            _activityDict.Clear();
            string[] guids = AssetDatabase.FindAssets("t:DefaultAsset", new string[] { BuilderSetting.ROOT_PATH });
            string startToken = BuilderSetting.ROOT_PATH + "/";
            foreach (string s in guids)
            {
                string dirName = AssetDatabase.GUIDToAssetPath(s).Replace(startToken, string.Empty);
                if (dirName.Contains("/") == false)
                {
                    _activityDict.Add(dirName, false);
                }
            }
        }

        private static void RefreshActivitySelection()
        {
            HashSet<string> selectedSet = BundleOptionsHelper.ReadSelectedActivitySet();
            foreach(string name in selectedSet)
            {
                if(_activityDict.ContainsKey(name) == true)
                {
                    _activityDict[name] = true;
                }
            }
        }

        public static List<string> GetActivityNameList()
        {
            _activityNameList.Clear();
            foreach(string name in _activityDict.Keys)
            {
                _activityNameList.Add(name);
            }
            return _activityNameList;
        }

        public static bool IsActivitySelected(string name)
        {
            return _activityDict[name];
        }

        public static bool IsActivityBuildPrefab(string name)
        {
            if(_buildPrefabDict.ContainsKey(name) == true)
            {
                return _buildPrefabDict[name];
            }
            return false;
        }

        public static bool IsActivityCommitSvn(string name)
        {
            if(_commitSvnDict.ContainsKey(name) == true)
            {
                return _commitSvnDict[name];
            }
            return false;
        }

        public static void ToggleActivity(string name, bool value)
        {
            _activityDict[name] = value;
        }

        public static void ToggleActivityBuildPrefab(string name, bool value)
        {
            _buildPrefabDict[name] = value;
        }

        public static void ToggleActivityCommitSvn(string name, bool value)
        {
            _commitSvnDict[name] = value;
        }

        public static List<string> GetSelectedActivityNameList()
        {
            _selectedActivityNameList.Clear();
            foreach(string name in _activityDict.Keys)
            {
                if(_activityDict[name] == true)
                {
                    _selectedActivityNameList.Add(name);
                }
            }
            BundleOptionsHelper.RecordSelectedActivityList(_selectedActivityNameList);
            return _selectedActivityNameList;
        }

    }

}
