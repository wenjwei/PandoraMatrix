using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

namespace com.tencent.pandora.tools
{
    /// <summary>
    /// 切换平台打包时，资源（主要是图片）需要重新导入时，因为图片需要根据不同平台重新压缩，该过程比较耗时。
    /// 解决方法：将不需要打包的图片导入设置为不压缩，真彩色
    /// </summary>
    public class TexturePostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// 此处加入导入图片为不压缩、真彩色的文件夹列表
        /// </summary>
        private static List<string> UNCOMPRESS_FOLDER_LIST = new List<string>() { "Assets/CACHE/", "Assets/NGUI/Editor" };

        /// <summary>
        /// 面板资源的原始贴图以真32导入
        /// </summary>
        private static Regex PANEL_TEXTURE_PATH = new Regex(@"Assets/Actions/Resources/.*?/Textures/", RegexOptions.IgnoreCase);


        private void OnPreprocessTexture()
        {
            ImportUncompressTexture();
            ImportPanelRawTexture();
        }

        //面板的原始图片和不参与面板的其他图片资源使用真32导入
        private void ImportPanelRawTexture()
        {
            if (PANEL_TEXTURE_PATH.IsMatch(this.assetPath) == true)
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;

#if UNITY_5_5_OR_NEWER
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
#else
                textureImporter.textureType = TextureImporterType.Advanced;
                textureImporter.textureFormat = TextureImporterFormat.RGBA32;
#endif
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = true;
                textureImporter.alphaIsTransparency = true;
                textureImporter.spriteImportMode = SpriteImportMode.None;
            }
        }

        private void ImportUncompressTexture()
        {
            if (IsUncompressTexture(this.assetPath) == true)
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
#if UNITY_4_6 || UNITY_4_7 || UNITY_5
                textureImporter.textureFormat = TextureImporterFormat.RGBA32;
#elif UNITY_2017_1_OR_NEWER
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
#endif
            }
        }

        private bool IsUncompressTexture(string path)
        {
            foreach (string s in UNCOMPRESS_FOLDER_LIST)
            {
                if (path.Contains(s) == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

