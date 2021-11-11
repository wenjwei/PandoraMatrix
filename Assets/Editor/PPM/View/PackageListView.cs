using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PPM
{
	public class PackageListView : IPackageView
	{
		private const int ItemNameHeight = 26;
		private const int ItemDescriptionHeight = 60;
		private const int ScrollBarRightPadding = 20;
		private readonly Color ItemActiveColor = new Color(33 / 255f, 105 / 255f, 156 / 255f);

		private Vector2 _scrollPosition = Vector2.zero;	
		private int _currentSelectItem;

		private List<PPMPackageInfo> _itemDataList;
		private PagingButtonView _pagingButtonView;

		public PackageListView()
		{
			_pagingButtonView = new PagingButtonView(PagingButtonSelect);
			PackageManager.Instance.OnListViewRefreshed += OnViewRefreshed;
		}

		private void PagingButtonSelect(int selectPage)
		{
			PackageManager.Instance.RequestData(selectPage);
		}

		public void OnViewRefreshed()
		{
			_itemDataList = PackageManager.Instance.GetPackageListViewListData();
			_pagingButtonView.UpdateTotalPageCount(PackageManager.Instance.GetTotalPageCount());
			ResetListViewState();
			PackageManager.Instance.SetCurrentPackageViewItem(CurrentSelectItem);
			PPMHelper.RefreshWindow<PackageManagerWindow>();
		}

		public void DrawGUI(Rect uiRect, GUISkin uiSkin)
		{
			Rect usedRect = DrawPackageListSortView();
			DrawPackageListView(new Rect(uiRect.x, uiRect.y + usedRect.height, uiRect.width, uiRect.height - usedRect.height));
		}

		private Rect DrawPackageListSortView()
		{
			DrawRect(PackageManagerUI.PackageListViewSortRect, PackageManagerUI.MenuViewBackgroundColor);

			if (PackageManager.Instance.GetSortOptionList() == null || PackageManager.Instance.GetSortOptionList().Count == 0)
				return PackageManagerUI.PackageListViewSortRect;

			Rect sortRect = PackageManagerUI.PackageListViewSortRect;
			UnityEngine.GUI.Label(sortRect, "排序依据:", "sortLabel");

			Rect sortOptionsRect = new Rect(sortRect.x + 60, sortRect.y, 160, sortRect.height);
			Rect sortOptionsMiddleRect = PPMHelper.ComputeControlMiddleDrawRect(sortOptionsRect, EditorStyles.popup);
			int currentIndex = PackageManager.Instance.GetCurrentSortSelectedIndex();
			EditorGUI.BeginChangeCheck();
			currentIndex = EditorGUI.Popup(sortOptionsMiddleRect, currentIndex, PackageManager.Instance.GetSortOptionList().ToArray());
			if (EditorGUI.EndChangeCheck())
			{
				PackageManager.Instance.SetCurrentSortSelectedIndex(currentIndex);
			}
			return PackageManagerUI.PackageListViewSortRect;
		}

		private Rect DrawRect(Rect drawRect, Color viewBackgroundColor)
		{
			GUIStyle style = PPMHelper.GenerateStyle(drawRect.width, drawRect.height, viewBackgroundColor);
			UnityEngine.GUI.Label(drawRect, GUIContent.none, style);
			return drawRect;
		}

		private void DrawPackageListView(Rect listViewRect)
		{
			DrawPackageList(listViewRect);
			_pagingButtonView.DrawGUI(listViewRect, null);
		}

		private void DrawPackageList(Rect listViewRect)
		{
			if (_itemDataList == null || _itemDataList.Count == 0)
				return;

			int itemHeight = ItemNameHeight + ItemDescriptionHeight;
			int pageHeight = itemHeight * _itemDataList.Count;

			_scrollPosition = UnityEngine.GUI.BeginScrollView(
				new Rect(listViewRect.x, listViewRect.y, listViewRect.width, listViewRect.height - _pagingButtonView.GetViewHeight()),
				_scrollPosition,
				new Rect(listViewRect.x, listViewRect.y, listViewRect.width - ScrollBarRightPadding, pageHeight));

			for (int i = 1; i <= _itemDataList.Count; i++)
			{
				Rect itemRect = new Rect(listViewRect.x, listViewRect.y + (i - 1) * itemHeight, listViewRect.width, itemHeight);

				if (i == _currentSelectItem)
					EditorGUI.DrawRect(itemRect, ItemActiveColor);

				if (UnityEngine.GUI.Button(itemRect, "", "listPageItem"))
				{
					_currentSelectItem = i;
					PackageManager.Instance.SetCurrentPackageViewItem(CurrentSelectItem);
				}

				Rect nameRect = new Rect(listViewRect.x, listViewRect.y + (i - 1) * itemHeight, listViewRect.width, ItemNameHeight);
				UnityEngine.GUI.Label(nameRect, _itemDataList[i - 1].name, "listViewName");
				Rect descriptionRect = new Rect(listViewRect.x, listViewRect.y + (i - 1) * itemHeight + ItemNameHeight, listViewRect.width, ItemDescriptionHeight);
				string description = _itemDataList[i - 1].description;
				GUIContent content = new GUIContent(
					description.Length > 60 ? description.Substring(0, 60) + "..." : description,
					description);
				UnityEngine.GUI.Label(descriptionRect, content, "listViewDescription");
			}

			UnityEngine.GUI.EndScrollView();
		}

		private PPMPackageInfo CurrentSelectItem
		{
			get
			{
				if (_itemDataList == null)
					return null;

				return _currentSelectItem > _itemDataList.Count ? null : _itemDataList[_currentSelectItem - 1];
			}
		}

		private void ResetListViewState()
		{
			_scrollPosition = Vector2.zero;
			_currentSelectItem = 1;
		}

		public void Dispose()
		{
			_pagingButtonView.Dispose();
			_pagingButtonView = null;

			PackageManager.Instance.OnListViewRefreshed -= OnViewRefreshed;
		}
	}
}
