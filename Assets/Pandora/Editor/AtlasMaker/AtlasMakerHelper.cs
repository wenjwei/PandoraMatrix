using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace com.tencent.pandora.tools
{
    public class AtlasMakerHelper
    {
        public const int MAX_ATLAS_SIZE = 2048;
        public const int FAVOR_ATLAS_SIZE = 1024;
        public const int QUALITY_PARAMS = 90;

        private static Regex SCALE_9_EXP = new Regex(@"(?<=@)l\d+_r\d+_t\d+_b\d+");
        private static Regex SCALE_9_LEFT = new Regex(@"(?<=l)\d+(?=_)");
        private static Regex SCALE_9_RIGHT = new Regex(@"(?<=r)\d+(?=_)");
        private static Regex SCALE_9_TOP = new Regex(@"(?<=t)\d+(?=_)");
        private static Regex SCALE_9_BOTTOM = new Regex(@"(?<=b)\d+");
        private const string BIG_TEXTURE_SLICE = "_@slice_";

        public static void SetTextureCompressFormat(TextureImporter importer, TextureImporterFormat androidFormat, TextureImporterFormat iPhoneFormat)
        {
#if UNITY_4_6 || UNITY_4_7
            importer.textureFormat = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings("iPhone", MAX_ATLAS_SIZE, iPhoneFormat, QUALITY_PARAMS);
            importer.SetPlatformTextureSettings("Android", MAX_ATLAS_SIZE, androidFormat, QUALITY_PARAMS);
#elif UNITY_5
            importer.textureFormat = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings("iPhone", MAX_ATLAS_SIZE, iPhoneFormat, QUALITY_PARAMS, false);
            importer.SetPlatformTextureSettings("Android", MAX_ATLAS_SIZE, androidFormat, QUALITY_PARAMS, false);
#elif UNITY_2017_1_OR_NEWER
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            TextureImporterPlatformSettings platFormatAndroid = importer.GetPlatformTextureSettings("Android");
            platFormatAndroid.name = "Android";
            platFormatAndroid.format = androidFormat;
            platFormatAndroid.overridden = true;

            TextureImporterPlatformSettings platFormatIos = importer.GetPlatformTextureSettings("iPhone");
            platFormatIos.name = "iPhone";
            platFormatIos.format = iPhoneFormat;
            platFormatIos.overridden = true;

            importer.allowAlphaSplitting = false;
            importer.maxTextureSize = MAX_ATLAS_SIZE;
            importer.compressionQuality = QUALITY_PARAMS;

            importer.SetPlatformTextureSettings(platFormatAndroid);
            importer.SetPlatformTextureSettings(platFormatIos);
#endif
        }

        public static void ImportRawAtlasTexture(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.maxTextureSize = MAX_ATLAS_SIZE;
            importer.spriteImportMode = SpriteImportMode.None;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            AtlasMakerHelper.SetTextureCompressFormat(importer, TextureImporterFormat.RGBA32, TextureImporterFormat.RGBA32);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void LogAtlasSize(Texture2D atlas)
        {
            if (atlas.width > FAVOR_ATLAS_SIZE || atlas.height > FAVOR_ATLAS_SIZE)
            {
                string content = atlas.name + " 图集宽高尺寸超过1024像素,为 " + atlas.width + " 像素";
                EditorUtility.DisplayDialog("图集尺寸超标", content, "改改改~");
                Debug.LogError(content);
            }
            else
            {
                Debug.Log(string.Format("<color=#D00FDBFF>图集 {0} 尺寸为： {1}x{2}</color>", atlas.name, atlas.width, atlas.height));
            }
        }

        public static Vector4[] GetPackTextureBorders(List<string> pathList)
        {
            Vector4[] result = new Vector4[pathList.Count];
            for (int i = 0; i < pathList.Count; i++)
            {
                string name = Path.GetFileNameWithoutExtension(pathList[i]);
                if (SCALE_9_EXP.IsMatch(name) == true)
                {
                    string nameMatchResult = SCALE_9_EXP.Match(name).Value;
                    string left = SCALE_9_LEFT.Match(nameMatchResult).Value;
                    string right = SCALE_9_RIGHT.Match(nameMatchResult).Value;
                    string top = SCALE_9_TOP.Match(nameMatchResult).Value;
                    string bottom = SCALE_9_BOTTOM.Match(nameMatchResult).Value;
                    result[i] = new Vector4(int.Parse(left), int.Parse(bottom), int.Parse(right), int.Parse(top));
                }
                else
                {
                    result[i] = Vector4.zero;
                }
            }
            return result;
        }

        public static Texture2D[] GetPackTextures(List<string> pathList)
        {
            Texture2D[] result = new Texture2D[pathList.Count];
            for (int i = 0; i < result.Length; i++)
            {
                string path = pathList[i];
                Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                if (path.Contains(BIG_TEXTURE_SLICE) == false) //大图切片已经增加过2像素
                {
                    texture = TextureClamper.Clamp(texture);
                }
                result[i] = texture;
            }
            return result;
        }

        public static void CreateMaterial(string shaderPath, string texturePath)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath(shaderPath, typeof(Shader)) as Shader;
            Material material = new Material(shader);
            Texture2D texture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
            material.SetTexture("_MainTex", texture);
            string materialPath = texturePath.Replace(".png", "_mat.mat");
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
        }

        public static string[] GetPackTextureNames(List<string> pathList)
        {
            string[] result = new string[pathList.Count];
            for (int i = 0; i < pathList.Count; i++)
            {
                result[i] = Path.GetFileNameWithoutExtension(pathList[i]);
            }
            return result;
        }

    }
}

