using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.tencent.pandora.tools
{
    /// <summary>
    /// 将选项序列化为文本
    /// </summary>
    public class BundleOptionsHelper
    {
        public static void RecordSelectedActivityList(List<string> selectedList)
        {
            if (selectedList == null || selectedList.Count == 0)
            {
                return;
            }
            CreateOptionsFolder();
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < selectedList.Count; i++)
            {
                sb.Append(selectedList[i]);
                sb.Append(";");
            }
            sb.Remove(sb.Length - 1, 1);
            File.WriteAllText(BuilderSetting.OPTIONS_PATH, sb.ToString(), Encoding.UTF8);
            AssetDatabase.ImportAsset(BuilderSetting.OPTIONS_PATH);
            AssetDatabase.ImportAsset(BuilderSetting.OPTIONS_PATH);
        }

        private static void CreateOptionsFolder()
        {
            if (Directory.Exists(BuilderSetting.OPTIONS_FOLDER) == false)
            {
                Directory.CreateDirectory(BuilderSetting.OPTIONS_FOLDER);
            }
        }

        public static HashSet<string> ReadSelectedActivitySet()
        {
            HashSet<string> result = new HashSet<string>();
            if(File.Exists(BuilderSetting.OPTIONS_PATH) == true)
            {
                string content = File.ReadAllText(BuilderSetting.OPTIONS_PATH);
                string[] activities = content.Split(';');
                foreach(string s in activities)
                {
                    result.Add(s);
                }
            }
            return result;
        }

    }
}

