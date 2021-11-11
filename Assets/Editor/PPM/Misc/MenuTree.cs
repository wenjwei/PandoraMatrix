using System.Collections;
using UnityEditor;

using PPM.TreeView;

namespace PPM
{
	public class MenuTree 
	{
		private TreeNode<MenuData> _root;

		public MenuTree()
		{
			_root = new TreeNode<MenuData>(null);
		}

		public TreeNode<MenuData> Root { get { return _root; }}

		public void Clear()
		{
			_root.Clear();
		}

		public MenuData FindAsset(string menuName)
		{
			if (string.IsNullOrEmpty(menuName)) return null;

			MenuData md = new MenuData(menuName, "", false);
			return _root.FindInChildren(md).Data;
		}

		public void AddAsset(string fullMenuName)
		{
			if (string.IsNullOrEmpty(fullMenuName)) return;

			TreeNode<MenuData> node = _root;

			int startIndex = 0, length = fullMenuName.Length;
			while (startIndex < length)
			{
				int endIndex = fullMenuName.IndexOf('/', startIndex);
				int subLength = endIndex == -1 ? length - startIndex : endIndex - startIndex;
				string directory = fullMenuName.Substring(startIndex, subLength);

				MenuData pathNode = new MenuData(directory, fullMenuName.Substring(0, endIndex == -1 ? length : endIndex), node.Level == 0);

				TreeNode<MenuData> child = node.FindInChildren(pathNode);
				if (child == null) child = node.AddChild(pathNode);

				node = child;
				startIndex += subLength + 1;
			}
		}
	}

	public class MenuData : ITreeIMGUIData
	{
		public string menuName;
		public string fullMenuName;
		public bool isExpanded { get; set; }

		public MenuData(string menuName, string fullMenuName, bool isExpanded)
		{
			this.menuName = menuName;
			this.fullMenuName = fullMenuName;
			this.isExpanded = isExpanded;
		}

		public override string ToString ()
		{
			return menuName;
		}

		public override int GetHashCode ()
		{
			return menuName.GetHashCode()+10;
		}

		public override bool Equals (object obj)
		{
			MenuData node = obj as MenuData;
			return node!=null && node.menuName == menuName;
		}

		public bool Equals(MenuData node)
		{
			return node.menuName == menuName;
		}
	}
}
