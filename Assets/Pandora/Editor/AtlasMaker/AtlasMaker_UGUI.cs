////#define USING_NGUI
#define USING_UGUI
using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;
using System.IO;

namespace com.tencent.pandora.tools
{
    public class AtlasMaker_UGUI
    {
#if USING_UGUI
        private const string SHADER_NAME = "Assets/Pandora/Resources/Shaders/ugui/Pandora-Default.shader";
        
        private static Texture2D _atlas;

        private const string modulePath = "modulePath";

        [MenuItem("PandoraTools/MakeAtlas_UGUI")]
        [MenuItem("Assets/MakeAtlas_UGUI")]
        public static void MakeAtlas()
        {
            //选择贴图列表
            List<string> texturePathList = SelectTexturePathList();
            MakeAtlas(texturePathList, true);
        }

        /// <summary>
        /// showDisplayDialog 是否显示提示弹窗，批量处理工具一般不显示。
        /// </summary>
        /// <param name="texturePathList"></param>
        /// <param name="showDisplayDialog"></param>
        /// <returns></returns>
        public static string MakeAtlas(List<string> texturePathList, bool showDisplayDialog)
        {
            string atlasPath = null;
            if (texturePathList.Count > 0)
            {
                if (showDisplayDialog)
                {
                    atlasPath = GetAtlasPath(texturePathList[0]);
                }
                else
                {
                    atlasPath = GetAtlasPathAuto(texturePathList[0]);
                }
            }
            if (string.IsNullOrEmpty(atlasPath) == true )
            {
                EditorUtility.DisplayDialog("消息", "未选择任何路径", "OK");
                return string.Empty;
            }
            //兼容旧版机制，1.检查旧版通道分离的图集是否存在，2.检查新版图集是否存在
            string existAtlasPath = atlasPath.Replace("_Atlas.png", "_Atlas_rgb.png");
            Sprite existAtlas = AssetDatabase.LoadAssetAtPath(existAtlasPath, typeof(Sprite)) as Sprite;
            if(existAtlas == null)
            {
                existAtlasPath = atlasPath;
                existAtlas = AssetDatabase.LoadAssetAtPath(existAtlasPath, typeof(Sprite)) as Sprite;
            }
            atlasPath = atlasPath.Replace("_Atlas_rgb.png", "_Atlas.png");
            if (existAtlas != null)
            {
                //更新Atlas：以目录下最新图片列表为准，增加，删除、更新Sprite，已有的Sprite GUID不改变
                RefreshAtlas(atlasPath, existAtlasPath, texturePathList, showDisplayDialog);
                RemoveRedundantAsset(existAtlasPath);
            }
            else
            {
                //创建新的Atlas
                CreateAtlas(atlasPath, texturePathList, showDisplayDialog);
            }
            return atlasPath;
        }

        private static List<string> SelectTexturePathList()
        {
            List<string> result = new List<string>();
            Object[] objs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
            if (objs.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "选择的图片长度为0", "ok");
                return result;
            }
            foreach (Object o in objs)
            {
                result.Add(AssetDatabase.GetAssetPath(o));
            }
            return result;
        }

        /// <summary>
        /// 返回Assets\开头的资源路径
        /// </summary>
        /// <returns></returns>
        private static string GetAtlasPath(string texturePath)
        {
            string textureFolderPath = Path.GetDirectoryName(texturePath);
            string actionFolderPath = Path.GetDirectoryName(textureFolderPath);
            string actionName = Path.GetFileNameWithoutExtension(actionFolderPath);
            string atlasName = actionName + "_Atlas.png";
            string atlasPath = Path.Combine(Path.Combine(actionFolderPath, "Atlas"), atlasName);
            string path = EditorUtility.SaveFilePanel("Save Atlas", Application.dataPath + atlasPath.Replace("Assets", ""), atlasName, "png");
            return path.Replace(Application.dataPath, "Assets");
        }

        /// <summary>
        /// 返回Assets\开头的资源路径,命名取图片文件夹名
        /// </summary>
        /// <returns></returns>
        private static string GetAtlasPathAuto(string texturePath)
        {
            string textureFolderPath = Path.GetDirectoryName(texturePath);
            string actionFolderPath = Path.GetDirectoryName(textureFolderPath);
            string modlePath = Path.GetDirectoryName(actionFolderPath);
            string actionName = Path.GetFileNameWithoutExtension(textureFolderPath);
            string atlasName = actionName + "_Atlas.png";
            string atlasPath = Path.Combine(Path.Combine(modlePath, "Atlas"), atlasName);
           return atlasPath;
        }

