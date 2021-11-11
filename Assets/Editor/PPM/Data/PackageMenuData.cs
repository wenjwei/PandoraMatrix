using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PPM
{
	public enum EPackageMenuItem
	{
		Installed,
		Network,
		Update,
        Own
	}

	public class SortInfo
	{
		public string SortLabel;
		public ViewSortType SortType;
	}

	public class PackageMenuItem
	{
		public EPackageMenuItem MenuItemEnum;
		public string MenuName;
		public List<string> TagList;
		public List<SortInfo> SortInfoList;

		public string CurrentTag;
		public SortInfo CurrentSortInfo;

		public PackageMenuItem(EPackageMenuItem packageMenuItem)
		{
			MenuItemEnum = packageMenuItem;
			TagList = new List<string>();
			SortInfoList = new List<SortInfo>();

			switch (MenuItemEnum)
			{
				case EPackageMenuItem.Installed:
					MenuName = "已安装";
					break;
				case EPackageMenuItem.Network:
					MenuName = "网络";
					break;
				case EPackageMenuItem.Update:
					MenuName = "更新";
					break;
                case EPackageMenuItem.Own:
                    MenuName = "我的";
                    break;
                default:
					MenuName = "未分类";
					break;
			}
		}

		public void AddTag(string tag)
		{
			if (!string.IsNullOrEmpty(tag) && !TagList.Contains(tag))
            {
                TagList.Add(tag);
            }
		}

		public void AddSortInfo(SortInfo sortInfo)
		{
			if (sortInfo != null && !SortInfoList.Contains(sortInfo))
            {
                SortInfoList.Add(sortInfo);
            }
		}

		public List<string> GetSortOptionList()
		{
			List<string> optionList = new List<string>();
			foreach(var sortInfo in SortInfoList)
			{
				optionList.Add(sortInfo.SortLabel);
			}
			return optionList;
		}

		public List<string> GetMenuList()
		{
			List<string> menuList = new List<string>();
			menuList.Add(MenuName);
			foreach(var tag in TagList)
			{
				menuList.Add(string.Format("{0}/{1}", MenuName, tag));
			}
			return menuList;
		}

		private bool IsAllTagMatched()
		{
			return string.IsNullOrEmpty(CurrentTag) || CurrentTag.Equals("全部");
		}

		public bool IsTagMatched(string requiredTags)
		{
			if (IsAllTagMatched())
			{
				return true;
			}
			else
			{
				string[] tags = requiredTags.Split(' ', ',');
				foreach (var tag in tags)
				{
					if (CurrentTag.Equals(tag))
                    {
                        return true;
                    }
				}
				return false;
			}
		}
	}

	public class PackageMenuData
	{
		public PackageMenuData()
		{
			InitMenuItems();
		}

		public List<PackageMenuItem> MenuItemList = new List<PackageMenuItem>();

		private void InitMenuItems()
		{

			PackageMenuItem installedMenuItem = new PackageMenuItem(EPackageMenuItem.Installed);
			installedMenuItem.AddTag("全部");
			installedMenuItem.AddTag("SDK");
			installedMenuItem.AddTag("工具");
			installedMenuItem.AddTag("模版");


			SortInfo networkNewestSortInfo = new SortInfo()
			{
				SortLabel = "最近",
				SortType = ViewSortType.DESC
			};
			SortInfo networkEarliestSortInfo = new SortInfo()
			{
				SortLabel = "最早",
				SortType = ViewSortType.ASC
			};

			PackageMenuItem networkMenuItem = new PackageMenuItem(EPackageMenuItem.Network);
			networkMenuItem.AddTag("全部");
			networkMenuItem.AddTag("SDK");
			networkMenuItem.AddTag("工具");
			networkMenuItem.AddTag("模版");
			networkMenuItem.AddSortInfo(networkNewestSortInfo);
			networkMenuItem.AddSortInfo(networkEarliestSortInfo);

			PackageMenuItem updateMenuItem = new PackageMenuItem(EPackageMenuItem.Update);

            PackageMenuItem OwnMenuItem = new PackageMenuItem(EPackageMenuItem.Own);

            MenuItemList.Add(installedMenuItem);
			MenuItemList.Add(networkMenuItem);
			MenuItemList.Add(updateMenuItem);
            MenuItemList.Add(OwnMenuItem);
        }
	}
}