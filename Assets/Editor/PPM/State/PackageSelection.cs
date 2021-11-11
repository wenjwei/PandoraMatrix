using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PPM
{
	public class PackageSelection
	{
		#region MenuView

		public PackageMenuItem CurrentMenuItem;

		public void SelectMenu(List<PackageMenuItem> menuItemList, MenuData menuData)
		{
			string menuFullName = menuData.fullMenuName;
			string tag = "";
			if (menuFullName.Contains("/"))
			{
				tag = menuData.menuName;
				menuFullName = menuFullName.Substring(0, menuFullName.IndexOf("/"));
			}

			if (menuFullName.Contains(" "))
			{
				menuFullName = menuFullName.Substring(0, menuFullName.IndexOf(" "));
			}

			foreach(var menuItem in menuItemList)
			{
				if (menuItem.MenuName.Equals(menuFullName))
				{
					CurrentMenuItem = menuItem;
					break;
				}
			}

			CurrentMenuItem.CurrentTag = tag;
			if (CurrentMenuItem.CurrentTag.Equals("全部"))
				CurrentMenuItem.CurrentTag = "";

			if (CurrentMenuItem.SortInfoList.Count > 0)
				CurrentMenuItem.CurrentSortInfo = CurrentMenuItem.SortInfoList[0];

			CurrentSortSelectedIndex = 0;
		}

		#endregion

		#region ListView

		public int CurrentSortSelectedIndex;

		public void SetCurrentSortSelectedIndex(int currentIndex)
		{
			CurrentSortSelectedIndex = currentIndex;
			CurrentMenuItem.CurrentSortInfo = CurrentMenuItem.SortInfoList[currentIndex];
		}

		public PPMPackageInfo CurrentPackageViewItem;

		#endregion

		#region DetailView

		public string CurrentSearchStr;

		#endregion
	}
}
