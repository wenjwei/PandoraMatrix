using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using cPackage.Package;

namespace PPM
{
	public class PackageDetailView : IPackageView
	{
		public const string SearchIconResourceName = "ppm_search_icon";
		public const string SearchBorderTexureResourceName = "ppm_textfield";
		public const int SearchIconWidth = 16;
		public const int SearchHistoryOptionWidth = 16;
		public const int SearchHistoryCount = 5;

		public string _newestVersion = "";
		private string _searchStr;
		private Texture _searchIcon;
		private PPMPackageInfo _currentPackageInfo;
		private PackageMenuItem _currentPackageMenuItem;

		private int _currentOwnPackageVersionIdx;

		public PackageDetailView()
		{
			_searchStr = "";
			_searchIcon = Resources.Load<Texture>(SearchIconResourceName);

			PackageManager.Instance.OnDetailViewRefreshed += OnViewRefreshed;
		}

		private void OnViewRefreshed()
		{
			_currentPackageInfo = PackageManager.Instance.GetCurrentPackageViewItem();
			_currentPackageMenuItem = PackageManager.Instance.GetCurrentMenuItem();
			_currentOwnPackageVersionIdx = 0;
		}

		public void DrawGUI(Rect uiRect, GUISkin uiSkin)
		{
			Rect usedRect = DrawPackageDetailSearchView(uiSkin);
			DrawDetailView(new Rect(uiRect.x, uiRect.y + usedRect.height, uiRect.width, uiRect.height - usedRect.height));
		}

		private Rect DrawPackageDetailSearchView(GUISkin uiSkin)
		{
			// search text field
			Rect searchRect = PackageManagerUI.PackageDetailViewSearchRect;
			Rect searchMiddleRect = PPMHelper.ComputeControlMiddleDrawRect(searchRect, uiSkin.textField);
			_searchStr = UnityEngine.GUI.TextField(searchMiddleRect, _searchStr);

			Event e = Event.current;
#if UNITY_2017_3_OR_NEWER
			if (e.type == EventType.KeyUp)//&& (e.keyCode == KeyCode.Return ||e.keyCode == KeyCode.KeypadEnter))
#else
			if (e.type == EventType.keyUp)// && (e.keyCode == KeyCode.Return ||e.keyCode == KeyCode.KeypadEnter))
#endif
			{
				PackageManager.Instance.SetCurrentSearchStr(_searchStr);
				//PPMHelper.RefreshWindow<PackageManagerWindow>();
			}

			// search icon
			Rect searchIconRect = new Rect(searchRect.x + searchRect.width - SearchIconWidth - 2, searchRect.y,
				SearchIconWidth, searchRect.height);
			Rect searchIconMiddleRect = PPMHelper.ComputeControlMiddleDrawRect(searchIconRect, uiSkin.textField);
			UnityEngine.GUI.DrawTexture(searchIconMiddleRect, _searchIcon, ScaleMode.ScaleToFit);

			return searchRect;
		}

