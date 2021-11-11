using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace com.tencent.pandora.tools {
    public class ViewSDKVersion {

        [MenuItem("PandoraTools/查看SDKVersion")]
        public static void ViewVersion()
        {
            EditorUtility.DisplayDialog("SDK Version", string.Format("当前SDK版本号：{0}", Pandora.Instance.CombinedSDKVersion()), "Close");
        }
    }
}