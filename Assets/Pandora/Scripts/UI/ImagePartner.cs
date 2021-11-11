//#define USING_NGUI
#define USING_UGUI

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace com.tencent.pandora
{
    public class ImagePartner : MonoBehaviour
    {
#if USING_UGUI
        private void OnDestroy()
        {
            Image image = this.gameObject.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.material = null;
            }
        }
#endif
    }
}