		private void DrawDetailView(Rect detailViewRect)
		{
			if (_currentPackageInfo == null)
			{
				return;
			}

			GUILayout.BeginArea(detailViewRect);
			GUILayout.Space(15);

			switch (_currentPackageMenuItem.MenuItemEnum)
			{
				case EPackageMenuItem.Installed:
					{
						DrawHorizontalText("创建者", _currentPackageInfo.author);
						DrawHorizontalText("版本", GetDisplayVersion(_currentPackageInfo));
						DrawHorizontalText("评分", _currentPackageInfo.score);
						DrawHorizontalText("标签", _currentPackageInfo.labels);
						//DrawHorizontalText("插件详情", GetNewestReleaseInfo(_currentPackageInfo).changeLog, true);

						GUILayout.FlexibleSpace();

						if (GUILayout.Button("卸载", "detailViewButton") &&
							EditorUtility.DisplayDialog("插件管理器", string.Format("确定要卸载插件:{0}?", _currentPackageInfo.name), "确认", "取消"))
						{
							bool ret = PPMHelper.UninstallPackage(_currentPackageInfo);
							if (ret)
								PackageManager.Instance.PackageDelete(_currentPackageInfo.name, "Unity", GetNewestReleaseInfo(_currentPackageInfo).version, "client");
						}
						break;
					}
				case EPackageMenuItem.Network:
					{
						DrawHorizontalText("创建者", _currentPackageInfo.author);
						DrawHorizontalText("版本", GetDisplayVersion(_currentPackageInfo));
						DrawHorizontalText("版本日志", GetNewestReleaseInfo(_currentPackageInfo).changeLog, true, true);
						DrawHorizontalText("评分", _currentPackageInfo.score);
						DrawHorizontalText("标签", _currentPackageInfo.labels);
						//DrawHorizontalText("插件详情", GetNewestReleaseInfo(_currentPackageInfo).changeLog, true);
						DrawHorizontalText("上传时间", _currentPackageInfo.createTime, true, false);

						GUILayout.FlexibleSpace();

						if (PPMHelper.IsPackageAlreadyInstalled(_currentPackageInfo.name))
						{
							GUILayout.Button("插件已安装", "detailViewButton");
						}
						else
						{
							if (GUILayout.Button("下载", "detailViewButton"))
							{
								DownloadPackage(false);
							}
						}
						break;
					}
				case EPackageMenuItem.Update:
					{
						DrawHorizontalText("创建者", _currentPackageInfo.author);
						DrawHorizontalText("当前版本", GetDisplayVersion(_currentPackageInfo, false));
						DrawHorizontalText("新版本", GetDisplayVersion(_currentPackageInfo));
						//DrawHorizontalText("插件详情", GetNewestReleaseInfo(_currentPackageInfo).changeLog, true);

						GUILayout.FlexibleSpace();

						if (GUILayout.Button("更新", "detailViewButton"))
						{
							DownloadPackage(true);
						}
						break;
					}
				case EPackageMenuItem.Own:
					{
						List<string> displayOptions = new List<string>();
						foreach (var release in _currentPackageInfo.releases)
						{
							displayOptions.Add(release.version);
						}

						GUILayout.BeginHorizontal();
						GUIStyle style = "detailTextValue";
						style.wordWrap = true;
						GUILayout.Label("版本:", "detailTextDescription");
						_currentOwnPackageVersionIdx = EditorGUI.Popup(new Rect(60, 18, detailViewRect.width - 70, 30), _currentOwnPackageVersionIdx, displayOptions.ToArray());
						GUILayout.EndHorizontal();

						DrawHorizontalText("版本日志", _currentPackageInfo.releases[_currentOwnPackageVersionIdx].changeLog, true, false);
						GUILayout.FlexibleSpace();

						if (_currentPackageInfo.releases[_currentOwnPackageVersionIdx].publishState == 0)
						{
							if (GUILayout.Button("发布", "detailViewButton"))
							{
								PackageManager.Instance.PackagePublish(_currentPackageInfo.name,
									_currentPackageInfo.releases[_currentOwnPackageVersionIdx].version,
									1, _currentPackageInfo);
							}
						}
						else
						{
							if (GUILayout.Button("撤销发布", "detailViewButton"))
							{
								PackageManager.Instance.PackagePublish(_currentPackageInfo.name,
									_currentPackageInfo.releases[_currentOwnPackageVersionIdx].version,
									0, _currentPackageInfo);
							}
						}
						break;
					}
			}

			GUILayout.Space(15);
			GUILayout.EndArea();
		}

		private void DrawHorizontalLink(string url, string content)
		{
			if (string.IsNullOrEmpty(url))
			{
				return;
			}
			if (GUILayout.Button(content, "detailViewLinkButton"))
			{
				Application.OpenURL(url);
			}
		}

		private void DrawHorizontalText(string textDescription, string textValue)
		{
			DrawHorizontalText(textDescription, textValue, false, false);
		}

		private void DrawHorizontalText(string textDescription, string textValue, bool twoLineStyle, bool needOpenChangelogWnd)
		{
			if (string.IsNullOrEmpty(textValue))
			{
				return;
			}
			if (!twoLineStyle)
				GUILayout.BeginHorizontal();

			GUIStyle style = new GUIStyle("detailTextValue");
			style.wordWrap = true;
			style.alignment = TextAnchor.LowerLeft;
			GUILayout.Label(textDescription + ":", "detailTextDescription");

			if (textValue.Length > 150)
				textValue = textValue.Substring(0, 148) + "...";

			GUIContent content = new GUIContent(textValue, needOpenChangelogWnd ? "点击查阅更多" : null);
			GUILayout.Label(content, style);

			if (needOpenChangelogWnd)
			{
				Rect clickArea = GUILayoutUtility.GetLastRect();
				if (GUI.Button(clickArea, "", "listPageItem"))
				{
					PackageChangeLogWindow.OpenWindow(_currentPackageInfo);
				}
			}

			if (!twoLineStyle)
				GUILayout.EndHorizontal();
		}

