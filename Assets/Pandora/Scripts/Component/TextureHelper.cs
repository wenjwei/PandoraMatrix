#define USING_UGUI
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{
    /// <summary>
    /// 收住Texture显示、更换、销毁的口子，以方便及时销毁Texture对象，释放内存
    /// </summary>
#if USING_UGUI
    [RequireComponent(typeof(UnityEngine.UI.MaskableGraphic))]
#else
    [RequireComponent(typeof(UITexture))]
#endif
    [CustomLuaClass]
    public class TextureHelper : MonoBehaviour
    {
        private static Dictionary<string, TextureWrapper> _dict = new Dictionary<string, TextureWrapper>();
        private static List<string> _deletedKeyList = new List<string>();

        public void Show(Texture texture)
        {
            Texture existTexture = GetExistTexture();
            Remove(existTexture);
            ApplyTexture(texture);
            Add(texture);
        }

        private Texture GetExistTexture()
        {
            Texture result = null;
#if USING_UGUI
            UnityEngine.UI.RawImage rawImage = this.gameObject.GetComponent<UnityEngine.UI.RawImage>();
            if(rawImage != null)
            {
                result = rawImage.texture;
            }
            UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
            if(image != null && image.sprite != null)
            {
                result = image.sprite.texture;
            }
#else
            result = this.gameObject.GetComponent<UITexture>().mainTexture;
#endif
            return result;
        }

        private void ApplyTexture(Texture texture)
        {
#if USING_UGUI
            UnityEngine.UI.RawImage rawImage = this.gameObject.GetComponent<UnityEngine.UI.RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = texture;
            }
            UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                Sprite sprite = Sprite.Create(texture as Texture2D, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
            }
#else
            this.gameObject.GetComponent<UITexture>().mainTexture = texture;
#endif
        }

        private void ClearTexture()
        {
#if USING_UGUI
            UnityEngine.UI.RawImage rawImage = this.gameObject.GetComponent<UnityEngine.UI.RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = null;
            }
            UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (image != null && image.sprite != null)
            {
                image.sprite = null;
            }
#else
            this.gameObject.GetComponent<UITexture>().mainTexture = null;
#endif
        }

        private static void Add(Texture texture)
        {
            if(texture == null || string.IsNullOrEmpty(texture.name) == true)
            {
                return;
            }
            TextureWrapper wrapper = null;
            if (_dict.ContainsKey(texture.name) == true)
            {
                wrapper = _dict[texture.name];
                wrapper.referenceCount += 1;
            }
            else
            {
                wrapper = new TextureWrapper();
                wrapper.referenceCount = 1;
                wrapper.reference = new WeakReference(texture);
                _dict.Add(texture.name, wrapper);
            }
        }

        private static void Remove(Texture texture)
        {
            if (texture == null || string.IsNullOrEmpty(texture.name) == true)
            {
                return;
            }
            TextureWrapper wrapper = null;
            if (_dict.ContainsKey(texture.name) == true)
            {
                wrapper = _dict[texture.name];
                wrapper.referenceCount -= 1;
            }
        }

        public void Clear()
        {
            Texture texture = GetExistTexture();
            Remove(texture);
            ClearTexture();
        }

        private void OnDestroy()
        {
            Texture texture = GetExistTexture();
            Remove(texture);
        }

        public static void GC()
        {
            AssetManager.DeleteZeroReferenceAsset(true);
            var enumerator = _dict.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var kvp = enumerator.Current;
                TextureWrapper wrapper = kvp.Value;
                if (wrapper.reference.Target == null || wrapper.reference.Target.ToString() == "null")
                {
                    _deletedKeyList.Add(kvp.Key);
                }
                else
                {
                    if(wrapper.referenceCount <= 0)
                    {
                        Texture.Destroy(wrapper.reference.Target as Texture);
                        _deletedKeyList.Add(kvp.Key);
                    }
                }
            }
            for(int i = 0; i < _deletedKeyList.Count; i++)
            {
                _dict.Remove(_deletedKeyList[i]);
            }
            _deletedKeyList.Clear();
        }

        class TextureWrapper
        {
            public int referenceCount;
            public WeakReference reference; //此处保持对Texture的弱引用，不影响Resources.UnloadUnusedAssets接口回收内存
        }

    }
}