        private static void RefreshAtlas(string atlasPath, string existAtlasPath, List<string> texturePathList, bool showDisplayDialog)
        {
            Dictionary<string, SpriteMetaData> spriteMetaDataDict = GetSpriteMetaDataDict(existAtlasPath);
            _atlas = new Texture2D(AtlasMakerHelper.MAX_ATLAS_SIZE, AtlasMakerHelper.MAX_ATLAS_SIZE);
            _atlas.name = Path.GetFileNameWithoutExtension(atlasPath);
            Rect[] rects = _atlas.PackTextures(AtlasMakerHelper.GetPackTextures(texturePathList), 0, AtlasMakerHelper.MAX_ATLAS_SIZE, false);
            _atlas = AtlasOptimizer.Optimize(_atlas, rects, true);
            AtlasWriter.Write(_atlas, atlasPath);
            AtlasMakerHelper.ImportRawAtlasTexture(atlasPath);
            CreateMultipleSpriteImporter(atlasPath, rects, AtlasMakerHelper.GetPackTextureNames(texturePathList), AtlasMakerHelper.GetPackTextureBorders(texturePathList), _atlas.width, _atlas.height, spriteMetaDataDict);
            AtlasMakerHelper.CreateMaterial(SHADER_NAME, atlasPath);
            AtlasMakerHelper.LogAtlasSize(_atlas);
            if (showDisplayDialog)
            {
                EditorUtility.DisplayDialog("提示", string.Format("图集更新成功, {0} 宽：{1} 高：{2}", _atlas.name, _atlas.width, _atlas.height), "知道了~");
            }
            else
            {
                Debug.Log(string.Format("图集更新成功, {0} 宽：{1} 高：{2}", _atlas.name, _atlas.width, _atlas.height));
            }
        }

        private static void RemoveRedundantAsset(string existAtlasPath)
        {
            if(existAtlasPath.Contains("_rgb.png") == true)
            {
                AssetDatabase.DeleteAsset(existAtlasPath);
                AssetDatabase.DeleteAsset(existAtlasPath.Replace("_rgb.png", "_alpha.png"));
                AssetDatabase.DeleteAsset(existAtlasPath.Replace("_rgb.png", "_rgb_mat.mat"));
            }
        }

        private static Dictionary<string, SpriteMetaData> GetSpriteMetaDataDict(string atlasPath)
        {
            Dictionary<string, SpriteMetaData> result = new Dictionary<string, SpriteMetaData>();
            TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                throw new Exception("所选图集必须是MultipleSprite， " + atlasPath);
            }
            foreach (SpriteMetaData data in importer.spritesheet)
            {
                result.Add(data.name, data);
            }
            return result;
        }

        private static void CreateAtlas(string atlasPath, List<string> texturePathList, bool showDisplayDialog)
        {
            _atlas = new Texture2D(AtlasMakerHelper.MAX_ATLAS_SIZE, AtlasMakerHelper.MAX_ATLAS_SIZE);
            _atlas.name = Path.GetFileNameWithoutExtension(atlasPath);
            Rect[] rects = _atlas.PackTextures(AtlasMakerHelper.GetPackTextures(texturePathList), 0, AtlasMakerHelper.MAX_ATLAS_SIZE, false);
            _atlas = AtlasOptimizer.Optimize(_atlas, rects, true);
            AtlasWriter.Write(_atlas, atlasPath);
            AtlasMakerHelper.ImportRawAtlasTexture(atlasPath);
            CreateMultipleSpriteImporter(atlasPath, rects, AtlasMakerHelper.GetPackTextureNames(texturePathList), AtlasMakerHelper.GetPackTextureBorders(texturePathList), _atlas.width, _atlas.height, null);
            AtlasMakerHelper.CreateMaterial(SHADER_NAME, atlasPath);
            AtlasMakerHelper.LogAtlasSize(_atlas);
            if (showDisplayDialog)
            {
                EditorUtility.DisplayDialog("提示", string.Format("图集更新成功, {0} 宽：{1} 高：{2}", _atlas.name, _atlas.width, _atlas.height), "知道了~");
            }
            else
            {
                Debug.Log(string.Format("图集更新成功, {0} 宽：{1} 高：{2}", _atlas.name, _atlas.width, _atlas.height));
            }
        }

        private static void CreateMultipleSpriteImporter(string path, Rect[] rects, string[] spriteNames, Vector4[] borders, int width, int height, Dictionary<string, SpriteMetaData> spriteMetaDataDict)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            SpriteMetaData[] metaDatas = new SpriteMetaData[spriteNames.Length];
            for (int i = 0; i < metaDatas.Length; i++)
            {
                SpriteMetaData metaData = new SpriteMetaData();
                if (spriteMetaDataDict != null && spriteMetaDataDict.ContainsKey(spriteNames[i]) == true)
                {
                    metaData = spriteMetaDataDict[spriteNames[i]];
                }
                metaData.name = spriteNames[i];
                Rect rect = rects[i];
                metaData.rect = new Rect(rect.xMin * width + TextureClamper.BORDER, rect.yMin * height + TextureClamper.BORDER, rect.width * width - TextureClamper.BORDER * 2, rect.height * height - TextureClamper.BORDER * 2);
                if(metaData.border == Vector4.zero)
                {
                    if(borders != null)
                    {
                        metaData.border = borders[i];
                    }
                }
                metaDatas[i] = metaData;
            }
            importer.spritesheet = metaDatas;
            importer.maxTextureSize = AtlasMakerHelper.MAX_ATLAS_SIZE;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            AtlasMakerHelper.SetTextureCompressFormat(importer, TextureImporterFormat.ETC2_RGBA8, TextureImporterFormat.ASTC_RGBA_5x5);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }


        public class SingleSpriteData
        {
            public string name;
            public Vector4 border;
            public Texture2D texture;
        }
#endif

    }
}