		private string GetDisplayVersion(PPMPackageInfo packageInfo, bool useNewest = true)
		{
			if (packageInfo == null)
			{
				return "未知";
			}
			if (useNewest)
				return "v" + GetNewestReleaseInfo(packageInfo).version;
			else
				return "v" + GetOldestReleaseInfo(packageInfo).version;
		}

		private ReleaseInfo GetNewestReleaseInfo(PPMPackageInfo packageInfo)
		{
			return packageInfo.releases[packageInfo.releases.Count - 1];
		}

		private ReleaseInfo GetOldestReleaseInfo(PPMPackageInfo packageInfo)
		{
			return packageInfo.releases[0];
		}

		private void DownloadPackage(bool isUpdateOperation)
		{
			string packageName = _currentPackageInfo.name;
			string downloadPackageName = string.Format("{0}_v{1}.ppm", packageName, GetNewestReleaseInfo(_currentPackageInfo).version);
#if UNITY_EDITOR_OSX
            string packagePath = System.Environment.GetEnvironmentVariable("HOME") + @"/ppm/Unity/" + downloadPackageName;
#else
			string packagePath = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\ppm\Unity\" + downloadPackageName;
#endif

            string savePath = PPMHelper.GetCacheDirectory() + packageName;
			string tips = string.Format("确认{0}插件: {1} v{2}?", isUpdateOperation ? "更新" : "下载", packageName, GetNewestReleaseInfo(_currentPackageInfo).version);

			if (!EditorUtility.DisplayDialog("插件系统", tips, "确认", "取消"))
			{
				return;
			}

			float downloadStartTime = Time.realtimeSinceStartup;

			Action<string> requestCallback = (requestContent) =>
			{
				PPMHelper.Log(packagePath);
				if (!File.Exists(packagePath))
				{
					PPMHelper.LogError(string.Format("{0} 下载失败", packageName));
					return;
				}

				float downloadCostTime = Time.realtimeSinceStartup - downloadStartTime;
				PPMHelper.Log(string.Format("{0} 下载完成, 耗时: {1}ms", packageName, (downloadCostTime * 100).ToString("f2")));

				ThreadUtils.DelayedExecute(
					() =>
					{
						if (isUpdateOperation)
						{
							PPMConfiguration conf = PPMHelper.GetPackageConfiguration(packageName);
							if (conf.DisableAutoUpdate)
							{
								string decompressPackagePath = "";
								PPMHelper.DecompressPPM(packagePath, ref decompressPackagePath);
								PPMHelper.Log(string.Format("插件已下载到:{0}, 需要手动进行更新操作!", decompressPackagePath));
								EditorUtility.DisplayDialog("插件管理器", string.Format("插件已下载到:{0}, 需要手动进行更新操作!", decompressPackagePath), "ok");
								return;
							}

							if (!EditorUtility.DisplayDialog("插件管理器", string.Format("更新之前需要卸载插件:{0}旧版本?", _currentPackageInfo.name), "确认", "取消"))
								return;

							bool ret = PPMHelper.UninstallPackage(_currentPackageInfo);
							if (!ret)
							{
								PPMHelper.LogError(string.Format("插件更新失败:{0}", _currentPackageInfo.name));
								return;
							}
							PackageManager.Instance.PackageDelete(_currentPackageInfo.name, "Unity", GetOldestReleaseInfo(_currentPackageInfo).version, "client");
						}

						if (EditorUtility.DisplayDialog("插件系统", "确定导入插件:" + packageName, "确认", "取消"))
						{
							PPMHelper.CloseWindow<PackageManagerWindow>();
							PackageManager.Instance.ImportPackage(packagePath);
						}
					}, 0.01f);
			};

			Action<float> requestProgress = (progress) =>
			{
				EditorUtility.DisplayProgressBar("插件系统", "下载插件: " + _currentPackageInfo.name, progress);
			};

			PackageManager.Instance.PackageDownload(
				packageName,
				GetNewestReleaseInfo(_currentPackageInfo).version,
				savePath,
				requestProgress,
				requestCallback);
		}


		public void Dispose()
		{
			PackageManager.Instance.OnDetailViewRefreshed -= OnViewRefreshed;
		}
	}
}
