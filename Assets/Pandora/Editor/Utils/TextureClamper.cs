using UnityEngine;

namespace com.tencent.pandora.tools
{
    public class TextureClamper
    {
        /// <summary>
        /// 做图集的时候，给单元图片四周补2像素，否则界面缩放时会出现黑标或透明边的现象
        /// </summary>
        public const int BORDER = 2;

        public static Texture2D Clamp(Texture2D sourceTexture)
        {
            int sourceWidth = sourceTexture.width;
            int sourceHeight = sourceTexture.height;
            Color32[] sourcePixels = sourceTexture.GetPixels32();
            int targetWidth = sourceWidth + BORDER * 2;
            int targetHeight = sourceHeight + BORDER * 2;
            Color32[] targetPixels = new Color32[targetWidth * targetHeight];
            Texture2D targetTexture = new Texture2D(targetWidth, targetHeight);
            for (int i = 0; i < sourceHeight; i++)
            {
                for (int j = 0; j < sourceWidth; j++)
                {
                    targetPixels[(i + BORDER) * targetWidth + (j + BORDER)] = sourcePixels[i * sourceWidth + j];
                }
            }
            //左边缘
            for (int v = 0; v < sourceHeight; v++)
            {
                for (int k = 0; k < BORDER; k++)
                {
                    targetPixels[(v + BORDER) * targetWidth + k] = sourcePixels[v * sourceWidth];
                }
            }
            //右边缘
            for (int v = 0; v < sourceHeight; v++)
            {
                for (int k = 0; k < BORDER; k++)
                {
                    targetPixels[(v + BORDER) * targetWidth + (sourceWidth + BORDER + k)] = sourcePixels[v * sourceWidth + sourceWidth - 1];
                }
            }
            //上边缘
            for (int h = 0; h < sourceWidth; h++)
            {
                for (int k = 0; k < BORDER; k++)
                {
                    targetPixels[(sourceHeight + BORDER + k) * targetWidth + BORDER + h] = sourcePixels[(sourceHeight - 1) * sourceWidth + h];
                }
            }
            //下边缘
            for (int h = 0; h < sourceWidth; h++)
            {
                for (int k = 0; k < BORDER; k++)
                {
                    targetPixels[k * targetWidth + BORDER + h] = sourcePixels[h];
                }
            }
            targetTexture.SetPixels32(targetPixels);
            targetTexture.Apply();
            return targetTexture;
        }
    }
}
