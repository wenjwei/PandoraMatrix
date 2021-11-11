//#define USING_NGUI
#define USING_UGUI

using UnityEngine;
using UnityEditor;
using System.Collections;


namespace com.tencent.pandora.tools
{
    public class NGUITools
    {
#if USING_NGUI
        [MenuItem("PandoraTools/NGUI/Rough Arrange Depth")]
        static public void RoughArrangeDepths()
        {
            GameObject root = GameObject.Find("UIRoot");
            if(root == null)
            {
                root = GameObject.Find("UI Root");
            }
            if(root == null)
            {
                EditorUtility.DisplayDialog("����", "û�ҵ���ΪUI Root�ĸ��ڵ�", "���ϴ���~");
                return;
            }
            UIPanel[] panels = root.GetComponentsInChildren<UIPanel>(true);
            if (panels.Length > 1)
            {
                int baseDepth = panels[0].depth;
                for (int i = 0; i < panels.Length; i++)
                {
                    UIPanel panel = panels[i];
                    panel.depth = baseDepth + i;
                    ArrangeDepthsInPanel(panel);
                }
            }
        }

        static private void ArrangeDepthsInPanel(UIPanel panel)
        {
            UIWidget[] widgets = panel.gameObject.GetComponentsInChildren<UIWidget>(true);
            for (int i = 0; i < widgets.Length; i++)
            {
                UIWidget w = widgets[i];
                w.depth = i + 1;
            }
        }
        
#endif
    }
}