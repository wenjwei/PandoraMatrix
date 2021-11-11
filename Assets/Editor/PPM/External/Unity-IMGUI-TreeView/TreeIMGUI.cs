using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/**
 * TreeNode.cs
 * Author: Luke Holland (http://lukeholland.me/)
 */

namespace PPM.TreeView {

	public class TreeIMGUI<T> where T : ITreeIMGUIData
	{

		private readonly TreeNode<T> _root;

		private Rect _controlRect;
		private float _drawY;
		private float _height;
		private TreeNode<T> _selected;
		private int _controlID;

		public event System.Action<T> NodeSelectCallback;

		public TreeIMGUI(TreeNode<T> root)
		{
			_root = root;
		}

		public void DrawTreeLayout(Rect controlRect)
		{
			_height = 0;
			_drawY = 0;
			_root.Traverse(OnGetLayoutHeight);

			_controlRect = controlRect;
			_controlID = GUIUtility.GetControlID(FocusType.Passive,_controlRect);
			_root.Traverse(OnDrawRow);
		}

		protected virtual float GetRowHeight(TreeNode<T> node)
		{
			// 顶级菜单项间距为普通菜单项两倍，UI体验更好
			return node.Level > 1 ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2f;
		}

		protected virtual bool OnGetLayoutHeight(TreeNode<T> node)
		{
			if(node.Data==null) return true;

			_height += GetRowHeight(node);
			return node.Data.isExpanded;
		}

		protected virtual bool OnDrawRow(TreeNode<T> node)
		{
			if(node.Data==null) return true;

			float rowIndent = 14*node.Level;
			float rowHeight = GetRowHeight(node);

			Rect rowRect = new Rect(0,_controlRect.y+_drawY,_controlRect.width,rowHeight);
			Rect indentRect = new Rect(rowIndent,_controlRect.y+_drawY,_controlRect.width-rowIndent,rowHeight);

			// render
			if(_selected==node){
				//EditorGUI.DrawRect(rowRect,Color.gray);
				EditorGUI.DrawRect(rowRect, new Color(33 / 255f, 105 / 255f, 156 / 255f));
			}

			OnDrawTreeNode(indentRect,node,_selected==node,false);

			// test for events
			EventType eventType = Event.current.GetTypeForControl(_controlID);
			if(eventType==EventType.MouseUp && rowRect.Contains(Event.current.mousePosition)){
				_selected = node;

				GUI.changed = true;
				Event.current.Use();

				if (NodeSelectCallback != null) NodeSelectCallback(node.Data);
			}

			_drawY += rowHeight;

			return node.Data.isExpanded;
		}

		protected virtual void OnDrawTreeNode(Rect rect, TreeNode<T> node, bool selected, bool focus)
		{
			GUIContent labelContent = new GUIContent(node.Data.ToString());

			if(!node.IsLeaf){
				node.Data.isExpanded = EditorGUI.Foldout(new Rect(rect.x-12,rect.y,12,rect.height),node.Data.isExpanded,GUIContent.none);
			}

			EditorGUI.LabelField(rect,labelContent,selected ? EditorStyles.whiteLabel : EditorStyles.label);
		}

		public void SelectNode(TreeNode<T> node)
		{
			_selected = node;

			if (NodeSelectCallback != null) NodeSelectCallback(node.Data);
		}
	}

	public interface ITreeIMGUIData
	{

		bool isExpanded { get; set; }

	}

}
