using System;
using UnityEngine;
using UnityEditor;
using cPackage.Tools;

namespace cPackage.Package
{
    public partial class Configuration
    {
        [ConfigurationExtensionField]
        [ConfigurationField("Replace Prefab Font:", "replace fonts for prefabs.", "cPackage.Pipeline.Prefab.FontReplacerSettingWindow")]
        public bool ReplaceFont;
    }
}

namespace cPackage.Pipeline.Prefab
{
    [ImportPostPipelineFeature("\\.prefab$", "ReplaceFont")]
    public class ReplaceFont : BaseFeatureProcess
    {
        public static Font targetFont;

        public override void ProcessFeature(string filePath)
        {
            string gameObjectPath = filePath.Substring(filePath.IndexOf("Assets"));
            GameObject gameObject = AssetDatabase.LoadAssetAtPath(gameObjectPath, typeof(GameObject)) as GameObject;
            FontReplacerSettingWindow.InitFontSettings();

            if (FontReplacerSettingWindow.HasSelectedFont() == true)
            {
                FontReplacer.ReplaceFont(ref gameObject, targetFont);
                cPackageHelper.Log(string.Format("Replace {0} 's fonts done!", gameObjectPath));
            }
        }
    }
}
