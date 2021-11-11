//#define USING_NGUI
#define USING_UGUI
using System;
using UnityEngine;
using System.Collections;

namespace com.tencent.pandora
{
    /// <summary>
    /// 打包时面板资源会将UnityEngine.UI.Text的font置空，同时添加TextPartner组件
    /// 在该组件中记录字体资源信息，在面板创建后在该组件Awake时，通过GetFont找到字体资源并赋值给Text组件。
    /// </summary>
    public class TextPartner : MonoBehaviour
    {
        public static Func<string, Font> GetFont;
        
        public string fontName;

        private void Awake()
        {
#if USING_NGUI
            UILabel label = gameObject.GetComponent<UILabel>();
            if(label != null && string.IsNullOrEmpty(fontName) == false && GetFont != null)
            {
                label.trueTypeFont = GetFont(fontName);
            }

            UIPopupList popup = gameObject.GetComponent<UIPopupList>();
            if(popup != null && string.IsNullOrEmpty(fontName) == false && GetFont != null)
            {
                popup.trueTypeFont = GetFont(fontName);
            }
#endif

#if USING_UGUI
            UnityEngine.UI.Text text = this.gameObject.GetComponent<UnityEngine.UI.Text>();
            if (text != null && string.IsNullOrEmpty(fontName) == false && GetFont != null)
            {
                text.font = GetFont(fontName);
            }
#endif
        }
    }
}

