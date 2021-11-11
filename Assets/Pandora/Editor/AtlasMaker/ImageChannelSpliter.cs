using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace com.tencent.pandora.tools
{
    public class ImageChannelSpliter
    {
        public static void Execute(string atlasPath, string rgbAtlasPath, string alphaAtlasPath)
        {
            Texture2D rawTex = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(Texture2D)) as Texture2D;
            Color32[] rawColors = rawTex.GetPixels32();

            Color32[] rgbColors = new Color32[rawColors.Length];
            Color32[] alphaColors = new Color32[rawColors.Length];
            for(int i = 0; i < rawColors.Length; i++)
            {
                Color32 rgb = rawColors[i];
                rgb.a = 255;
                rgbColors[i] = rgb;
                Color32 alpha = rawColors[i];
                alpha.r = alpha.a;
                alpha.g = alpha.a;
                alpha.b = alpha.a;
                alpha.a = 255;
                alphaColors[i] = alpha;
            }
            Texture2D rgbTex = new Texture2D(rawTex.width, rawTex.height);
            rgbTex.SetPixels32(rgbColors);
            rgbTex.Apply();
            AtlasWriter.Write(rgbTex, rgbAtlasPath);

            Texture2D alphaTex = new Texture2D(rawTex.width, rawTex.height);
            alphaTex.SetPixels32(alphaColors);
            alphaTex.Apply();
            AtlasWriter.Write(alphaTex, alphaAtlasPath);
        }
    }
}

