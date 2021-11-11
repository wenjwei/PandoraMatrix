using UnityEngine;
using UnityEditor;
using System.Collections;

using PPM.TreeView;

namespace PPM
{
	public class MenuTreeIMGUI : TreeIMGUI<MenuData>
	{

		public MenuTreeIMGUI(TreeNode<MenuData> root) : base(root)
		{
			
		}

		protected override void OnDrawTreeNode(Rect rect, TreeNode<MenuData> node, bool selected, bool focus)
		{
			GUIContent labelContent = new GUIContent(node.Data.menuName);

			if (!node.IsLeaf){
				float foldoutHeight = EditorStyles.foldout.CalcHeight(GUIContent.none, 12);
				float tweakYPos = rect.y + (rect.height - foldoutHeight) / 2;
				node.Data.isExpanded = EditorGUI.Foldout(new Rect(rect.x - 12, tweakYPos, 12, rect.height), node.Data.isExpanded, GUIContent.none);
			}

			GUIStyle style = new GUIStyle(PPMHelper.IsProSkin() ? EditorStyles.label : EditorStyles.whiteLabel)
			{
				alignment = TextAnchor.MiddleLeft
			};
			EditorGUI.LabelField(rect, labelContent, style);
		}
	}
}
