using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.tencent.pandora.tools
{
    public class AtlasWriter
    {
        public static void Write(Texture2D atlas, string path)
        {
            byte[] pngData = atlas.EncodeToPNG();
            string pngPath = Application.dataPath + path.Replace("Assets", "");
            File.WriteAllBytes(pngPath, pngData);
        }
    }
}
