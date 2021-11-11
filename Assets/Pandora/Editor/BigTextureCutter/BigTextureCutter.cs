#define USING_UGUI
//#define USING_NGUI

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.tencent.pandora.tools
{
    public class BigTextureCutter
    {
        //做图集的时候为了消除黑边问题边缘会各补2像素
        public const int DOUBLE_BORDER = 4;

        private static string _selectedTexturePath;

#if USING_UGUI
        [MenuItem("PandoraTools/BigTextureCutter_UGUI")]
        [MenuItem("Assets/BigTextureCutter_UGUI")]
        public static void Cut_UGUI()
        {
            _selectedTexturePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            bool isTexture = _selectedTexturePath.EndsWith(".png") || _selectedTexturePath.EndsWith(".jpg") || _selectedTexturePath.EndsWith(".jpeg");
            if (isTexture == false)
            {
                //选择的资源不是图片
                return;
            }
            ImportReadableTexture(_selectedTexturePath);
            DoCut_UGUI(_selectedTexturePath);
        }
#endif

#if USING_NGUI
        [MenuItem("PandoraTools/BigTextureCutter_NGUI")]
        [MenuItem("Assets/BigTextureCutter_NGUI")]
        public static void Cut_NGUI()
        {
            _selectedTexturePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            bool isTexture = _selectedTexturePath.EndsWith(".png") || _selectedTexturePath.EndsWith(".jpg") || _selectedTexturePath.EndsWith(".jpeg");
            if (isTexture == false)
            {
                //选择的资源不是图片
                return;
            }
            ImportReadableTexture(_selectedTexturePath);
            DoCut_NGUI(_selectedTexturePath);
        }
#endif

        private static void ImportReadableTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

#if UNITY_5_5_OR_NEWER
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
#else
            importer.textureType = TextureImporterType.Advanced;
            importer.textureFormat = TextureImporterFormat.RGBA32;
#endif
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

#if USING_UGUI
        private static void DoCut_UGUI(string path)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            Size source = new Size(texture.width, texture.height);
            Size closest = Size.GetClosestPT(source);
            List<Rect> sliceRectList = GetSliceRectList(source, closest);
            Texture2D[] slices = CreateSlices(texture, sliceRectList);
            List<string> sliceTexturePathList = SaveSlices(slices, path);
            string atlasPath = CreateAtlas_UGUI(sliceTexturePathList);
            CreateSliceGameObject_UGUI(atlasPath, sliceTexturePathList, sliceRectList, texture.width, texture.height);
        }
#endif

#if USING_NGUI
        private static void DoCut_NGUI(string path)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            Size source = new Size(texture.width, texture.height);
            Size closest = Size.GetClosestPT(source);
            List<Rect> sliceRectList = GetSliceRectList(source, closest);
            Texture2D[] slices = CreateSlices(texture, sliceRectList);
            List<string> sliceTexturePathList = SaveSlices(slices, path);
            string atlasPath = CreateAtlas_NGUI(sliceTexturePathList);
            CreateSliceGameObject_NGUI(atlasPath, sliceTexturePathList, sliceRectList, texture.width, texture.height);
        }
#endif

        private static Texture2D[] CreateSlices(Texture2D texture, List<Rect> sliceRectList)
        {
            Texture2D[] result = new Texture2D[sliceRectList.Count];
            for (int i = 0; i < sliceRectList.Count; i++)
            {
                Rect rect = sliceRectList[i];
                Texture2D slice = new Texture2D((int)rect.width, (int)rect.height);
                Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                slice.SetPixels(pixels);
                slice.Apply();
                result[i] = slice;
                if (result.Length > 1)
                {
                    result[i] = TextureClamper.Clamp(slice);
                }
            }
            return result;
        }

        private static List<string> SaveSlices(Texture2D[] slices, string selectedTexturePath)
        {
            List<string> result = new List<string>();
            string selectedTextureName = Path.GetFileNameWithoutExtension(selectedTexturePath);
            for (int i = 0; i < slices.Length; i++)
            {
                string sliceName = selectedTextureName + "_@slice_" + i.ToString();
                string folderPath = Path.GetDirectoryName(selectedTexturePath);
                string savePath = Path.Combine(Application.dataPath + folderPath.Replace("Assets", ""), sliceName + ".png");
                byte[] bytes = slices[i].EncodeToPNG();
                File.WriteAllBytes(savePath, bytes);
                string sliceTexturePath = Path.Combine(folderPath, sliceName + ".png");
                AssetDatabase.ImportAsset(sliceTexturePath, ImportAssetOptions.ForceUpdate);
                ImportReadableTexture(sliceTexturePath);
                result.Add(sliceTexturePath);
            }
            return result;
        }

