//#define USING_NGUI
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
    public class AtlasMaker_NGUI
    {
#if USING_NGUI

        private const string SHADER_NAME = "Assets/Pandora/Resources/Shaders/ngui/Pandora - Transparent Colored.shader";

        private static Texture2D _atlas;

        [MenuItem("PandoraTools/MakeAtlas_NGUI")]
        [MenuItem("Assets/MakeAtlas_NGUI")]
        public static void MakeAtlas()
        {
            //选择贴图列表
            List<string> texturePathList = SelectTexturePathList();
            MakeAtlas(texturePathList);
        }

        public static string MakeAtlas(List<string> texturePathList)
        {
            string atlasPath = null;
            if (texturePathList.Count > 0)
            {
                atlasPath = GetAtlasPath(texturePathList[0]);
            }
            if (string.IsNullOrEmpty(atlasPath) == true)
            {
                EditorUtility.DisplayDialog("消息", "未选择任何路径", "OK");
                return string.Empty;
            }
            GameObject existAtlas = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(GameObject)) as GameObject;
            if (existAtlas != null)
            {
                //更新Atlas：以目录下最新图片列表为准，增加，删除、更新Sprite，已有的Sprite GUID不改变
                RefreshAtlas(atlasPath, texturePathList);
                RemoveRedundantAsset(atlasPath);
            }
            else
            {
                //创建新的Atlas
                CreateAtlas(atlasPath, texturePathList);
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
            string atlasName = actionName + "_Atlas.prefab";
            string atlasPath = Path.Combine(Path.Combine(actionFolderPath, "Atlas"), atlasName);
            string path = EditorUtility.SaveFilePanel("Save Atlas", Application.dataPath + atlasPath.Replace("Assets", ""), atlasName, "prefab");
            return path.Replace(Application.dataPath, "Assets");
        }

        private static void RefreshAtlas(string atlasPath, List<string> texturePathList)
        {
            Dictionary<string, UISpriteData> spriteDataDict = GetSpriteDataDict(atlasPath);
            string rgbaAtlasPath = atlasPath.Replace(".prefab", ".png");
            string materialPath = atlasPath.Replace(".prefab", "_mat.mat");
            _atlas = new Texture2D(AtlasMakerHelper.MAX_ATLAS_SIZE, AtlasMakerHelper.MAX_ATLAS_SIZE);
            _atlas.name = Path.GetFileNameWithoutExtension(atlasPath);
            Rect[] rects = _atlas.PackTextures(AtlasMakerHelper.GetPackTextures(texturePathList), 0, AtlasMakerHelper.MAX_ATLAS_SIZE, false);
            _atlas = AtlasOptimizer.Optimize(_atlas, rects, true);
            AtlasWriter.Write(_atlas, rgbaAtlasPath);
            AtlasMakerHelper.ImportRawAtlasTexture(rgbaAtlasPath);
            AtlasMakerHelper.CreateMaterial(SHADER_NAME, rgbaAtlasPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(GameObject)) as GameObject;
            GameObject go = PrefabUtility.InstantiatePrefab(prefab as Object) as GameObject;
            UIAtlas atlas = go.GetComponent<UIAtlas>();
            atlas.spriteMaterial = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            atlas.spriteList = GetSpriteDataList(rects, AtlasMakerHelper.GetPackTextureNames(texturePathList), AtlasMakerHelper.GetPackTextureBorders(texturePathList), _atlas.width, _atlas.height, spriteDataDict);
            atlas.MarkAsChanged();
            PrefabUtility.ReplacePrefab(go, prefab);
            GameObject.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
            ForceRefreshAllSprites();
            AtlasMakerHelper.LogAtlasSize(_atlas);
            EditorUtility.DisplayDialog("提示", string.Format("图集更新成功, {0} 宽：{1} 高：{2}", _atlas.name, _atlas.width, _atlas.height), "知道了~");
        }

        private static void RemoveRedundantAsset(string atlasPath)
        {
            AssetDatabase.DeleteAsset(atlasPath.Replace(".prefab", "_rgb.png"));
            AssetDatabase.DeleteAsset(atlasPath.Replace(".prefab", "_alpha.png"));
        }

        private static void ForceRefreshAllSprites()
        {
            UISprite[] sprites = GameObject.FindObjectsOfType(typeof(UISprite)) as UISprite[];
            for (int i = 0; i < sprites.Length; i++)
            {
                UISprite sprite = sprites[i];
                sprite.enabled = false;
                sprite.enabled = true;
            }
        }

        private static void CreateAtlas(string atlasPath, List<string> texturePathList)
        {
            _atlas = new Texture2D(AtlasMakerHelper.MAX_ATLAS_SIZE, AtlasMakerHelper.MAX_ATLAS_SIZE);
            _atlas.name = Path.GetFileNameWithoutExtension(atlasPath);
            string rgbaAtlasPath = atlasPath.Replace(".prefab", ".png");
            string materialPath = atlasPath.Replace(".prefab", "_mat.mat");
            Rect[] rects = _atlas.PackTextures(AtlasMakerHelper.GetPackTextures(texturePathList), 0, AtlasMakerHelper.MAX_ATLAS_SIZE, false);
            _atlas = AtlasOptimizer.Optimize(_atlas, rects, true);
            AtlasWriter.Write(_atlas, rgbaAtlasPath);
            AtlasMakerHelper.ImportRawAtlasTexture(rgbaAtlasPath);
            AtlasMakerHelper.CreateMaterial(SHADER_NAME, rgbaAtlasPath);
            string name = Path.GetFileNameWithoutExtension(atlasPath);
            GameObject go = new GameObject(name);
            UIAtlas atlas = go.AddComponent<UIAtlas>();
            atlas.spriteMaterial = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            atlas.spriteList = GetSpriteDataList(rects, AtlasMakerHelper.GetPackTextureNames(texturePathList), AtlasMakerHelper.GetPackTextureBorders(texturePathList), _atlas.width, _atlas.height, null);
            PrefabUtility.CreatePrefab(atlasPath, go);
            AssetDatabase.SaveAssets();
            GameObject.DestroyImmediate(go);
            AtlasMakerHelper.LogAtlasSize(_atlas);
            EditorUtility.DisplayDialog("提示", string.Format("图集创建成功, {0} 宽：{1} 高：{2}", _atlas.name, _atlas.width, _atlas.height), "知道了~");
        }

        //更新Atlas时记录已有Sprite的设置参数
        private static Dictionary<string, UISpriteData> GetSpriteDataDict(string atlasPath)
        {
            Dictionary<string, UISpriteData> result = new Dictionary<string, UISpriteData>();
            GameObject atlasPrefab = AssetDatabase.LoadAssetAtPath(atlasPath,typeof(GameObject)) as GameObject;
            UIAtlas atlas = atlasPrefab.GetComponent<UIAtlas>();
            for(int i = 0; i < atlas.spriteList.Count; i++)
            {
                UISpriteData spriteData = atlas.spriteList[i];
                if(result.ContainsKey(spriteData.name) == false)
                {
                    result.Add(spriteData.name, spriteData);
                }
            }
            return result;
        }

        //nGUI sprite data
        private static List<UISpriteData> GetSpriteDataList(Rect[] rects, string[] names, Vector4[] borders, int width, int height, Dictionary<string, UISpriteData> spriteDataDict)
        {
            List<UISpriteData> spriteDataList = new List<UISpriteData>();
            for (int i = 0; i < rects.Length; i++)
            {
                Rect rect = rects[i];
                Vector4 border = borders[i];
                UISpriteData spriteData = new UISpriteData();
                spriteData.name = names[i];
                spriteData.x = (int)(rect.xMin * width) + TextureClamper.BORDER;
                spriteData.y = (int)((1 - rect.yMax) * height) + TextureClamper.BORDER;
                spriteData.width = (int)(rect.width * width) - TextureClamper.BORDER * 2;
                spriteData.height = (int)(rect.height * height) - TextureClamper.BORDER * 2;
                if (spriteDataDict != null && spriteDataDict.ContainsKey(spriteData.name) == true)
                {
                    UISpriteData recordSpriteData = spriteDataDict[spriteData.name];
                    spriteData.borderLeft = recordSpriteData.borderLeft;
                    spriteData.borderBottom = recordSpriteData.borderBottom;
                    spriteData.borderRight = recordSpriteData.borderRight;
                    spriteData.borderTop = recordSpriteData.borderTop;
                }
                if (spriteData.borderLeft == 0 && spriteData.borderBottom == 0 && spriteData.borderRight == 0 && spriteData.borderTop == 0)
                {
                    spriteData.borderLeft = (int)border.x;
                    spriteData.borderBottom = (int)border.y;
                    spriteData.borderRight = (int)border.z;
                    spriteData.borderTop = (int)border.w;
                }
                spriteDataList.Add(spriteData);
            }
            return spriteDataList;
        }

#endif
    }
}