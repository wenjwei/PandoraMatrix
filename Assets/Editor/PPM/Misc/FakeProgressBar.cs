using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PPM
{
	public static class FakeProgressBar
	{
		private static string _title;
		private static string _info;
		private static float _maxProgress;
		private static float _minProgress;
		private static float _curProgress;
		private static bool _isEnabled;

		public static void DisplayFakeProgressBar(string title, string info, float maxProgress = 1.0f, float minProgress = 0f)
		{
			EditorApplication.update += FakeProgressUpdate;
			_isEnabled = true;

			_title = title;
			_info = info;
			_maxProgress = maxProgress;
			_minProgress = minProgress;
			_curProgress = _minProgress;

			FakeProgressUpdate();
		}

		private static void FakeProgressUpdate()
		{
			if (!_isEnabled)
				return;

			_curProgress += 0.001f;
			if (_curProgress > _maxProgress)
				_curProgress = _minProgress;
			EditorUtility.DisplayProgressBar(_title, _info, _curProgress);
		}

		public static void ClearFakeProgressBar()
		{
			EditorApplication.update -= FakeProgressUpdate;
			_isEnabled = false;
			ThreadUtils.ExecuteOnNextFrame(() => { EditorUtility.ClearProgressBar(); });
		}
	}
}