#if USING_UGUI
        private static string CreateAtlas_UGUI(List<string> sliceTexturePathList)
        {
            return AtlasMaker_UGUI.MakeAtlas(sliceTexturePathList, true);
        }
#endif

#if USING_NGUI
        private static string CreateAtlas_NGUI(List<string> sliceTexturePathList)
        {
            return AtlasMaker_NGUI.MakeAtlas(sliceTexturePathList);
        }
#endif

#if USING_UGUI
        private static void CreateSliceGameObject_UGUI(string atlasPath, List<string> slicePathList, List<Rect> sliceRectList, int originalWidth, int originalHeight)
        {
            if (string.IsNullOrEmpty(atlasPath) || slicePathList.Count == 0 || sliceRectList.Count == 0)
            {
                return;
            }
            atlasPath = atlasPath.Replace(".png", "_rgb.png");
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
            Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
            foreach (UnityEngine.Object obj in objs)
            {
                if (obj is Sprite)
                {
                    spriteDict.Add(obj.name, obj as Sprite);
                }
            }
            string materialPath = atlasPath.Replace("_rgb.png", "_mat.mat");
            Material material = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            string rootName = Path.GetFileNameWithoutExtension(slicePathList[0]);
            rootName = rootName.Substring(0, rootName.IndexOf("@") - 1);
            GameObject root = new GameObject("BigTexture_" + rootName);
            RectTransform rootTrans = root.AddComponent<RectTransform>();
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Debug.LogError("未发现名字为Canvas的节点");
            }
            else
            {
                rootTrans.SetParent(canvas.transform);
                rootTrans.anchorMin = new Vector2(0, 1);
                rootTrans.anchorMax = new Vector2(0, 1);
                rootTrans.pivot = new Vector2(0, 1);
                rootTrans.sizeDelta = new Vector2(originalWidth, originalHeight);
                rootTrans.anchoredPosition = new Vector2(0, 0);
            }
            for (int i = 0; i < slicePathList.Count; i++)
            {
                string sliceName = Path.GetFileNameWithoutExtension(slicePathList[i]);
                GameObject slice = new GameObject(sliceName);
                RectTransform sliceTrans = slice.AddComponent<RectTransform>();
                sliceTrans.SetParent(rootTrans);
                sliceTrans.anchorMin = new Vector2(0, 0);
                sliceTrans.anchorMax = new Vector2(0, 0);
                sliceTrans.pivot = new Vector2(0, 0);
                Rect rect = sliceRectList[i];
                sliceTrans.anchoredPosition = new Vector2(rect.x, rect.y);
                sliceTrans.sizeDelta = new Vector2(rect.width, rect.height);
                UnityEngine.UI.Image image = slice.AddComponent<UnityEngine.UI.Image>();
                image.sprite = spriteDict[sliceName];
                image.material = material;
            }
        }
