using UnityEngine;

namespace com.tencent.pandora.tools
{
    public class AtlasOptimizer
    {
        /// <summary>
        /// 优化图集
        /// 强制设置正方形Atlas
        /// </summary>
        public static Texture2D Optimize(Texture2D atlas, Rect[] rects, bool forceSquare = false)
        {
            Texture2D result = atlas;
            if(forceSquare == true)
            {
                while(result.width > result.height)
                {
                    result = CreateResizedAtlas(result, 1.0f, 2.0f, rects);
                }
                while(result.width < result.height)
                {
                    result = CreateResizedAtlas(result, 2.0f, 1.0f, rects);
                }
            }
            return result;
        }

        private static Texture2D CreateResizedAtlas(Texture2D atlas, float xScale, float yScale, Rect[] rects)
        {
            int width = (int)(atlas.width * xScale);
            int height = (int)(atlas.height * yScale);
            Texture2D result = new Texture2D(width, height);
            result.name = atlas.name;
            int pixelWidth = width > atlas.width ? atlas.width : width;
            int pixelHeight = height > atlas.height ? atlas.height : height;
            result.SetPixels(0, 0, pixelWidth, pixelHeight, atlas.GetPixels(0, 0, pixelWidth, pixelHeight));
            result.Apply();
            for(int i = 0; i < rects.Length; i++)
            {
                Rect rect = rects[i];
                rects[i] = new Rect(rect.xMin / xScale, rect.yMin / yScale, rect.width / xScale, rect.height / yScale);
            }
            return result;
        }

        private static Rect GetAtlasContentRect(Rect[] rects)
        {
            Rect result = new Rect(0, 0, 0, 0);
            foreach(Rect rect in rects)
            {
                if(rect.xMin < result.xMin)
                {
                    result.xMin = rect.xMin;
                }
                if(rect.yMin < result.yMin)
                {
                    result.yMin = rect.yMin;
                }
                if(rect.xMax > result.xMax)
                {
                    result.xMax = rect.xMax;
                }
                if(rect.yMax > result.yMax)
                {
                    result.yMax = rect.yMax;
                }
            }
            return result;
        }
    }
}
