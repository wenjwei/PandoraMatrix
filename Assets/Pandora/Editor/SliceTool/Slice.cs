using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slice
{
    private static int[] horizontalBuffer;

    //计算9宫图片的boder参数
    public static Vector4 CalculateBorder(Texture2D source)
    {
        int left = 0;
        int right = source.width;
        for (int i = 0; i < source.height; i++)
        {
            Color[] row = source.GetPixels(0, i, source.width, 1);
            Vector2 rowRange = FindRepeatPixelRange(row);
            left = Mathf.Max(left, (int)rowRange.x);
            right = Mathf.Min(right, (int)rowRange.y);
        }
        int top = 0;
        int bottom = source.height;
        for (int i = 0; i < source.width; i++)
        {
            Color[] column = source.GetPixels(i, 0, 1, source.height);
            Vector2 columnRange = FindRepeatPixelRange(column);
            top = Mathf.Max(top, (int)columnRange.x);
            bottom = Mathf.Min(bottom, (int)columnRange.y);
        }
        if (left > right && top > bottom)
        {
            return Vector4.zero;
        }
        if (top >= bottom)//横向可九宫
        {
            top = source.height / 2 -1;
            bottom = top + 2;
        }
        if(left >= right)//纵向可九宫
        {
            left = source.width / 2 -1;
            right = left + 2;
        }
        return new Vector4(left, top, right, bottom);
    }

    private static Vector2 FindRepeatPixelRange(Color[] colors)
    {
        int max = 0;        //记录最大重复像素数量
        int maxIndex = 0;   //重复像素最后一个像素的索引值
        int current = 0;    //计算当前重复像素数目值
        for (int i = 1; i < colors.Length; i++)
        {
            if(ColorEqual(colors[i], colors[i-1]) == false)
            {
                if (current > max)
                {
                    max = current;
                    maxIndex = i - 1;
                }
                current = 0;
            }
            else
            {
                current += 1;
            }
        }
        if (current > max)
        {
            max = current;
            maxIndex = colors.Length - 1;
        }
        return new Vector2(maxIndex - max, maxIndex);
    }

    public static bool ColorEqual(Color a, Color b)
    {
        float threshold = 0.002f;
        if(Mathf.Abs(a.r - b.r) > threshold)
        {
            return false;
        }
        if (Mathf.Abs(a.g - b.g) > threshold)
        {
            return false;
        }
        if (Mathf.Abs(a.b - b.b) > threshold)
        {
            return false;
        }
        if (Mathf.Abs(a.a - b.a) > threshold)
        {
            return false;
        }
        return true;
    }

    public static Texture2D SliceTexture(Texture2D source, int top, int right, int bottom, int left)
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