#endif
#if USING_NGUI
        private static void CreateSliceGameObject_NGUI(string atlasPath, List<string> slicePathList, List<Rect> sliceRectList, int originalWidth, int originalHeight)
        {
            if (string.IsNullOrEmpty(atlasPath) || slicePathList.Count == 0 || sliceRectList.Count == 0)
            {
                return;
            }
            UIAtlas atlas = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(UIAtlas)) as UIAtlas;
            //string materialPath = atlasPath.Replace(".prefab", "_mat.mat");
            //Material material = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            string rootName = Path.GetFileNameWithoutExtension(slicePathList[0]);
            rootName = rootName.Substring(0, rootName.IndexOf("@") - 1);
            GameObject root = new GameObject("BigTexture_" + rootName);
            Transform rootTrans = root.GetComponent<Transform>();
            UIWidget rootWidget = root.AddComponent<UIWidget>();
            rootWidget.pivot = UIWidget.Pivot.Center;
            rootWidget.width = originalWidth;
            rootWidget.height = originalHeight;
            GameObject uiRoot = GameObject.Find("UI Root");
            if (uiRoot == null)
            {
                Debug.LogError("未发现名字为UI Root的节点");
            }
            else
            {
                rootTrans.SetParent(uiRoot.transform);
                rootTrans.localScale = Vector3.one;
                rootTrans.localPosition = Vector3.zero;
            }
            for (int i = 0; i < slicePathList.Count; i++)
            {
                string sliceName = Path.GetFileNameWithoutExtension(slicePathList[i]);
                GameObject slice = new GameObject(sliceName);
                Transform sliceTrans = slice.GetComponent<Transform>();
                sliceTrans.SetParent(rootTrans);
                Rect rect = sliceRectList[i];
                UISprite sprite = slice.AddComponent<UISprite>();
                sprite.pivot = UIWidget.Pivot.Center;
                sprite.width = (int)rect.width;
                sprite.height = (int)rect.height + 1;
                sprite.atlas = atlas;
                sprite.spriteName = sliceName;
                sliceTrans.localScale = Vector3.one;
                sliceTrans.localPosition = new Vector3(rect.width / 2 + rect.x - originalWidth / 2, rect.height / 2 + rect.y - originalHeight / 2);
            }
        }
#endif

        /// <summary>
        /// 输入：大图的宽，高
        /// 输出：切片的尺寸列表
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static List<Rect> GetSliceRectList(Size sourceSize, Size closestPTSize)
        {
            Size recordClosestSize = closestPTSize;
            Size recordSourceSize = sourceSize;
            List<Rect> result = new List<Rect>();
            float startX = 0;
            float startY = 0;
            while (sourceSize.Area() > 0
                && ((sourceSize.w + DOUBLE_BORDER) >= closestPTSize.w || (sourceSize.h + DOUBLE_BORDER) >= closestPTSize.h))
            {
                if ((sourceSize.w + DOUBLE_BORDER) < closestPTSize.w)
                {
                    sourceSize.h = sourceSize.h - (closestPTSize.h - DOUBLE_BORDER);
                    closestPTSize.w = closestPTSize.w - (sourceSize.w + DOUBLE_BORDER);
                    Rect rect = new Rect(startX, startY, sourceSize.w, closestPTSize.h - DOUBLE_BORDER);
                    startY += rect.height;
                    result.Add(rect);
                }
                else
                {
                    closestPTSize.h = closestPTSize.h - (sourceSize.h + DOUBLE_BORDER);
                    sourceSize.w = sourceSize.w - (closestPTSize.w - DOUBLE_BORDER);
                    Rect rect = new Rect(startX, startY, closestPTSize.w - DOUBLE_BORDER, sourceSize.h);
                    startX += rect.width;
                    result.Add(rect);
                }
                if (sourceSize.Area() >= closestPTSize.Area())
                {
                    Size expandSize = recordClosestSize.Expand();
                    Debug.Log("切片边缘增加的面积导致不能制作该尺寸图集，将Atlas面积扩容一倍再切: " + expandSize.ToString());
                    return GetSliceRectList(recordSourceSize, recordClosestSize.Expand());
                }
            }
            if (sourceSize.Area() > 0)
            {
                Rect rect = new Rect(startX, startY, sourceSize.w, sourceSize.h);
                result.Add(rect);
            }
            return result;
        }
    }

    internal struct Size
    {
        public int w;
        public int h;

        public Size(int w, int h)
        {
            this.w = w;
            this.h = h;
        }

        public int Area()
        {
            return w * h;
        }

        public Size Expand()
        {
            Size result = new Size(this.w, this.h);
            if (result.w < result.h)
            {
                result.w = result.w * 2;
            }
            else
            {
                result.h = result.h * 2;
            }
            return result;
        }

        public override string ToString()
        {
            return w.ToString() + " , " + h.ToString();
        }

        public static Size GetClosestPT(Size value)
        {
            float side = Mathf.Pow((float)value.Area(), 0.5f);
            int PTside = Mathf.ClosestPowerOfTwo((int)side);
            return new Size(PTside, PTside);
        }

    }
}