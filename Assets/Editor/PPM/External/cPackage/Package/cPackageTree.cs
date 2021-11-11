
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace cPackage.Package
{
	public enum cPackageTreeNodeState
	{
		Init = 0x00,
		None = 0x01,
		All = 0x10,
		Mix = 0x11,
	}

	public class ImportItem
	{
		public bool IsDir;
		public bool IsNew;
		public bool AssetChange;
		public bool MetaChange;
		public bool FileTypeChange;
		public string DestPath;
		public string ImportPath;
		public string ImportDir;
	}

	public abstract class cPackageTreeNode
	{
		#region view data

		public int ViewLevel;
		public bool ViewFoldout;
		public bool ViewSelect;

		#endregion

		#region node data

		public string FileName;
		public string FullPath;

		protected List<cPackageTreeNode> _childTreeNodeList;

		#endregion

		protected void ConstuctorInit()
		{
			_childTreeNodeList = new List<cPackageTreeNode>();
			ViewFoldout = true;
			ViewLevel = -2;
		}

		protected void Init(string fileName, string fullPath, int viewLevel)
		{
			FileName = fileName;
			FullPath = fullPath;

			ViewLevel = viewLevel;
			ViewSelect = CanSelectCurrentNode();
		}

		public virtual bool CanSelectCurrentNode() { return true; }

		public void TraverseNode(Action<cPackageTreeNode> traverseDelegate, bool ignoreFoldout = false)
		{
			traverseDelegate(this);

			if (ViewFoldout || ignoreFoldout)
			{
				foreach (var childTreeNode in _childTreeNodeList)
					childTreeNode.TraverseNode(traverseDelegate, ignoreFoldout);
			}	
		}

		public cPackageTreeNode FindChild(string fileName)
		{
			foreach (var childTreeNode in _childTreeNodeList)
			{
				if (childTreeNode.FileName.Equals(fileName))
					return childTreeNode;
			}
			return null;
		}

		public int GetChildCount()
		{
			return _childTreeNodeList.Count;
		}

		public bool IsLeafNode()
		{
			return _childTreeNodeList.Count == 0;
		}

		public cPackageTreeNodeState GetCurrentNodeState()
		{
			if (IsLeafNode())
				return ViewSelect ? cPackageTreeNodeState.All : cPackageTreeNodeState.None;

			cPackageTreeNodeState state = cPackageTreeNodeState.Init;
			foreach (var childTreeNode in _childTreeNodeList)
				state |= childTreeNode.GetCurrentNodeState();

			return state;
		}

		public void ToggleNodeSelectState(bool nodeSelectState)
		{
			if (CanSelectCurrentNode())
				ViewSelect = nodeSelectState;

			foreach (var childTreeNode in _childTreeNodeList)
				childTreeNode.ToggleNodeSelectState(nodeSelectState);
		}
	}

	public class cPackageExportTreeNode : cPackageTreeNode
	{
		public cPackageExportTreeNode()
		{
			ConstuctorInit();
		}

		public string Guid;

		public cPackageExportTreeNode AddChild(string guid, string fileName, string fullPath)
		{
			cPackageExportTreeNode node = new cPackageExportTreeNode();
			node.Guid = guid;
			node.Init(fileName, fullPath, ViewLevel + 1);
			_childTreeNodeList.Add(node);
			return node;
		}

		public List<string> GetSelectedGuids()
		{
			List<string> selectedGuids = new List<string>();

			if (ViewSelect && !string.IsNullOrEmpty(Guid))
				selectedGuids.Add(Guid);

			foreach (var childTreeNode in _childTreeNodeList)
				selectedGuids.AddRange((childTreeNode as cPackageExportTreeNode).GetSelectedGuids());

			return selectedGuids;
		}
	}

	public class cPackageImportTreeNode : cPackageTreeNode
	{
		public cPackageImportTreeNode()
		{
			ConstuctorInit();
		}

		public ImportItem ImportItem;

		public cPackageImportTreeNode AddChild(string fileName, string fullPath, ImportItem importItem)
		{
			cPackageImportTreeNode node = new cPackageImportTreeNode();
			node.ImportItem = importItem;
			node.Init(fileName, fullPath, ViewLevel + 1);
			_childTreeNodeList.Add(node);
			return node;
		}

		public override bool CanSelectCurrentNode()
		{
			return ImportItem != null ? (ImportItem.AssetChange || ImportItem.MetaChange) : true;
		}

		public List<ImportItem> GetSelectedImportItems()
		{
			List<ImportItem> selectedImportItems = new List<ImportItem>();

			if (ViewSelect && ImportItem != null)
				selectedImportItems.Add(ImportItem);

			foreach (var childTreeNode in _childTreeNodeList)
				selectedImportItems.AddRange((childTreeNode as cPackageImportTreeNode).GetSelectedImportItems());

			return selectedImportItems;
		}

		public bool HasItemToImport()
		{
			if (IsLeafNode())
				return ImportItem.AssetChange || ImportItem.MetaChange;

			bool hasItemToImport = false;
			foreach (var childTreeNode in _childTreeNodeList)
				hasItemToImport |= (childTreeNode as cPackageImportTreeNode).HasItemToImport();

			return hasItemToImport;
		}
	}

	public abstract class cPackageTree
	{
		protected cPackageTreeNode _root;

		public cPackageTreeNode GetRoot()
		{
			return _root;
		}

		public void SelectAllNodes()
		{
			_root.ToggleNodeSelectState(true);
		}

		public void DeselectAllNodes()
		{
			_root.ToggleNodeSelectState(false);
		}

		protected abstract cPackageTreeNode AddChild(cPackageTreeNode parentNode, string fileName, string fullPath, bool isLeaf);

		protected void AddAsset(string assetPath)
		{
			cPackageTreeNode node = _root;
			int startIndex = 0, length = assetPath.Length;
			while (startIndex < length)
			{
				int endIndex = assetPath.IndexOf('/', startIndex);
				int subLength = endIndex == -1 ? length - startIndex : endIndex - startIndex;
				string fileName = assetPath.Substring(startIndex, subLength);
				string fullPath = assetPath.Substring(0, endIndex == -1 ? length : endIndex);
				node = AddChild(node, fileName, fullPath, endIndex == -1);
				startIndex += subLength + 1;
			}
		}
	}

	public class cPackageExportTree : cPackageTree
	{
		public cPackageExportTree()
		{
			_root = new cPackageExportTreeNode();
		}

		public void SelectNodes(List<string> guidList)
		{
			foreach (var guid in guidList)
			{
				Action<cPackageTreeNode> a = (node) => 
				{
					cPackageExportTreeNode exportTreeNode = node as cPackageExportTreeNode;
					if (guid.Equals(exportTreeNode.Guid))
					{
						exportTreeNode.ViewSelect = true;
					}
				};
				_root.TraverseNode(a, true);
			}
		}

		public List<string> GetSelectedGuids()
		{
			return (_root as cPackageExportTreeNode).GetSelectedGuids();
		}

		protected override cPackageTreeNode AddChild(cPackageTreeNode parentNode, string fileName, string fullPath, bool isLeaf)
		{
			cPackageExportTreeNode node = parentNode as cPackageExportTreeNode;
			return node.FindChild(fileName) ?? node.AddChild(isLeaf ? _guid : null, fileName, fullPath);
		}

		private string _guid;

		public void AddExportAsset(string guid)
		{
			if (string.IsNullOrEmpty(guid))
				return;

			_guid = guid;

			AddAsset(AssetDatabase.GUIDToAssetPath(guid));
		}
	}

	public class cPackageImportTree : cPackageTree
	{
		public cPackageImportTree()
		{
			_root = new cPackageImportTreeNode();
		}

		public bool HasItemToImport()
		{
			return (_root as cPackageImportTreeNode).HasItemToImport();
		}

		public List<ImportItem> GetSelectedImportItems()
		{
			return (_root as cPackageImportTreeNode).GetSelectedImportItems();
		}

		protected override cPackageTreeNode AddChild(cPackageTreeNode parentNode, string fileName, string fullPath, bool isLeaf)
		{
			cPackageImportTreeNode node = parentNode as cPackageImportTreeNode;
			return node.FindChild(fileName) ?? node.AddChild(fileName, fullPath, isLeaf ? _importItem : null);
		}

		private ImportItem _importItem;

		public void AddImportAsset(ImportItem importItem)
		{
			if (importItem == null)
				return;

			_importItem = importItem;

			AddAsset(_importItem.DestPath);
		}
	}

	public abstract class cPackageTreeView
	{
		private cPackageTreeNode _root;

		public cPackageTreeView(cPackageTreeNode root)
		{
			_root = root;
			_foldoutWidth = 16;
			_toggleWidth = 16;
			_indentWidth = 16;
			_selectedColor = new Color(0.0f, 0.22f, 0.44f);
		}

		private Rect _windowRect;
		private float _height;
		private float _curYPosition;
		private int _controlID;
		private float _foldoutWidth;
		private float _toggleWidth;
		private float _indentWidth;
		private cPackageTreeNode _selected;
		private Color _selectedColor;

		public void Display()
		{
			_curYPosition = 0;
			_height = 0;
			_curYPosition = 0;

			Action<cPackageTreeNode> getHeight = (node) =>
			{
				_height += EditorGUIUtility.singleLineHeight;
			};
			_root.TraverseNode(getHeight);

			_windowRect = EditorGUILayout.GetControlRect(false, _height);
			_controlID = GUIUtility.GetControlID(FocusType.Passive, _windowRect);
			_root.TraverseNode(DrawRow);
		}

		protected Texture GetCachedIcon(string path)
		{
			return AssetDatabase.GetCachedIcon(path) ?? EditorGUIUtility.FindTexture("DefaultAsset Icon");
		}

		protected abstract GUIStyle GetContentStyle(cPackageTreeNode node);

		protected abstract void DrawToggle(cPackageTreeNode node, Rect toggleRect);

		protected abstract void DrawStatus(cPackageTreeNode node, Rect rowRect);

		protected void DrawContent(cPackageTreeNode node, Rect contentRect)
		{
			GUIContent content = new GUIContent(node.FileName, GetCachedIcon(node.FullPath));
			EditorGUI.LabelField(contentRect, content, GetContentStyle(node));
		}

		private void DrawRow(cPackageTreeNode node)
		{
			if (string.IsNullOrEmpty(node.FullPath) || node.FullPath == "Assets")
				return;

			float x = _windowRect.x + node.ViewLevel * _indentWidth;
			float y = _windowRect.y + _curYPosition;
			float height = EditorGUIUtility.singleLineHeight;

			Rect rowRect = new Rect(_windowRect.x, y, _windowRect.width, height);
			Rect foldoutRect = new Rect(x, y, _foldoutWidth, height);
			Rect toggleRect = new Rect(x + _foldoutWidth, y, _toggleWidth, height);
			Rect contentRect = new Rect(x + _foldoutWidth + _toggleWidth, y, _windowRect.width - x - _foldoutWidth - _toggleWidth, height);

			if (_selected == node)
			{
				EditorGUI.DrawRect(rowRect, _selectedColor);
			}

			if (!node.IsLeafNode())
				node.ViewFoldout = EditorGUI.Foldout(foldoutRect, node.ViewFoldout, "");

			DrawToggle(node, toggleRect);
			DrawContent(node, contentRect);
			DrawStatus(node, rowRect);

			_curYPosition += EditorGUIUtility.singleLineHeight;

			EventType eventType = Event.current.GetTypeForControl(_controlID);
			if (eventType == EventType.MouseUp && rowRect.Contains(Event.current.mousePosition))
			{
				_selected = node;

				GUI.changed = true;
				Event.current.Use();
			}
		}
	}

	public class cPackageExportTreeView : cPackageTreeView
	{
		public cPackageExportTreeView(cPackageTreeNode root) : base(root) { }

		protected override void DrawToggle(cPackageTreeNode node, Rect toggleRect)
		{
			EditorGUI.BeginChangeCheck();
			cPackageTreeNodeState state = node.GetCurrentNodeState();
			bool isSelect = state > cPackageTreeNodeState.None;
			GUIStyle style = EditorStyles.toggle;
			if (state == cPackageTreeNodeState.Mix)
				style = "ToggleMixed";

			bool selectRet = EditorGUI.Toggle(toggleRect, isSelect, style);
			if (EditorGUI.EndChangeCheck())
			{
				node.ToggleNodeSelectState(selectRet);
			}
		}

		protected override GUIStyle GetContentStyle(cPackageTreeNode node)
		{
			cPackageExportTreeNode n = node as cPackageExportTreeNode;
			GUIStyle s = new GUIStyle(EditorStyles.label);
			if (string.IsNullOrEmpty(n.Guid))
				s.normal.textColor = Color.gray;
			return s;
		}

		protected override void DrawStatus(cPackageTreeNode node, Rect rowRect) { }
	}

	public class cPackageImportTreeView : cPackageTreeView
	{
		public cPackageImportTreeView(cPackageTreeNode root) : base(root) { }

		protected override void DrawToggle(cPackageTreeNode node, Rect toggleRect)
		{
			cPackageImportTreeNode n = node as cPackageImportTreeNode;
			if (n.ImportItem != null && (n.ImportItem.AssetChange || n.ImportItem.MetaChange))
			{
				EditorGUI.BeginChangeCheck();
				cPackageTreeNodeState state = node.GetCurrentNodeState();
				bool isSelect = state > cPackageTreeNodeState.None;
				GUIStyle style = EditorStyles.toggle;
				if (state == cPackageTreeNodeState.Mix)
					style = "ToggleMixed";

				bool selectRet = EditorGUI.Toggle(toggleRect, isSelect, style);
				if (EditorGUI.EndChangeCheck())
				{
					node.ToggleNodeSelectState(selectRet);
				}
			}
		}

		protected override GUIStyle GetContentStyle(cPackageTreeNode node)
		{
			cPackageImportTreeNode n = node as cPackageImportTreeNode;
			GUIStyle s = new GUIStyle(EditorStyles.label);
			if (n.ImportItem == null)
				s.normal.textColor = Color.gray;
			return s;
		}

		protected override void DrawStatus(cPackageTreeNode node, Rect rowRect)
		{
			cPackageImportTreeNode n = node as cPackageImportTreeNode;
			if (n.ImportItem == null)
				return;
			if (n.ImportItem.IsNew)
			{
				Rect labelRect = new Rect(rowRect.xMax - 38, rowRect.y, 50, rowRect.height);
				GUI.Label(labelRect, new GUIContent("New"));
			}
			else if (n.ImportItem.AssetChange || n.ImportItem.MetaChange)
			{
				Rect labelRect = new Rect(rowRect.xMax - 48, rowRect.y, 50, rowRect.height);
				GUI.Label(labelRect, new GUIContent("Update"));
			}
		}
	}
}
