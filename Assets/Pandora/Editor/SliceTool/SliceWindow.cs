using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace com.tencent.pandora.tools
{
    public class SliceWindow : EditorWindow
    {
        public static SliceWindow window;
        public static TextureData selected;
        public static TextureData sliced;
        public static string selectedObjectPath;

        [MenuItem("Assets/九宫切图 %#x")]
        private static void Init()
        {
            window = (SliceWindow)EditorWindow.GetWindow(typeof(SliceWindow), true, "九宫切图", true);
            //window.minSize = new Vector2(650, 550);
            window.maxSize = new Vector2(650, 550);
            window.Show();
            selected = TextureManager.LoadTexture(selectedObjectPath);
            Vector4 border = Vector4.zero;
            if(selected.texture != null)
            {
                border = Slice.CalculateBorder(selected.texture);
                if(border != Vector4.zero)
                {
                    selected.borderLeft = (int)border.x + 1;
                    selected.borderBottom = (int)border.y + 1;
                    selected.borderRight = selected.texture.width - (int)border.z;
                    selected.borderTop = selected.texture.height - (int)border.w;
                }
            }
            sliced = null;
        }
        [MenuItem("Assets/九宫切图 %#x", true)]
        private static bool SliceToolMenuValidation()
        {
            selectedObjectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            bool isPicture = selectedObjectPath.EndsWith(".png") || selectedObjectPath.EndsWith(".jpg") || selectedObjectPath.EndsWith(".jpeg");
            if (!isPicture)
            {
                return false;
            }
            TextureImporter impoter = AssetImporter.GetAtPath(selectedObjectPath) as TextureImporter;
            bool isReadable = impoter.isReadable;

            if (!isReadable)
            {
                return false;
            }

            return true;
        }

        public void OnGUI()
        {
            //按钮区
            GUI.BeginGroup(new Rect(420, 460, 230, 150));
            DrawBorder(new Rect(0, 0, 220, 80));
            GUI.Label(new Rect(5, 0, 100, 18), "Border:");
            DrawSliceParamArea(selected);
            DrawSliceButton(selected);
            GUI.EndGroup();

            //图片显示区
            GUI.BeginGroup(new Rect(0, 0, 640, 230));
            GUI.Label(new Rect(5, 0, 100, 18), "OriginPreview:");
            ShowPicture(new Rect(10, 20, 620, 210), new Rect(250, 0, 150, 18), selected);
            GUI.EndGroup();

            GUI.BeginGroup(new Rect(0, 230, 640, 230));
            GUI.Label(new Rect(5, 0, 100, 18), "SlicedPreview:");
            ShowPicture(new Rect(10, 20, 620, 210), new Rect(250, 0, 150, 18), sliced);
            GUI.EndGroup();

            //快捷键
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                OnCloseWindow();
            }
            if (e.control == true && e.keyCode == KeyCode.S)
            {
                OnSave();
            }
        }
        static void ShowPicture(Rect drawRect, Rect sizeTipsRect, TextureData textureData)
        {
            if (textureData == null || textureData.texture == null)
            {
                return;
            }

            Rect outerRect = CalculateOuterRect(drawRect, textureData.width, textureData.height);
            //绘制背景
            DrawTiledTexture(outerRect, TextureManager.backgroundTexture);
            //绘制图片
            DrawTexture(outerRect, textureData.texture);
            //绘制参考线
            DrawReferenceLines(outerRect, textureData);
            //绘制外框线
            DrawBorder(outerRect);
            //绘制图片大小提示语
            DrawTextureSizeTips(sizeTipsRect, textureData);
        }

        static Rect CalculateOuterRect(Rect drawRect, int width, int height)
        {
            Rect outerRect = new Rect(drawRect.x, drawRect.y, width, height);
            //如果小于显示区域则按原样显示
            if (outerRect.width <= drawRect.width && outerRect.height <= drawRect.height)
            {
                outerRect.x += (drawRect.width - outerRect.width) * 0.5f;
                outerRect.y += (drawRect.height - outerRect.height) * 0.5f;
                return outerRect;
            }

            if (outerRect.width > 0)
            {
                float f = drawRect.width / outerRect.width;
                outerRect.width *= f;
                outerRect.height *= f;
            }
            if (drawRect.height > outerRect.height)
            {
                outerRect.y += (drawRect.height - outerRect.height) * 0.5f;
            }
            else if (outerRect.height > drawRect.height)
            {
                float f = drawRect.height / outerRect.height;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (drawRect.width > outerRect.width) outerRect.x += (drawRect.width - outerRect.width) * 0.5f;
            return outerRect;
        }

        static void DrawTiledTexture(Rect rect, Texture tex)
        {
            GUI.BeginGroup(rect);
            {
                int width = Mathf.RoundToInt(rect.width);
                int height = Mathf.RoundToInt(rect.height);

                for (int y = 0; y < height; y += tex.height)
                {
                    for (int x = 0; x < width; x += tex.width)
                    {
                        GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                    }
                }
            }
            GUI.EndGroup();
        }

        static void DrawTexture(Rect rect, Texture tex)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(rect, tex);
        }

        //绘制参考线
        static void DrawReferenceLines(Rect refRect, TextureData textureData)
        {
            GUI.BeginGroup(refRect);
            {
                Texture2D tex = TextureManager.contrastTexture;
                GUI.color = Color.white;

                if (textureData.borderLeft > 0)
                {
                    float x0 = (float)textureData.borderLeft / textureData.width * refRect.width - 1;
                    DrawTiledTexture(new Rect(x0, 0f, 1f, refRect.height), tex);
                }

                if (textureData.borderRight > 0)
                {
                    float x1 = (float)(textureData.width - textureData.borderRight) / textureData.width * refRect.width - 1;
                    DrawTiledTexture(new Rect(x1, 0f, 1f, refRect.height), tex);
                }

                if (textureData.borderBottom > 0)
                {
                    float y0 = (float)(textureData.height - textureData.borderBottom) / textureData.height * refRect.height - 1;
                    DrawTiledTexture(new Rect(0f, y0, refRect.width, 1f), tex);
                }

                if (textureData.borderTop > 0)
                {
                    float y1 = (float)textureData.borderTop / textureData.height * refRect.height - 1;
                    DrawTiledTexture(new Rect(0f, y1, refRect.width, 1f), tex);
                }
            }
            GUI.EndGroup();
        }

        static void DrawBorder(Rect outerRect)
        {
            // Draw the lines around the rect
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMin, outerRect.yMax));
            Handles.DrawLine(new Vector3(outerRect.xMax, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMax));
            Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMin));
            Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMax), new Vector3(outerRect.xMax, outerRect.yMax));
        }

        static void DrawTextureSizeTips(Rect sizeTipsRect, TextureData textureData)
        {
            string text = string.Format("Sprite Size: {0}x{1}", Mathf.RoundToInt(textureData.width), Mathf.RoundToInt(textureData.height));
            EditorGUI.DropShadowLabel(sizeTipsRect, text);
        }

        static void DrawSliceParamArea(TextureData textureData)
        {
            if (textureData == null)
            {
                return;
            }
            Color greenColor = new Color(0.4f, 1f, 0f, 1f);
            GUI.backgroundColor = greenColor;
            GUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUIUtility.labelWidth = 50f;
            textureData.borderLeft = EditorGUILayout.IntField("Left:", textureData.borderLeft, GUILayout.MaxWidth(85f));
            textureData.borderLeft = Mathf.Clamp(textureData.borderLeft, 0, textureData.width);
            GUILayout.Space(20f);
            textureData.borderRight = EditorGUILayout.IntField("Right:", textureData.borderRight, GUILayout.MaxWidth(85f));
            textureData.borderRight = Mathf.Clamp(textureData.borderRight, 0, textureData.width);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUIUtility.labelWidth = 50f;
            textureData.borderTop = EditorGUILayout.IntField("Top:", textureData.borderTop, GUILayout.MaxWidth(85f));
            textureData.borderTop = Mathf.Clamp(textureData.borderTop, 0, textureData.height);
            GUILayout.Space(20f);
            textureData.borderBottom = EditorGUILayout.IntField("Bottom:", textureData.borderBottom, GUILayout.MaxWidth(85f));
            textureData.borderBottom = Mathf.Clamp(textureData.borderBottom, 0, textureData.height);
            EditorGUILayout.EndHorizontal();
        }

        static void DrawSliceButton(TextureData textureData)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);
            if (GUILayout.Button("预览", GUILayout.MaxWidth(80f)))
            {
                OnSlice();
            }
            GUILayout.Space(3f);
            if (GUILayout.Button("生成并保存（Ctrl+S）", GUILayout.MaxWidth(130f)))
            {
                OnSave();
            }
            GUILayout.EndHorizontal();
        }

        static bool OnSlice()
        {
            if(selected.borderTop == 0 && selected.borderRight == 0 && selected.borderBottom == 0 && selected.borderLeft == 0)
            {
                EditorUtility.DisplayDialog("九宫参数错误", "上下左右九宫参数不能同时都为0", "马上就改~");
                return false;
            }
            if((selected.borderTop == 0 && selected.borderBottom != 0) ||(selected.borderTop != 0 && selected.borderBottom == 0))
            {
                EditorUtility.DisplayDialog("九宫参数错误", "上下九宫参数要同时都为0", "马上就改~");
                return false;
            }
            if ((selected.borderRight == 0 && selected.borderLeft != 0) || (selected.borderRight != 0 && selected.borderLeft == 0))
            {
                EditorUtility.DisplayDialog("九宫参数错误", "左右九宫参数要同时都为0", "马上就改~");
                return false;
            }
            if ((selected.borderRight + selected.borderLeft) >= selected.texture.width)
            {
                EditorUtility.DisplayDialog("九宫参数错误", "左右九宫参数之和不能大于图片宽度值", "马上就改~");
                return false;
            }
            if ((selected.borderTop + selected.borderBottom) >= selected.texture.height)
            {
                EditorUtility.DisplayDialog("九宫参数错误", "上下九宫参数之和不能大于图片高度值", "马上就改~");
                return false;
            }
            if(selected.borderTop == 0 && selected.borderBottom == 0) //垂直方向不需要做拉伸变化
            {
                selected.borderTop = selected.texture.height / 2;
                selected.borderBottom = selected.texture.height - selected.borderTop - 1;
            }
            if (selected.borderRight == 0 && selected.borderLeft == 0) //水平方向不需要做拉伸变化
            {
                selected.borderRight = selected.texture.width / 2;
                selected.borderLeft = selected.texture.width - selected.borderRight - 1;
            }
            Texture2D tex = Slice.SliceTexture(selected.texture, selected.borderTop, selected.borderRight, selected.borderBottom, selected.borderLeft);
            sliced = new TextureData(tex);
            return true;
        }

        static void OnSave()
        {
            if(OnSlice() == true)
            {
                string savePath = GetSavePath(selected.path, selected.borderLeft, selected.borderRight, selected.borderTop, selected.borderBottom);
                TextureManager.SaveAsPng(sliced.texture, savePath);
                OnCloseWindow();
                //重新导入图片
                string slicedRelativePath = savePath.Substring(savePath.IndexOf("Assets"));
                RefreshAsset(slicedRelativePath);
            }
        }

        static void OnCloseWindow()
        {
            selected = null;
            sliced = null;
            window.Close();
        }

        static string GetSavePath(string relativePath, int left, int right, int top, int bottom)
        {
            string pictureName = Path.GetFileNameWithoutExtension(relativePath);
            string saveName = pictureName + string.Format("@l{0}_r{1}_t{2}_b{3}.png", left, right, top, bottom);
            string folderPath = Path.GetDirectoryName(relativePath);
            string savePath = Path.Combine(Application.dataPath + folderPath.Replace("Assets", ""), saveName);
            return savePath;
        }

        static void RefreshAsset(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

    }
}