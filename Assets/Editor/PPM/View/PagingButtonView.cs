using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPM
{
	public class PagingButtonView : IPackageView
	{
		public delegate void PageSelect(int selectPage);
		private PageSelect _pageSelectCallback;

		private int _totalPageCount;

		private int _currentSelectPage;
		private int _leftPageIndex;
		private int _rightPageIndex;

		private int _buttonTopPadding;
		private int _buttonHeight;
		private int _buttonWidth;
		private int _maxShowPageCount;

		public PagingButtonView(PageSelect selectCallback)
		{
			_pageSelectCallback = selectCallback;

			_buttonTopPadding = 4;
			_buttonHeight = 30;
			_buttonWidth = 22;
			_maxShowPageCount = 5;

			ResetState();
		}

		public void UpdateTotalPageCount(int totalPageCount)
		{
			_totalPageCount = totalPageCount;

			if (IsNeedShowArrowButton && _rightPageIndex == -1)
			{
				_rightPageIndex = _maxShowPageCount;
			}
		}

		public void UpdateViewSettings(int buttonTopPadding, int buttonHeight, int buttonWidth, int maxShowPageCount)
		{
			_buttonTopPadding = buttonTopPadding;
			_buttonHeight = buttonHeight;
			_buttonWidth = buttonWidth;
			_maxShowPageCount = maxShowPageCount;
		}

		public int GetViewHeight()
		{
			return _buttonHeight;
		}

		public void DrawGUI(Rect uiRect, GUISkin uiSkin)
		{
			if (_totalPageCount <= 1)
            {
                return;
            }

			int showPageCount = Mathf.Min(_totalPageCount, _maxShowPageCount);

			float startX = uiRect.x + (uiRect.width - showPageCount * _buttonWidth) / 2;
			float startY = uiRect.y + uiRect.height - _buttonHeight + _buttonTopPadding;

			for (int i = 0; i < showPageCount; i++)
			{
				Rect rect = new Rect(startX + i * _buttonWidth, startY, _buttonWidth, _buttonHeight);

				GUIStyle style = "listPageBottomNormalButton";

				int currentPage = i + _leftPageIndex;
				if (_currentSelectPage == currentPage)
                {
                    style = "listPageBottomActiveButton";
                }

				if (UnityEngine.GUI.Button(rect, currentPage.ToString(), style))
				{
					ChangeCurrentPage(currentPage);
				}
			}

			if (IsNeedShowArrowButton)
            {
                DrawBottomArrowButton(startX, startY, _totalPageCount);
            }
		}

		private void DrawBottomArrowButton(float startX, float startY, int totalPageCount)
		{
			Rect leftArrowRect = new Rect(startX - _buttonWidth, startY, _buttonWidth, _buttonHeight);
			if (_currentSelectPage <= 1)
            {
                UnityEngine.GUI.enabled = false;
            }
			if (UnityEngine.GUI.Button(leftArrowRect, "◀", "listPageBottomArrowButton"))
			{
				ChangeCurrentPage(_currentSelectPage - 1);
			}
			UnityEngine.GUI.enabled = true;

			Rect rightArrowRect = new Rect(startX + _maxShowPageCount * _buttonWidth, startY, _buttonWidth, _buttonHeight);
			if (_currentSelectPage >= totalPageCount)
            {
                UnityEngine.GUI.enabled = false;
            }
			if (UnityEngine.GUI.Button(rightArrowRect, "▶", "listPageBottomArrowButton"))
			{
				ChangeCurrentPage(_currentSelectPage + 1);
			}
			UnityEngine.GUI.enabled = true;
		}

		private bool IsNeedShowArrowButton
		{
			get { return _totalPageCount > _maxShowPageCount; }
		}

		private void ResetState()
		{
			_currentSelectPage = 1;
			_leftPageIndex = 1;
			_rightPageIndex = -1;
		}

		private void ChangeCurrentPage(int changedPage)
		{
			_currentSelectPage = changedPage;

			if (IsNeedShowArrowButton)
			{
				// move left
				if (changedPage < _leftPageIndex)
				{
					_leftPageIndex--;
					_rightPageIndex--;
				}
				// move right
				else if (changedPage > _rightPageIndex)
				{
					_leftPageIndex++;
					_rightPageIndex++;
				}
			}

			if (_pageSelectCallback != null)
			{
				_pageSelectCallback(changedPage);
			}
		}

		public void Dispose()
		{
			_pageSelectCallback = null;
		}
	}
}
