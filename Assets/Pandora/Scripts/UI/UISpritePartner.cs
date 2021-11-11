//#define USING_NGUI
#define USING_UGUI

using UnityEngine;
using System.Collections;

/// <summary>
/// nGUI中销毁面板时，解除Sprite和atlas资源的引用关系，使图集
/// </summary>
namespace com.tencent.pandora
{
    public class UISpritePartner : MonoBehaviour
    {
#if USING_NGUI
        private void OnDestroy()
        {
            UISprite sprite = this.gameObject.GetComponent<UISprite>();
            if (sprite != null)
            {
                sprite.atlas = null;
                sprite.spriteName = string.Empty;
            }
        }
#endif
    }
}

