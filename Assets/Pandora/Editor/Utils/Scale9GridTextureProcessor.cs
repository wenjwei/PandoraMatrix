using UnityEngine;

namespace com.tencent.pandora.tools
{
    public class Scale9GridTextureProcessor
    {
        public static Texture2D Process(Texture2D source, int top, int right, int bottom, int left)
        {
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            if (top == 0 || bottom == 0)
            {
                top = sourceHeight / 2;
                bottom = sourceHeight - top - 1;
            }
            if (right == 0 || left == 0)
            {
                right = sourceWidth / 2;
                left = sourceWidth - right - 1;
            }
            Color32[] sourcePixels = source.GetPixels32();
            int targetWidth = left + 1 + right;
            int targetHeight = top + 1 + bottom;
            Color32[] targetPixels = new Color32[targetWidth * targetHeight];
            Texture2D target = new Texture2D(targetWidth, targetHeight);
            int pixelIndex = 0;
            for (int i = 0; i < sourceHeight; i++)
            {
                if (i > bottom && i < (sourceHeight - top))
                {
                    continue;
                }
                for (int j = 0; j < sourceWidth; j++)
                {
                    if (j > left && j < (sourceWidth - right))
                    {
                        continue;
                    }
                    targetPixels[pixelIndex++] = sourcePixels[i * sourceWidth + j];
                }
            }
            target.SetPixels32(targetPixels);
            target.Apply();
            return target;
        }
    }
}
