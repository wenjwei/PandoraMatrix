using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace com.tencent.pandora.tools
{
    public class TextureData
    {
        public Texture2D texture = null;
        public string path = "";
        public int width = 0;
        public int height = 0;

        public int borderLeft = 0;
        public int borderRight = 0;
        public int borderTop = 0;
        public int borderBottom = 0;

        public TextureData()
        {

        }
        public TextureData(Texture2D texture)
        {
            this.texture = texture;
            this.width = texture.width;
            this.height = texture.height;
        }
    }

    public class TextureManager
    {
        public static Texture2D mBackgroundTexture;
        public static Texture2D mContrastTexture;

        public static TextureData LoadTexture(string picPath)
        {
            TextureData textureData = new TextureData();
            if (picPath.StartsWith("Assets"))
            {
                textureData.path = picPath;
                textureData.texture = AssetDatabase.LoadAssetAtPath(picPath, typeof(Texture2D)) as Texture2D;
                textureData.width = textureData.texture.width;
                textureData.height = textureData.texture.height;
            }
            else
            {
                Debug.LogError("传入的资源路径不正确："+picPath);
            }
            return textureData;
        }

        static public Texture2D backgroundTexture
        {
            get
            {
                if (mBackgroundTexture == null)
                {
                    mBackgroundTexture = CreateCheckerTexture(new Color(0.1f, 0.1f, 0.1f, 0.5f),new Color(0.2f, 0.2f, 0.2f, 0.5f));
                }
                return mBackgroundTexture;
            }
        }

        static public Texture2D contrastTexture
        {
            get
            {
                if (mContrastTexture == null)
                {
                    mContrastTexture = CreateCheckerTexture(new Color(0f, 0.0f, 0f, 0.5f),new Color(1f, 1f, 1f, 0.5f));
                }
                return mContrastTexture;
            }
        }
        static Texture2D CreateCheckerTexture(Color c0, Color c1)
        {
            Texture2D tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (int y = 0; y < 8; ++y) 
            {
                for (int x = 0; x < 8; ++x)
                {
                    tex.SetPixel(x, y, c1);
                }
            }

            for (int y = 8; y < 16; ++y) 
            {
                for (int x = 0; x < 8; ++x) 
                {
                    tex.SetPixel(x, y, c0);
                }
            }

            for (int y = 0; y < 8; ++y)
            {
                for (int x = 8; x < 16; ++x)
                {
                    tex.SetPixel(x, y, c0);
                }
            }

            for (int y = 8; y < 16; ++y)
            {
                for (int x = 8; x < 16; ++x)
                {
                    tex.SetPixel(x, y, c1);
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        public static void SaveAsPng(Texture2D source, string destPath)
        {
            byte[] bytes = source.EncodeToPNG();
            File.WriteAllBytes(destPath, bytes);
            EditorUtility.DisplayDialog("提示", "Clamp 图片生成成功，保存路径为：" + destPath, "OK");
        }


    }
}