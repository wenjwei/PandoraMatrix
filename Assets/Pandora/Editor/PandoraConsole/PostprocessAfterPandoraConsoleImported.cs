using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 新版本的Console工具集导入后，删除老的LogConsole.cs文件
/// </summary>
[InitializeOnLoad]
public class PostprocessAfterPandoraConsoleImported
{
    static PostprocessAfterPandoraConsoleImported()
    {
        string[] fileNames = new string[]
       {
            "LogConsole",
            "LogUploader",
       };

        for (int i = 0; i < fileNames.Length; i++)
        {
            string name = fileNames[i];
            string[] guids = AssetDatabase.FindAssets(name);
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                //这里不能调用AssetDataBase的DeleteAsset和Refresh接口，否则会有报错，只能使用File的接口
                string absolutePath = Path.Combine(Application.dataPath, path.Replace("Assets/", ""));
                File.Delete(absolutePath);
            }
        }
    }
}
